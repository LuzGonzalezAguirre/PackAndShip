-- 016_offline_import_identity.sql
-- Run on CCS after 014_security_audit_and_corrections.sql.
-- Durable idempotent local-id mapping prevents duplicate pallet creation
-- after an ambiguous network timeout (server committed, client never heard).
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO
IF OBJECT_ID(N'dbo.ssi_PaS_OfflineImports',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_OfflineImports
    (
        local_id CHAR(32) NOT NULL,
        container_id INT NOT NULL,
        created_by_user_id INT NOT NULL,
        imported_at DATETIME2(3) NOT NULL
            CONSTRAINT DF_ssi_PaS_OfflineImports_Imported DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ssi_PaS_OfflineImports PRIMARY KEY(local_id),
        CONSTRAINT UQ_ssi_PaS_OfflineImports_Container UNIQUE(container_id),
        CONSTRAINT FK_ssi_PaS_OfflineImports_Container FOREIGN KEY(container_id)
            REFERENCES dbo.ssi_PaS_Containers(container_id),
        CONSTRAINT FK_ssi_PaS_OfflineImports_Actor FOREIGN KEY(created_by_user_id)
            REFERENCES dbo.ssi_PaS_Users(user_id)
    );
END;
GO
CREATE OR ALTER PROCEDURE dbo.ssi_PaS_OfflineOpen
    @local_id CHAR(32),
    @container_code VARCHAR(18),
    @pn_id INT,
    @actor_user_id INT,
    @server_container_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @existing_id INT,@existing_code VARCHAR(18),@existing_part INT;
    DECLARE @results TABLE(container_id INT, container_code VARCHAR(18),pn_id INT,
                           level_count INT,required_total INT,status VARCHAR(12));
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @local_id IS NULL OR LEN(RTRIM(@local_id))<>32 OR
           @local_id COLLATE Latin1_General_BIN2 LIKE '%[^0-9a-f]%'
            THROW 51601,'Invalid offline local ID.',1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users WITH (HOLDLOCK)
            WHERE user_id=@actor_user_id AND is_active=1
        ) THROW 51602,'Offline actor is inactive or unknown.',1;

        SELECT @existing_id=m.container_id,@existing_code=c.container_code,
               @existing_part=cfg.pn_id
        FROM dbo.ssi_PaS_OfflineImports m WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.ssi_PaS_Containers c ON c.container_id=m.container_id
        JOIN dbo.ssi_PaS_PalletConfigurations cfg ON cfg.configuration_id=c.configuration_id
        WHERE m.local_id=@local_id;

        IF @existing_id IS NOT NULL
        BEGIN
            IF @existing_code<>@container_code OR @existing_part<>@pn_id
                THROW 51603,'Offline ID was previously associated with another pallet.',1;
            SET @server_container_id=@existing_id;
            COMMIT TRANSACTION;
            SELECT @existing_id AS container_id;
            RETURN;
        END;

        -- Existing unrelated pallet code is a hard conflict. Never reuse it.
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)
            WHERE container_code=@container_code
        ) THROW 51604,'Pallet ID already exists in CCS. Manual review required.',1;

        INSERT @results
            EXEC dbo.ssi_PaS_OpenContainer
                @container_code=@container_code,
                @pn_id=@pn_id,
                @actor_user_id=@actor_user_id;

        SELECT TOP (1) @existing_id=container_id FROM @results;
        IF @existing_id IS NULL THROW 51605,'Could not open offline pallet.',1;

        INSERT dbo.ssi_PaS_OfflineImports(local_id,container_id,created_by_user_id)
        VALUES(@local_id,@existing_id,@actor_user_id);

        SET @server_container_id=@existing_id;
        COMMIT TRANSACTION;
        SELECT @existing_id AS container_id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Runtime role is created by 015_runtime_permissions.sql.
IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NOT NULL
BEGIN
    GRANT EXECUTE ON dbo.ssi_PaS_OfflineOpen TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_OfflineImports TO ssi_PaS_RuntimeRole;
    DENY INSERT,UPDATE,DELETE ON dbo.ssi_PaS_OfflineImports TO ssi_PaS_RuntimeRole;
END;
GO
SELECT name,modify_date FROM sys.procedures WHERE name=N'ssi_PaS_OfflineOpen';
