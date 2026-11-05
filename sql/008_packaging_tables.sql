-- 008_packaging_tables.sql
-- CCS / SQL Server. Creates PackAndShip operational tables only.
-- Run AFTER 001_pallet_configuration.sql and the user/role tables.
-- Does NOT change Q-Wall tables or insert production records.
-- IMPORTANT: table constraints alone cannot enforce Q-Wall PASS or
-- complete levels/containers. Implement transactional stored procedures
-- before enabling writes or the Complete Pallet button in WinForms.
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- One physical container. Configuration is saved as a reference and
    -- its required counts are snapshotted into individual ContainerLevels.
    IF OBJECT_ID(N'dbo.ssi_PaS_Containers', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_Containers
        (
            container_id INT IDENTITY(1,1) NOT NULL,
            container_code VARCHAR(18) NOT NULL,
            configuration_id INT NOT NULL,
            status VARCHAR(12) NOT NULL
                CONSTRAINT DF_ssi_PaS_Containers_Status DEFAULT ('OPEN'),
            created_by_user_id INT NOT NULL,
            created_at DATETIME2(3) NOT NULL
                CONSTRAINT DF_ssi_PaS_Containers_Created DEFAULT SYSUTCDATETIME(),
            closed_by_user_id INT NULL,
            closed_at DATETIME2(3) NULL,
            CONSTRAINT PK_ssi_PaS_Containers PRIMARY KEY (container_id),
            CONSTRAINT UQ_ssi_PaS_Containers_Code UNIQUE (container_code),
            CONSTRAINT FK_ssi_PaS_Containers_Configuration
                FOREIGN KEY (configuration_id)
                REFERENCES dbo.ssi_PaS_PalletConfigurations(configuration_id),
            CONSTRAINT FK_ssi_PaS_Containers_CreatedBy
                FOREIGN KEY (created_by_user_id)
                REFERENCES dbo.ssi_PaS_Users(user_id),
            CONSTRAINT FK_ssi_PaS_Containers_ClosedBy
                FOREIGN KEY (closed_by_user_id)
                REFERENCES dbo.ssi_PaS_Users(user_id),
            CONSTRAINT CK_ssi_PaS_Containers_Status
                CHECK (status IN ('OPEN','CLOSED','CANCELLED')),
            CONSTRAINT CK_ssi_PaS_Containers_Closure
                CHECK (
                    (status = 'CLOSED' AND closed_at IS NOT NULL AND closed_by_user_id IS NOT NULL)
                    OR
                    (status <> 'CLOSED' AND closed_at IS NULL AND closed_by_user_id IS NULL)
                ),
            CONSTRAINT CK_ssi_PaS_Containers_Code
                CHECK (LEN(LTRIM(RTRIM(container_code))) > 0
                       AND container_code = LTRIM(RTRIM(container_code)))
        );
    END;

    -- Snapshot of the configuration's levels at the moment the container
    -- is opened. Later Setup edits must not change a container in progress.
    IF OBJECT_ID(N'dbo.ssi_PaS_ContainerLevels', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_ContainerLevels
        (
            container_level_id INT IDENTITY(1,1) NOT NULL,
            container_id INT NOT NULL,
            level_number INT NOT NULL,
            required_pieces INT NOT NULL,
            status VARCHAR(12) NOT NULL
                CONSTRAINT DF_ssi_PaS_ContainerLevels_Status DEFAULT ('OPEN'),
            closed_at DATETIME2(3) NULL,
            closed_by_user_id INT NULL,
            CONSTRAINT PK_ssi_PaS_ContainerLevels PRIMARY KEY (container_level_id),
            CONSTRAINT FK_ssi_PaS_ContainerLevels_Container
                FOREIGN KEY (container_id)
                REFERENCES dbo.ssi_PaS_Containers(container_id),
            CONSTRAINT FK_ssi_PaS_ContainerLevels_ClosedBy
                FOREIGN KEY (closed_by_user_id)
                REFERENCES dbo.ssi_PaS_Users(user_id),
            CONSTRAINT UQ_ssi_PaS_ContainerLevels_Position
                UNIQUE (container_id, level_number),
            CONSTRAINT CK_ssi_PaS_ContainerLevels_Number
                CHECK (level_number > 0),
            CONSTRAINT CK_ssi_PaS_ContainerLevels_Quantity
                CHECK (required_pieces > 0),
            CONSTRAINT CK_ssi_PaS_ContainerLevels_Status
                CHECK (status IN ('OPEN','CLOSED')),
            CONSTRAINT CK_ssi_PaS_ContainerLevels_Closure
                CHECK (
                    (status = 'CLOSED' AND closed_at IS NOT NULL AND closed_by_user_id IS NOT NULL)
                    OR
                    (status = 'OPEN' AND closed_at IS NULL AND closed_by_user_id IS NULL)
                )
        );
    END;

    -- A Q-Wall product is assigned to one and only one level anywhere
    -- in PackAndShip. UNIQUE(product_id) enforces this across all
    -- containers even when multiple stations insert concurrently.
    IF OBJECT_ID(N'dbo.ssi_PaS_ContainerPieces', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_ContainerPieces
        (
            container_piece_id BIGINT IDENTITY(1,1) NOT NULL,
            container_level_id INT NOT NULL,
            product_id INT NOT NULL,
            verified_inspection_id INT NOT NULL,
            scanned_by_user_id INT NOT NULL,
            scanned_at DATETIME2(3) NOT NULL
                CONSTRAINT DF_ssi_PaS_ContainerPieces_Scanned DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_ssi_PaS_ContainerPieces PRIMARY KEY (container_piece_id),
            CONSTRAINT UQ_ssi_PaS_ContainerPieces_Product UNIQUE (product_id),
            CONSTRAINT FK_ssi_PaS_ContainerPieces_Level
                FOREIGN KEY (container_level_id)
                REFERENCES dbo.ssi_PaS_ContainerLevels(container_level_id),
            CONSTRAINT FK_ssi_PaS_ContainerPieces_Product
                FOREIGN KEY (product_id)
                REFERENCES dbo.ssi_Products(product_id),
            CONSTRAINT FK_ssi_PaS_ContainerPieces_Inspection
                FOREIGN KEY (verified_inspection_id)
                REFERENCES dbo.ssi_Inspections(inspection_id),
            CONSTRAINT FK_ssi_PaS_ContainerPieces_User
                FOREIGN KEY (scanned_by_user_id)
                REFERENCES dbo.ssi_PaS_Users(user_id)
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_ssi_PaS_ContainerPieces_Level'
                     AND object_id = OBJECT_ID(N'dbo.ssi_PaS_ContainerPieces'))
        CREATE INDEX IX_ssi_PaS_ContainerPieces_Level
            ON dbo.ssi_PaS_ContainerPieces(container_level_id);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_ssi_PaS_ContainerLevels_ContainerStatus'
                     AND object_id = OBJECT_ID(N'dbo.ssi_PaS_ContainerLevels'))
        CREATE INDEX IX_ssi_PaS_ContainerLevels_ContainerStatus
            ON dbo.ssi_PaS_ContainerLevels(container_id, status);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT t.name AS table_name, c.name AS column_name, ty.name AS data_type,
       c.max_length, c.is_nullable
FROM sys.tables AS t
JOIN sys.columns AS c ON c.object_id = t.object_id
JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN (N'ssi_PaS_Containers',
                 N'ssi_PaS_ContainerLevels',
                 N'ssi_PaS_ContainerPieces')
ORDER BY t.name, c.column_id;

-- Required next step (NOT implemented by this schema):
-- A single transaction for piece insertion must lock the target container
-- and level; verify status OPEN, model identity, active Q-Wall PASS (including
-- flags/scrap), remaining capacity, and INSERT into ContainerPieces.
-- Level closure must check COUNT(pieces)=required_pieces under locks.
-- Container closure must check ALL levels CLOSED and their exact counts.
-- A SQL FK only proves inspection existence, NOT that it belongs to the
-- product or that its result is PASS: the transaction must check that too.
-- Direct DML rights to these tables should be restricted in production.
