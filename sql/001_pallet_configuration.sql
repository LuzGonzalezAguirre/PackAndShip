-- PackAndShip - Pallet Configuration (one configuration per pn_id)
-- Execute manually in SQL Server after reviewing. No existing Q-Wall tables are changed.
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.ssi_PaS_PalletConfigurations', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_PalletConfigurations
        (
            configuration_id INT IDENTITY(1,1) NOT NULL,
            pn_id INT NOT NULL,
            total_pieces INT NOT NULL,
            is_active BIT NOT NULL CONSTRAINT DF_ssi_PaS_PalletConfigurations_Active DEFAULT (1),
            created_at DATETIME2(3) NOT NULL CONSTRAINT DF_ssi_PaS_PalletConfigurations_Created DEFAULT (SYSDATETIME()),
            updated_at DATETIME2(3) NOT NULL CONSTRAINT DF_ssi_PaS_PalletConfigurations_Updated DEFAULT (SYSDATETIME()),
            CONSTRAINT PK_ssi_PaS_PalletConfigurations PRIMARY KEY (configuration_id),
            CONSTRAINT UQ_ssi_PaS_PalletConfigurations_Part UNIQUE (pn_id),
            CONSTRAINT FK_ssi_PaS_PalletConfigurations_Part
                FOREIGN KEY (pn_id) REFERENCES dbo.ssi_PartNumbers(pn_id),
            CONSTRAINT CK_ssi_PaS_PalletConfigurations_TotalPositive CHECK (total_pieces > 0)
        );
    END;

    IF OBJECT_ID(N'dbo.ssi_PaS_PalletLevels', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_PalletLevels
        (
            level_id INT IDENTITY(1,1) NOT NULL,
            configuration_id INT NOT NULL,
            level_number INT NOT NULL,
            required_pieces INT NOT NULL,
            CONSTRAINT PK_ssi_PaS_PalletLevels PRIMARY KEY (level_id),
            CONSTRAINT FK_ssi_PaS_PalletLevels_Configuration
                FOREIGN KEY (configuration_id)
                REFERENCES dbo.ssi_PaS_PalletConfigurations(configuration_id),
            CONSTRAINT UQ_ssi_PaS_PalletLevels_ConfigurationLevel
                UNIQUE (configuration_id, level_number),
            CONSTRAINT CK_ssi_PaS_PalletLevels_LevelPositive CHECK (level_number > 0),
            CONSTRAINT CK_ssi_PaS_PalletLevels_PiecesPositive CHECK (required_pieces > 0)
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- Structural verification (no writes).
SELECT t.name AS TableName, c.name AS ColumnName, ty.name AS DataType
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN (N'ssi_PaS_PalletConfigurations', N'ssi_PaS_PalletLevels')
ORDER BY t.name, c.column_id;

-- When saving a configuration, validate within the SAME SQL transaction:
-- SUM(ssi_PaS_PalletLevels.required_pieces) = ssi_PaS_PalletConfigurations.total_pieces
-- and level numbers are consecutive starting at 1. These are cross-row rules;
-- they are not enforced by the simple CHECK constraints above.
