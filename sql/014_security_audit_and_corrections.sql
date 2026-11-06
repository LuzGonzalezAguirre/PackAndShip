-- 014_security_audit_and_corrections.sql
-- Run on CCS AFTER 013_authorized_quality_flags.sql.
-- Adds an append-only action log and privileged, reason-required corrections.
-- Preserves Q-Wall. No historical containers/pieces are modified by this deployment.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ssi_PaS_PackingAudit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_PackingAudit
    (
        audit_id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        event_type VARCHAR(30) NOT NULL,
        container_id INT NOT NULL,
        container_piece_id BIGINT NULL,
        container_level_id INT NULL,
        product_id INT NULL,
        inspection_id INT NULL,
        actor_user_id INT NOT NULL,
        reason NVARCHAR(500) NULL,
        event_at DATETIME2(3) NOT NULL
            CONSTRAINT DF_ssi_PaS_PackingAudit_EventAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ssi_PaS_PackingAudit_Container
            FOREIGN KEY (container_id) REFERENCES dbo.ssi_PaS_Containers(container_id),
        CONSTRAINT FK_ssi_PaS_PackingAudit_Actor
            FOREIGN KEY (actor_user_id) REFERENCES dbo.ssi_PaS_Users(user_id),
        CONSTRAINT CK_ssi_PaS_PackingAudit_Event
            CHECK (event_type IN ('OPEN_CONTAINER','ADD_PIECE','REMOVE_PIECE',
                                  'CLOSE_LEVEL','CLOSE_CONTAINER','CANCEL_CONTAINER'))
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ssi_PaS_PackingAudit')
               AND name=N'IX_ssi_PaS_PackingAudit_Container')
    CREATE INDEX IX_ssi_PaS_PackingAudit_Container
        ON dbo.ssi_PaS_PackingAudit(container_id,event_at DESC,audit_id DESC);
GO

-- Record standard operations even when they are issued by existing SQL procedures.
-- For added pieces, use the source data's scanned_by_user_id.
CREATE OR ALTER TRIGGER dbo.trg_ssi_PaS_AuditContainerInsert
ON dbo.ssi_PaS_Containers AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ssi_PaS_PackingAudit(event_type,container_id,actor_user_id)
    SELECT 'OPEN_CONTAINER',container_id,created_by_user_id FROM inserted;
END;
GO
CREATE OR ALTER TRIGGER dbo.trg_ssi_PaS_AuditPieceInsert
ON dbo.ssi_PaS_ContainerPieces AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ssi_PaS_PackingAudit
        (event_type,container_id,container_piece_id,container_level_id,
         product_id,inspection_id,actor_user_id)
    SELECT 'ADD_PIECE',l.container_id,i.container_piece_id,i.container_level_id,
           i.product_id,i.verified_inspection_id,i.scanned_by_user_id
    FROM inserted AS i
    JOIN dbo.ssi_PaS_ContainerLevels AS l ON l.container_level_id=i.container_level_id;
END;
GO
CREATE OR ALTER TRIGGER dbo.trg_ssi_PaS_AuditLevelClose
ON dbo.ssi_PaS_ContainerLevels AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ssi_PaS_PackingAudit(event_type,container_id,container_level_id,actor_user_id)
    SELECT 'CLOSE_LEVEL',i.container_id,i.container_level_id,i.closed_by_user_id
    FROM inserted i JOIN deleted d ON d.container_level_id=i.container_level_id
    WHERE i.status='CLOSED' AND d.status<>'CLOSED';
END;
GO
CREATE OR ALTER TRIGGER dbo.trg_ssi_PaS_AuditContainerClose
ON dbo.ssi_PaS_Containers AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ssi_PaS_PackingAudit(event_type,container_id,actor_user_id)
    SELECT 'CLOSE_CONTAINER',i.container_id,i.closed_by_user_id
    FROM inserted i JOIN deleted d ON d.container_id=i.container_id
    WHERE i.status='CLOSED' AND d.status<>'CLOSED';
END;
GO

-- Only a current active setup-authorized user can correct a piece.
-- The piece must still belong to an OPEN level in an OPEN container.
-- The removed row is preserved in the audit including original product,
-- inspection and its immutable historical container/level reference.
CREATE OR ALTER PROCEDURE dbo.ssi_PaS_RemovePiece
    @container_id INT, @product_id INT, @actor_user_id INT, @reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @piece_id BIGINT, @level_id INT, @inspection_id INT;
    DECLARE @note NVARCHAR(500)=LTRIM(RTRIM(ISNULL(@reason,N'')));
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users u
            JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id
            WHERE u.user_id=@actor_user_id AND u.is_active=1 AND r.can_modify_setup=1
        ) THROW 51401,'An active authorized Setup user is required to remove a piece.',1;
        IF LEN(@note)<10 THROW 51402,'Correction reason must be at least 10 characters.',1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)
            WHERE container_id=@container_id AND status='OPEN'
        ) THROW 51403,'Only OPEN containers can be corrected.',1;

        SELECT @piece_id=p.container_piece_id,
               @level_id=p.container_level_id,
               @inspection_id=p.verified_inspection_id
        FROM dbo.ssi_PaS_ContainerPieces p WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.ssi_PaS_ContainerLevels l WITH (UPDLOCK,HOLDLOCK)
            ON l.container_level_id=p.container_level_id
        WHERE l.container_id=@container_id AND p.product_id=@product_id
          AND l.status='OPEN';

        IF @piece_id IS NULL THROW 51404,'Piece not found in an OPEN level of this pallet.',1;

        INSERT dbo.ssi_PaS_PackingAudit
            (event_type,container_id,container_piece_id,container_level_id,
             product_id,inspection_id,actor_user_id,reason)
        VALUES ('REMOVE_PIECE',@container_id,@piece_id,@level_id,
                @product_id,@inspection_id,@actor_user_id,@note);

        DELETE dbo.ssi_PaS_ContainerPieces WHERE container_piece_id=@piece_id;
        IF @@ROWCOUNT<>1 THROW 51405,'Piece correction failed; no changes were saved.',1;
        COMMIT TRANSACTION;
        SELECT @container_id AS container_id,@product_id AS removed_product_id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- Cancellation is irreversible and allowed ONLY when every level is empty;
-- operators must correct each piece with a reason before cancelling.
-- Existing snapshots, configuration, users and audit remain.
CREATE OR ALTER PROCEDURE dbo.ssi_PaS_CancelContainer
    @container_id INT, @actor_user_id INT, @reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @note NVARCHAR(500)=LTRIM(RTRIM(ISNULL(@reason,N'')));
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users u
            JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id
            WHERE u.user_id=@actor_user_id AND u.is_active=1 AND r.can_modify_setup=1
        ) THROW 51501,'An active authorized Setup user is required to cancel a pallet.',1;
        IF LEN(@note)<10 THROW 51502,'Cancellation reason must be at least 10 characters.',1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)
            WHERE container_id=@container_id AND status='OPEN'
        ) THROW 51503,'Only OPEN pallets can be cancelled.',1;
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_ContainerLevels
            WHERE container_id=@container_id AND status='CLOSED'
        ) THROW 51504,'A pallet with closed levels cannot be cancelled.',1;
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_ContainerPieces p
            JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=p.container_level_id
            WHERE l.container_id=@container_id
        ) THROW 51505,'Remove all packed pieces with reasons before cancelling.',1;

        UPDATE dbo.ssi_PaS_Containers SET status='CANCELLED'
        WHERE container_id=@container_id AND status='OPEN';
        IF @@ROWCOUNT<>1 THROW 51506,'Could not cancel pallet.',1;
        INSERT dbo.ssi_PaS_PackingAudit(event_type,container_id,actor_user_id,reason)
        VALUES ('CANCEL_CONTAINER',@container_id,@actor_user_id,@note);
        COMMIT TRANSACTION;
        SELECT @container_id AS container_id,'CANCELLED' AS status;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
SELECT name,modify_date FROM sys.procedures
WHERE name IN (N'ssi_PaS_RemovePiece',N'ssi_PaS_CancelContainer');
