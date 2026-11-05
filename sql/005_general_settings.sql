-- Shared PackAndShip General Settings (run once on CCS).
-- Global across customers and Windows workstations.
-- Safe to rerun: does not overwrite existing configuration.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.ssi_PaS_GeneralSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_GeneralSettings
    (
        settings_id     int NOT NULL CONSTRAINT PK_ssi_PaS_GeneralSettings PRIMARY KEY
                         CONSTRAINT CK_ssi_PaS_GeneralSettings_singleton CHECK (settings_id = 1),
        pallet_prefix   varchar(5) NOT NULL
                         CONSTRAINT CK_ssi_PaS_GeneralSettings_prefix
                         CHECK (pallet_prefix COLLATE Latin1_General_BIN2 NOT LIKE '%[^A-Z]%'
                                AND LEN(pallet_prefix) BETWEEN 1 AND 5),
        pallet_digits   int NOT NULL
                         CONSTRAINT CK_ssi_PaS_GeneralSettings_digits CHECK (pallet_digits BETWEEN 1 AND 12),
        updated_at      datetime2(3) NOT NULL CONSTRAINT DF_ssi_PaS_GeneralSettings_updated DEFAULT SYSUTCDATETIME(),
        updated_by_user_id int NULL
                         CONSTRAINT FK_ssi_PaS_GeneralSettings_Users
                         FOREIGN KEY REFERENCES dbo.ssi_PaS_Users(user_id)
    );
END;

BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_GeneralSettings WITH (UPDLOCK, HOLDLOCK) WHERE settings_id=1)
BEGIN
    INSERT INTO dbo.ssi_PaS_GeneralSettings
        (settings_id, pallet_prefix, pallet_digits)
    VALUES (1, 'P', 6);
END;
COMMIT;

SELECT settings_id, pallet_prefix, pallet_digits, updated_at, updated_by_user_id
FROM dbo.ssi_PaS_GeneralSettings
WHERE settings_id=1;
