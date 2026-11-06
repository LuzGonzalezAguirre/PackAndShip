-- 015_runtime_permissions.sql
-- OPTIONAL deployment hardening: does not assign any SQL login automatically.
-- Execute on CCS after 014. Review your application's SQL principal first.
-- SQL sysadmin/db_owner bypass role DENY; use a dedicated non-owner SQL login.
SET NOCOUNT ON;
GO
IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NULL
    CREATE ROLE ssi_PaS_RuntimeRole AUTHORIZATION dbo;
GO
-- GUI and Q-Wall integration require read access.
GRANT SELECT ON dbo.ssi_PaS_Containers TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PaS_ContainerLevels TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PaS_ContainerPieces TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PaS_PackingAudit TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PaS_PalletConfigurations TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PaS_PalletLevels TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PaS_Users TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PaS_Roles TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_Products TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_Inspections TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PieceFlagRecords TO ssi_PaS_RuntimeRole;
GRANT SELECT ON dbo.ssi_PartNumbers TO ssi_PaS_RuntimeRole;
GO
GRANT EXECUTE ON dbo.ssi_PaS_OpenContainer TO ssi_PaS_RuntimeRole;
GRANT EXECUTE ON dbo.ssi_PaS_AddPiece TO ssi_PaS_RuntimeRole;
GRANT EXECUTE ON dbo.ssi_PaS_CloseLevel TO ssi_PaS_RuntimeRole;
GRANT EXECUTE ON dbo.ssi_PaS_CloseContainer TO ssi_PaS_RuntimeRole;
GRANT EXECUTE ON dbo.ssi_PaS_RemovePiece TO ssi_PaS_RuntimeRole;
GRANT EXECUTE ON dbo.ssi_PaS_CancelContainer TO ssi_PaS_RuntimeRole;
GO
-- Prevent application login from evading audit and validations by writing
-- to packaging tables. Procedures share dbo ownership and retain access.
DENY INSERT, UPDATE, DELETE ON dbo.ssi_PaS_Containers TO ssi_PaS_RuntimeRole;
DENY INSERT, UPDATE, DELETE ON dbo.ssi_PaS_ContainerLevels TO ssi_PaS_RuntimeRole;
DENY INSERT, UPDATE, DELETE ON dbo.ssi_PaS_ContainerPieces TO ssi_PaS_RuntimeRole;
DENY INSERT, UPDATE, DELETE ON dbo.ssi_PaS_PackingAudit TO ssi_PaS_RuntimeRole;
GO
-- MANUAL step, after confirming the dedicated runtime principal can still
-- authenticate and access other required Setup/Q-Wall tables:
-- ALTER ROLE ssi_PaS_RuntimeRole ADD MEMBER [YOUR_RUNTIME_DATABASE_USER];
-- NEVER add dbo, db_owner, sysadmin or an administrator account to this role.
-- This script deliberately does not alter any user's role membership.
SELECT name,type_desc FROM sys.database_principals
WHERE name=N'ssi_PaS_RuntimeRole';
