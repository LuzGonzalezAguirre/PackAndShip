-- 020_scan_rejection_authorizations.sql
-- Ejecutar en CCS despues de 019. Conserva usuarios, roles, Q-Wall y pallets existentes.
-- Una aprobacion desbloquea el escaner, NUNCA permite insertar una pieza rechazada.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ssi_PaS_RejectionDepartments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_RejectionDepartments(
        department_id INT NOT NULL PRIMARY KEY,
        department_name NVARCHAR(60) NOT NULL UNIQUE,
        is_required BIT NOT NULL CONSTRAINT DF_PaS_RejDept_Required DEFAULT(1)
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments WHERE department_id=1)
    INSERT dbo.ssi_PaS_RejectionDepartments(department_id,department_name,is_required) VALUES(1,N'Manufactura',1);
IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments WHERE department_id=2)
    INSERT dbo.ssi_PaS_RejectionDepartments(department_id,department_name,is_required) VALUES(2,N'Calidad',1);
IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments WHERE department_id=3)
    INSERT dbo.ssi_PaS_RejectionDepartments(department_id,department_name,is_required) VALUES(3,N'Producción',1);
GO

IF OBJECT_ID(N'dbo.ssi_PaS_RejectionDepartmentRoles',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_RejectionDepartmentRoles(
        department_id INT NOT NULL REFERENCES dbo.ssi_PaS_RejectionDepartments(department_id),
        role_id INT NOT NULL REFERENCES dbo.ssi_PaS_Roles(role_id),
        CONSTRAINT PK_PaS_RejectionDepartmentRoles PRIMARY KEY(department_id,role_id)
    );
    -- Semilla SOLO en la instalacion inicial. Posteriores cambios quedan administrados por Setup.
    INSERT dbo.ssi_PaS_RejectionDepartmentRoles(department_id,role_id)
    SELECT 1,role_id FROM dbo.ssi_PaS_Roles WHERE role_id IN (3,9);
    INSERT dbo.ssi_PaS_RejectionDepartmentRoles(department_id,role_id)
    SELECT 2,role_id FROM dbo.ssi_PaS_Roles WHERE role_id IN (6,7,8);
    INSERT dbo.ssi_PaS_RejectionDepartmentRoles(department_id,role_id)
    SELECT 3,role_id FROM dbo.ssi_PaS_Roles WHERE role_id IN (2,4,5);
END;
GO

IF OBJECT_ID(N'dbo.ssi_PaS_ScanRejections',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_ScanRejections(
        rejection_id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        container_id INT NOT NULL REFERENCES dbo.ssi_PaS_Containers(container_id),
        rejection_type VARCHAR(30) NOT NULL,
        ssi_scan NVARCHAR(200) NOT NULL,
        client_scan NVARCHAR(200) NULL,
        rejection_reason NVARCHAR(1000) NOT NULL,
        detected_by_user_id INT NOT NULL REFERENCES dbo.ssi_PaS_Users(user_id),
        detected_at DATETIME2(3) NOT NULL CONSTRAINT DF_PaS_Rej_Detected DEFAULT SYSUTCDATETIME(),
        status VARCHAR(12) NOT NULL CONSTRAINT DF_PaS_Rej_Status DEFAULT('BLOCKED'),
        resolved_at DATETIME2(3) NULL,
        CONSTRAINT CK_PaS_Rej_Status CHECK (status IN ('BLOCKED','APPROVED'))
    );
    CREATE UNIQUE INDEX UX_PaS_Rej_OneActive
        ON dbo.ssi_PaS_ScanRejections(container_id) WHERE status='BLOCKED';
END;
GO

IF OBJECT_ID(N'dbo.ssi_PaS_RejectionRequirements',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_RejectionRequirements(
        rejection_id BIGINT NOT NULL REFERENCES dbo.ssi_PaS_ScanRejections(rejection_id),
        department_id INT NOT NULL REFERENCES dbo.ssi_PaS_RejectionDepartments(department_id),
        CONSTRAINT PK_PaS_RejectionRequirements PRIMARY KEY(rejection_id,department_id)
    );
END;
GO

IF OBJECT_ID(N'dbo.ssi_PaS_RejectionApprovals',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_RejectionApprovals(
        approval_id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        rejection_id BIGINT NOT NULL REFERENCES dbo.ssi_PaS_ScanRejections(rejection_id),
        department_id INT NOT NULL,
        authorized_by_user_id INT NOT NULL REFERENCES dbo.ssi_PaS_Users(user_id),
        approved_at DATETIME2(3) NOT NULL CONSTRAINT DF_PaS_Rej_Approved DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PaS_Rej_Requirement FOREIGN KEY(rejection_id,department_id)
            REFERENCES dbo.ssi_PaS_RejectionRequirements(rejection_id,department_id),
        CONSTRAINT UQ_PaS_Rej_ApprovalDepartment UNIQUE(rejection_id,department_id),
        CONSTRAINT UQ_PaS_Rej_ApprovalPerson UNIQUE(rejection_id,authorized_by_user_id)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_StartScanRejection
    @container_id INT,
    @actor_user_id INT,
    @rejection_type VARCHAR(30),
    @ssi_scan NVARCHAR(200),
    @client_scan NVARCHAR(200)=NULL,
    @reason NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @id BIGINT;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_Containers WITH(UPDLOCK,HOLDLOCK)
                       WHERE container_id=@container_id AND status='OPEN')
            THROW 52001,'Solo se registran rechazos en pallets abiertos.',1;
        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_Users
                       WHERE user_id=@actor_user_id AND is_active=1)
            THROW 52002,'Se requiere un usuario activo.',1;
        SELECT @id=rejection_id FROM dbo.ssi_PaS_ScanRejections WITH(UPDLOCK,HOLDLOCK)
        WHERE container_id=@container_id AND status='BLOCKED';
        IF @id IS NULL
        BEGIN
            INSERT dbo.ssi_PaS_ScanRejections
                (container_id,rejection_type,ssi_scan,client_scan,rejection_reason,detected_by_user_id)
            VALUES (@container_id,@rejection_type,COALESCE(@ssi_scan,N''),
                    @client_scan,LEFT(@reason,1000),@actor_user_id);
            SET @id=SCOPE_IDENTITY();
            INSERT dbo.ssi_PaS_RejectionRequirements(rejection_id,department_id)
            SELECT @id,department_id FROM dbo.ssi_PaS_RejectionDepartments WHERE is_required=1;
            -- Si no hay departamentos activos, mantener bloqueado para revision de Setup.
        END;
        COMMIT TRANSACTION;
        SELECT @id AS rejection_id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_ApproveScanRejection
    @rejection_id BIGINT,
    @department_id INT,
    @barcode VARCHAR(50),
    @password VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @user_id INT, @role_id INT, @detector INT;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @detector=detected_by_user_id
        FROM dbo.ssi_PaS_ScanRejections WITH(UPDLOCK,HOLDLOCK)
        WHERE rejection_id=@rejection_id AND status='BLOCKED';
        IF @detector IS NULL THROW 52011,'El rechazo no esta pendiente.',1;
        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionRequirements
                       WHERE rejection_id=@rejection_id AND department_id=@department_id)
            THROW 52012,'El departamento no es requerido para este rechazo.',1;
        SELECT @user_id=user_id,@role_id=role_id
        FROM dbo.ssi_PaS_Users
        WHERE barcode_id=UPPER(LTRIM(RTRIM(@barcode))) AND is_active=1
          AND password_hash=HASHBYTES('SHA2_256',CONVERT(VARCHAR(50),@password));
        IF @user_id IS NULL THROW 52013,'Credenciales incorrectas o usuario inactivo.',1;
        IF @user_id=@detector THROW 52014,'El operador del rechazo no puede autorizarse a si mismo.',1;
        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionDepartmentRoles
                       WHERE department_id=@department_id AND role_id=@role_id)
            THROW 52015,'El rol del usuario no esta autorizado para este departamento.',1;
        IF EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionApprovals
                   WHERE rejection_id=@rejection_id AND authorized_by_user_id=@user_id)
            THROW 52016,'Una persona solo puede aprobar un departamento por rechazo.',1;
        IF EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionApprovals
                   WHERE rejection_id=@rejection_id AND department_id=@department_id)
            THROW 52017,'Este departamento ya aprobo el rechazo.',1;
        INSERT dbo.ssi_PaS_RejectionApprovals(rejection_id,department_id,authorized_by_user_id)
        VALUES (@rejection_id,@department_id,@user_id);
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_RejectionRequirements rr
            WHERE rr.rejection_id=@rejection_id AND NOT EXISTS (
                SELECT 1 FROM dbo.ssi_PaS_RejectionApprovals a
                WHERE a.rejection_id=rr.rejection_id AND a.department_id=rr.department_id)
        ) AND EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionRequirements WHERE rejection_id=@rejection_id)
            UPDATE dbo.ssi_PaS_ScanRejections SET status='APPROVED',resolved_at=SYSUTCDATETIME()
            WHERE rejection_id=@rejection_id AND status='BLOCKED';
        COMMIT TRANSACTION;
        SELECT status FROM dbo.ssi_PaS_ScanRejections WHERE rejection_id=@rejection_id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SQL impide introducir piezas en un contenedor bloqueado, incluso desde otras estaciones.
CREATE OR ALTER TRIGGER dbo.trg_ssi_PaS_RejectBlockedContainerInsert
ON dbo.ssi_PaS_ContainerPieces AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=i.container_level_id
        JOIN dbo.ssi_PaS_ScanRejections r ON r.container_id=l.container_id AND r.status='BLOCKED'
    )
        THROW 52019,'El pallet tiene un rechazo pendiente de autorizaciones.',1;
END;
GO

-- Gestion de permisos: solo usuario activo con acceso a Setup.
CREATE OR ALTER PROCEDURE dbo.ssi_PaS_SaveRejectionDepartment
    @actor_user_id INT,@department_id INT,@is_required BIT,@roles_csv VARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_Users u
            JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id
            WHERE u.user_id=@actor_user_id AND u.is_active=1 AND r.can_modify_setup=1
        ) THROW 52021,'No tienes permiso para configurar autorizaciones.',1;
        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments WHERE department_id=@department_id)
            THROW 52022,'Departamento inexistente.',1;
        DECLARE @roles TABLE(role_id INT PRIMARY KEY);
        INSERT @roles(role_id)
        SELECT DISTINCT TRY_CONVERT(INT,LTRIM(RTRIM(value)))
        FROM STRING_SPLIT(@roles_csv,',')
        WHERE TRY_CONVERT(INT,LTRIM(RTRIM(value))) IS NOT NULL;
        IF EXISTS(SELECT 1 FROM @roles a WHERE NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Roles r WHERE r.role_id=a.role_id))
            THROW 52023,'Uno de los roles seleccionados no existe.',1;
        IF @is_required=1 AND NOT EXISTS(SELECT 1 FROM @roles)
            THROW 52024,'Un departamento habilitado requiere al menos un rol.',1;
        UPDATE dbo.ssi_PaS_RejectionDepartments
        SET is_required=@is_required WHERE department_id=@department_id;
        DELETE dbo.ssi_PaS_RejectionDepartmentRoles WHERE department_id=@department_id;
        INSERT dbo.ssi_PaS_RejectionDepartmentRoles(department_id,role_id)
        SELECT @department_id,role_id FROM @roles;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- IMPORTANTE: el usuario de la app necesita EXECUTE sobre estos procedimientos
-- y SELECT de estas cinco tablas. Administrar GRANT segun rol SQL de la planta.
