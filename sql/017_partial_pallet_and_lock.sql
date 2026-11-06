-- 017_partial_pallet_and_lock.sql
-- CCS: partial pallet close requires a second, non-Operator role.
-- Run after 014_security_audit_and_corrections.sql.
-- Existing OPEN/CLOSED/CANCELLED pallets and their pieces are preserved.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id=OBJECT_ID(N'dbo.ssi_PaS_Containers')
          AND name=N'CK_ssi_PaS_Containers_Status'
    ) THROW 51700, 'Expected container status constraint missing. Review schema.', 1;
    ALTER TABLE dbo.ssi_PaS_Containers DROP CONSTRAINT CK_ssi_PaS_Containers_Status;
    ALTER TABLE dbo.ssi_PaS_Containers
        ADD CONSTRAINT CK_ssi_PaS_Containers_Status
        CHECK (status IN ('OPEN','CLOSED','CANCELLED','PARTIAL'));

    ALTER TABLE dbo.ssi_PaS_Containers DROP CONSTRAINT CK_ssi_PaS_Containers_Closure;
    ALTER TABLE dbo.ssi_PaS_Containers
        ADD CONSTRAINT CK_ssi_PaS_Containers_Closure CHECK (
            (status IN ('CLOSED','PARTIAL') AND closed_at IS NOT NULL
                AND closed_by_user_id IS NOT NULL)
            OR (status NOT IN ('CLOSED','PARTIAL') AND closed_at IS NULL
                AND closed_by_user_id IS NULL)
        );

    ALTER TABLE dbo.ssi_PaS_PackingAudit DROP CONSTRAINT CK_ssi_PaS_PackingAudit_Event;
    ALTER TABLE dbo.ssi_PaS_PackingAudit
        ADD CONSTRAINT CK_ssi_PaS_PackingAudit_Event CHECK (
            event_type IN ('OPEN_CONTAINER','ADD_PIECE','REMOVE_PIECE',
                           'CLOSE_LEVEL','CLOSE_CONTAINER','CANCEL_CONTAINER',
                           'CLOSE_PARTIAL')
        );

    IF OBJECT_ID(N'dbo.ssi_PaS_PartialClosures',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_PartialClosures
        (
            container_id INT NOT NULL PRIMARY KEY,
            authorized_by_user_id INT NOT NULL,
            packing_user_id INT NOT NULL,
            actual_pieces INT NOT NULL,
            required_pieces INT NOT NULL,
            reason NVARCHAR(500) NOT NULL,
            authorized_at DATETIME2(3) NOT NULL
                CONSTRAINT DF_ssi_PaS_PartialClosures_AuthorizedAt DEFAULT SYSUTCDATETIME(),
            CONSTRAINT FK_ssi_PaS_PartialClosures_Container
                FOREIGN KEY(container_id) REFERENCES dbo.ssi_PaS_Containers(container_id),
            CONSTRAINT FK_ssi_PaS_PartialClosures_AuthorizedBy
                FOREIGN KEY(authorized_by_user_id) REFERENCES dbo.ssi_PaS_Users(user_id),
            CONSTRAINT FK_ssi_PaS_PartialClosures_PackingUser
                FOREIGN KEY(packing_user_id) REFERENCES dbo.ssi_PaS_Users(user_id),
            CONSTRAINT CK_ssi_PaS_PartialClosures_Counts
                CHECK (actual_pieces > 0 AND required_pieces >= actual_pieces),
            CONSTRAINT CK_ssi_PaS_PartialClosures_Reason
                CHECK (LEN(LTRIM(RTRIM(reason))) >= 10)
        );
    END;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_ClosePartialContainer
    @container_id INT,
    @packing_user_id INT,
    @authorized_by_user_id INT,
    @authorized_barcode VARCHAR(50),
    @authorized_password VARCHAR(50),
    @reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @actual INT, @required INT, @note NVARCHAR(500)=LTRIM(RTRIM(ISNULL(@reason,N'')));
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users
            WHERE user_id=@packing_user_id AND is_active=1
        ) THROW 51701,'Current packing user is not active.',1;
        IF @authorized_by_user_id=@packing_user_id
            THROW 51702,'A second non-operator account must authorize the partial pallet.',1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users u
            INNER JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id
            WHERE u.user_id=@authorized_by_user_id AND u.is_active=1
              AND u.barcode_id=@authorized_barcode
              AND u.password_hash=HASHBYTES('SHA2_256',CAST(@authorized_password AS VARCHAR(50)))
              AND LOWER(LTRIM(RTRIM(r.role_name))) NOT LIKE '%operator%'
              AND LOWER(LTRIM(RTRIM(r.role_name))) NOT LIKE '%operador%'
        ) THROW 51703,'Invalid authorizer credentials or Operator role is not permitted.',1;
        IF LEN(@note)<10 THROW 51704,'Provide a reason of at least 10 characters.',1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)
            WHERE container_id=@container_id AND status='OPEN'
        ) THROW 51705,'Only an OPEN pallet can be closed as partial.',1;

        SELECT @required=SUM(required_pieces)
        FROM dbo.ssi_PaS_ContainerLevels WITH (UPDLOCK,HOLDLOCK)
        WHERE container_id=@container_id;
        SELECT @actual=COUNT(*)
        FROM dbo.ssi_PaS_ContainerPieces p WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.ssi_PaS_ContainerLevels l WITH (UPDLOCK,HOLDLOCK)
            ON l.container_level_id=p.container_level_id
        WHERE l.container_id=@container_id;

        IF @required IS NULL OR @actual IS NULL OR @actual=0
            THROW 51706,'A partial pallet must have at least one recorded piece.',1;
        IF @actual>=@required
            THROW 51707,'This pallet is full. Close normally instead of partial.',1;

        UPDATE dbo.ssi_PaS_Containers
        SET status='PARTIAL',closed_at=SYSUTCDATETIME(),
            closed_by_user_id=@authorized_by_user_id
        WHERE container_id=@container_id AND status='OPEN';
        IF @@ROWCOUNT<>1 THROW 51708,'Pallet changed during closure.',1;

        INSERT dbo.ssi_PaS_PartialClosures
            (container_id,authorized_by_user_id,packing_user_id,
             actual_pieces,required_pieces,reason)
        VALUES (@container_id,@authorized_by_user_id,@packing_user_id,
                @actual,@required,@note);
        INSERT dbo.ssi_PaS_PackingAudit
            (event_type,container_id,actor_user_id,reason)
        VALUES ('CLOSE_PARTIAL',@container_id,@authorized_by_user_id,@note);
        COMMIT TRANSACTION;
        SELECT @container_id AS container_id,'PARTIAL' AS status,
               @actual AS actual_pieces,@required AS required_pieces;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NOT NULL
BEGIN
    GRANT EXECUTE ON dbo.ssi_PaS_ClosePartialContainer TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_PartialClosures TO ssi_PaS_RuntimeRole;
    DENY INSERT,UPDATE,DELETE ON dbo.ssi_PaS_PartialClosures TO ssi_PaS_RuntimeRole;
END;
GO

SELECT name,modify_date FROM sys.procedures
WHERE name=N'ssi_PaS_ClosePartialContainer';
