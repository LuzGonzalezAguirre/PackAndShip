-- 019_authorized_empty_pallet_cancellation.sql
-- Run in CCS after 017_partial_pallet_and_lock.sql.
-- Cancel an OPEN, EMPTY pallet with independent non-operator authorization.
-- Never delete containers or pieces; keep code reservation and full audit.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO
IF OBJECT_ID(N'dbo.ssi_PaS_EmptyPalletCancellations',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_EmptyPalletCancellations
    (
        container_id INT NOT NULL CONSTRAINT PK_ssi_PaS_EmptyPalletCancellations PRIMARY KEY,
        packing_user_id INT NOT NULL,
        authorized_by_user_id INT NOT NULL,
        reason NVARCHAR(500) NOT NULL,
        cancelled_at DATETIME2(3) NOT NULL
            CONSTRAINT DF_ssi_PaS_EmptyPalletCancellations_Date DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ssi_PaS_EmptyPalletCancellations_Container
            FOREIGN KEY(container_id) REFERENCES dbo.ssi_PaS_Containers(container_id),
        CONSTRAINT FK_ssi_PaS_EmptyPalletCancellations_Packing
            FOREIGN KEY(packing_user_id) REFERENCES dbo.ssi_PaS_Users(user_id),
        CONSTRAINT FK_ssi_PaS_EmptyPalletCancellations_Authorizer
            FOREIGN KEY(authorized_by_user_id) REFERENCES dbo.ssi_PaS_Users(user_id),
        CONSTRAINT CK_ssi_PaS_EmptyPalletCancellations_Reason
            CHECK (LEN(LTRIM(RTRIM(reason)))>=10)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_CancelEmptyPalletAuthorized
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
    DECLARE @note NVARCHAR(500)=LTRIM(RTRIM(ISNULL(@reason,N'')));
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users
            WHERE user_id=@packing_user_id AND is_active=1
        ) THROW 51901,'The current packing user is inactive.',1;

        IF @authorized_by_user_id=@packing_user_id
            THROW 51902,'A different user must authorize cancellation.',1;

        IF @authorized_password IS NULL OR @authorized_barcode IS NULL OR
           NOT EXISTS (
              SELECT 1 FROM dbo.ssi_PaS_Users u
              INNER JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id
              WHERE u.user_id=@authorized_by_user_id
                AND u.is_active=1
                AND u.barcode_id=@authorized_barcode
                AND u.password_hash=HASHBYTES('SHA2_256',CAST(@authorized_password AS VARCHAR(50)))
                AND LOWER(LTRIM(RTRIM(r.role_name))) NOT LIKE '%operator%'
                AND LOWER(LTRIM(RTRIM(r.role_name))) NOT LIKE '%operador%'
           )
           THROW 51903,'Invalid authorization or Operator role not permitted.',1;

        IF LEN(@note)<10
            THROW 51904,'Cancellation reason requires at least 10 characters.',1;

        IF NOT EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)
            WHERE container_id=@container_id AND status='OPEN'
        ) THROW 51905,'Only OPEN pallets may be cancelled.',1;

        IF EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_ContainerLevels WITH (UPDLOCK,HOLDLOCK)
            WHERE container_id=@container_id AND status='CLOSED'
        ) THROW 51906,'Pallet has closed levels and cannot be cancelled.',1;

        IF EXISTS(
            SELECT 1
            FROM dbo.ssi_PaS_ContainerPieces p WITH (UPDLOCK,HOLDLOCK)
            INNER JOIN dbo.ssi_PaS_ContainerLevels l WITH (UPDLOCK,HOLDLOCK)
                ON l.container_level_id=p.container_level_id
            WHERE l.container_id=@container_id
        ) THROW 51907,'Pallet already contains pieces; use authorized partial closure.',1;

        UPDATE dbo.ssi_PaS_Containers
        SET status='CANCELLED'
        WHERE container_id=@container_id AND status='OPEN';

        IF @@ROWCOUNT<>1
            THROW 51908,'Pallet cancellation failed.',1;

        INSERT dbo.ssi_PaS_EmptyPalletCancellations
            (container_id,packing_user_id,authorized_by_user_id,reason)
        VALUES(@container_id,@packing_user_id,@authorized_by_user_id,@note);

        INSERT dbo.ssi_PaS_PackingAudit
            (event_type,container_id,actor_user_id,reason)
        VALUES('CANCEL_CONTAINER',@container_id,@authorized_by_user_id,@note);

        COMMIT TRANSACTION;
        SELECT @container_id AS container_id,'CANCELLED' AS status;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NOT NULL
BEGIN
    GRANT EXECUTE ON dbo.ssi_PaS_CancelEmptyPalletAuthorized TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_EmptyPalletCancellations TO ssi_PaS_RuntimeRole;
    DENY INSERT,UPDATE,DELETE ON dbo.ssi_PaS_EmptyPalletCancellations TO ssi_PaS_RuntimeRole;
END;
GO
SELECT name,modify_date FROM sys.procedures
WHERE name=N'ssi_PaS_CancelEmptyPalletAuthorized';
