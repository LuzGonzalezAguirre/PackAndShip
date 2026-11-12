-- 023_direct_sql_runtime_permissions.sql
-- EJECUTAR UNA SOLA VEZ EN CCS con una cuenta administradora, nunca desde la tablet.
-- PRECONDICION: esquema existente 008..022, respaldos y pruebas.
-- NO crea procedimientos, triggers ni tablas. No concede db_owner ni escritura en Q-Wall.
-- Advertencia: con permisos DML directos, una cuenta mal utilizada puede escribir fuera
-- de la aplicación. No equivale al control de acceso de los procedimientos.
USE CCS;
GO
SET NOCOUNT ON;
IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NULL
    THROW 52301,'El rol ssi_PaS_RuntimeRole no existe.',1;
GO
-- Los DENY previos del rol 015 bloquean DML directo aunque existan GRANT.
-- REVOKE retira esos DENY/GRANT explícitos y permite otorgar exactamente lo necesario.
REVOKE INSERT, UPDATE, DELETE ON OBJECT::dbo.ssi_PaS_Containers FROM ssi_PaS_RuntimeRole;
REVOKE INSERT, UPDATE, DELETE ON OBJECT::dbo.ssi_PaS_ContainerLevels FROM ssi_PaS_RuntimeRole;
REVOKE INSERT, UPDATE, DELETE ON OBJECT::dbo.ssi_PaS_ContainerPieces FROM ssi_PaS_RuntimeRole;
REVOKE INSERT, UPDATE, DELETE ON OBJECT::dbo.ssi_PaS_PackingAudit FROM ssi_PaS_RuntimeRole;
GO

-- Solo tablas operativas de PackAndShip, sin escritura en Q-Wall.
GRANT SELECT, INSERT, UPDATE ON OBJECT::dbo.ssi_PaS_Containers TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT, UPDATE ON OBJECT::dbo.ssi_PaS_ContainerLevels TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.ssi_PaS_ContainerPieces TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_PackingAudit TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_PartialClosures TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_EmptyPalletCancellations TO ssi_PaS_RuntimeRole;
GO
GRANT SELECT, INSERT, UPDATE ON OBJECT::dbo.ssi_PaS_ScanRejections TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_ScanRejectionEvents TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_RejectionRequirements TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_RejectionApprovals TO ssi_PaS_RuntimeRole;
GRANT SELECT, UPDATE ON OBJECT::dbo.ssi_PaS_RejectionDepartments TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT, DELETE ON OBJECT::dbo.ssi_PaS_RejectionDepartmentRoles TO ssi_PaS_RuntimeRole;
GO
GRANT SELECT, INSERT, UPDATE ON OBJECT::dbo.ssi_PaS_PalletActionRequests TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_PalletActionRequirements TO ssi_PaS_RuntimeRole;
GRANT SELECT, INSERT ON OBJECT::dbo.ssi_PaS_PalletActionApprovals TO ssi_PaS_RuntimeRole;
GO
GRANT SELECT ON OBJECT::dbo.ssi_PaS_GeneralSettings TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_PaS_PalletConfigurations TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_PaS_PalletLevels TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_Products TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_Inspections TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_PieceFlagRecords TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_PartNumbers TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_PaS_Users TO ssi_PaS_RuntimeRole;
GRANT SELECT ON OBJECT::dbo.ssi_PaS_Roles TO ssi_PaS_RuntimeRole;
GO

-- No se agrega automáticamente ninguna cuenta a este rol.
-- Si la tablet usa un principal propio, el administrador debe asignar esa cuenta
-- después de comprobar su nombre y que no tenga un DENY heredado adicional.
SELECT dp.name,dp.type_desc
FROM sys.database_principals dp
WHERE dp.name=N'ssi_PaS_RuntimeRole';
GO
