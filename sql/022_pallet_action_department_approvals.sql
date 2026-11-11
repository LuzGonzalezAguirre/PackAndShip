-- 022_pallet_action_department_approvals.sql
-- Ejecutar en CCS despues de 020 (y 021 si se utiliza).
-- Mantiene pallets, usuarios, Q-Wall y configuraciones existentes.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ssi_PaS_PalletActionRequests',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_PalletActionRequests(
        request_id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        container_id INT NOT NULL REFERENCES dbo.ssi_PaS_Containers(container_id),
        action_type VARCHAR(20) NOT NULL CHECK(action_type IN('CLOSE_PARTIAL','CANCEL_EMPTY')),
        requested_by_user_id INT NOT NULL REFERENCES dbo.ssi_PaS_Users(user_id),
        reason NVARCHAR(500) NOT NULL CHECK(LEN(LTRIM(RTRIM(reason)))>=10),
        status VARCHAR(12) NOT NULL DEFAULT('PENDING')
            CHECK(status IN('PENDING','COMPLETED')),
        requested_at DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        completed_at DATETIME2(3) NULL
    );
    CREATE UNIQUE INDEX UX_PaS_ActionRequestPending
    ON dbo.ssi_PaS_PalletActionRequests(container_id) WHERE status='PENDING';
END;
GO
IF OBJECT_ID(N'dbo.ssi_PaS_PalletActionRequirements',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_PalletActionRequirements(
        request_id BIGINT NOT NULL REFERENCES dbo.ssi_PaS_PalletActionRequests(request_id),
        department_id INT NOT NULL REFERENCES dbo.ssi_PaS_RejectionDepartments(department_id),
        CONSTRAINT PK_PaS_PalletActionRequirements PRIMARY KEY(request_id,department_id)
    );
END;
GO
IF OBJECT_ID(N'dbo.ssi_PaS_PalletActionApprovals',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_PalletActionApprovals(
        approval_id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        request_id BIGINT NOT NULL,
        department_id INT NOT NULL,
        authorized_by_user_id INT NOT NULL REFERENCES dbo.ssi_PaS_Users(user_id),
        approved_at DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PaS_ActionApprovalRequired
            FOREIGN KEY(request_id,department_id)
            REFERENCES dbo.ssi_PaS_PalletActionRequirements(request_id,department_id),
        CONSTRAINT UQ_PaS_ActionApprovalDepartment UNIQUE(request_id,department_id),
        CONSTRAINT UQ_PaS_ActionApprovalPerson UNIQUE(request_id,authorized_by_user_id)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_BeginPalletAction
    @container_id INT,@action_type VARCHAR(20),
    @packing_user_id INT,@reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @id BIGINT;
    DECLARE @note NVARCHAR(500)=LTRIM(RTRIM(COALESCE(@reason,N'')));
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @action_type NOT IN('CLOSE_PARTIAL','CANCEL_EMPTY')
            THROW 52201,'Tipo de operación no permitido.',1;
        IF LEN(@note)<10 THROW 52202,'Motivo obligatorio: mínimo 10 caracteres.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Users
                      WHERE user_id=@packing_user_id AND is_active=1)
            THROW 52203,'El operador no tiene una sesión activa.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Containers WITH(UPDLOCK,HOLDLOCK)
                      WHERE container_id=@container_id AND status='OPEN')
            THROW 52204,'El pallet debe estar ABIERTO.',1;
        IF EXISTS(SELECT 1 FROM dbo.ssi_PaS_ScanRejections
                  WHERE container_id=@container_id AND status='BLOCKED')
            THROW 52205,'Resuelve primero el rechazo de pieza pendiente.',1;
        SELECT @id=request_id FROM dbo.ssi_PaS_PalletActionRequests WITH(UPDLOCK,HOLDLOCK)
        WHERE container_id=@container_id AND status='PENDING';
        IF @id IS NOT NULL
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionRequests
                WHERE request_id=@id AND action_type=@action_type
                  AND requested_by_user_id=@packing_user_id AND reason=@note)
                THROW 52206,'Ya existe una solicitud diferente pendiente en este pallet.',1;
        END
        ELSE
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments WHERE is_required=1)
                THROW 52207,'Habilita al menos un departamento autorizador en Setup.',1;
            IF EXISTS(
                SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments d
                WHERE d.is_required=1 AND NOT EXISTS(
                    SELECT 1 FROM dbo.ssi_PaS_RejectionDepartmentRoles m
                    WHERE m.department_id=d.department_id
                )
            ) THROW 52208,'Hay un departamento sin roles autorizados.',1;
            INSERT dbo.ssi_PaS_PalletActionRequests
                (container_id,action_type,requested_by_user_id,reason)
            VALUES(@container_id,@action_type,@packing_user_id,@note);
            SET @id=SCOPE_IDENTITY();
            INSERT dbo.ssi_PaS_PalletActionRequirements(request_id,department_id)
            SELECT @id,department_id FROM dbo.ssi_PaS_RejectionDepartments WHERE is_required=1;
        END;
        COMMIT TRANSACTION;
        SELECT @id AS request_id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_ApprovePalletAction
    @request_id BIGINT,@department_id INT,
    @barcode VARCHAR(50),@password VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @user_id INT,@role_id INT,@operator INT;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @operator=requested_by_user_id
        FROM dbo.ssi_PaS_PalletActionRequests WITH(UPDLOCK,HOLDLOCK)
        WHERE request_id=@request_id AND status='PENDING';
        IF @operator IS NULL THROW 52211,'La solicitud ya no está pendiente.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionRequirements
                      WHERE request_id=@request_id AND department_id=@department_id)
            THROW 52212,'Este departamento no requiere aprobación.',1;
        SELECT @user_id=user_id,@role_id=role_id
        FROM dbo.ssi_PaS_Users
        WHERE barcode_id=UPPER(LTRIM(RTRIM(@barcode))) AND is_active=1
          AND password_hash=HASHBYTES('SHA2_256',CONVERT(VARCHAR(50),@password));
        IF @user_id IS NULL THROW 52213,'Credenciales incorrectas o usuario inactivo.',1;
        IF @user_id=@operator THROW 52214,'El operador solicitante no puede aprobar su propio pallet.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_RejectionDepartmentRoles
                      WHERE department_id=@department_id AND role_id=@role_id)
            THROW 52215,'El rol no está autorizado para ese departamento.',1;
        IF EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionApprovals
                  WHERE request_id=@request_id AND authorized_by_user_id=@user_id)
            THROW 52216,'Un usuario solo puede aprobar un departamento.',1;
        INSERT dbo.ssi_PaS_PalletActionApprovals
            (request_id,department_id,authorized_by_user_id)
        VALUES(@request_id,@department_id,@user_id);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Nunca llamar los procedimientos anteriores desde la app: sólo admitían una aprobación.
-- El procedimiento nuevo realiza toda la operación bajo una única transacción SQL.
CREATE OR ALTER PROCEDURE dbo.ssi_PaS_CompletePalletAction
    @request_id BIGINT,@packing_user_id INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @container INT,@action VARCHAR(20),@note NVARCHAR(500);
    DECLARE @actual INT,@required INT,@authorizer INT;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @container=container_id,@action=action_type,@note=reason
        FROM dbo.ssi_PaS_PalletActionRequests WITH(UPDLOCK,HOLDLOCK)
        WHERE request_id=@request_id AND status='PENDING'
          AND requested_by_user_id=@packing_user_id;
        IF @container IS NULL THROW 52221,'Solicitud inexistente, completada o de otro usuario.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Users
                      WHERE user_id=@packing_user_id AND is_active=1)
            THROW 52222,'Operador inactivo.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Containers WITH(UPDLOCK,HOLDLOCK)
                      WHERE container_id=@container AND status='OPEN')
            THROW 52223,'Pallet cerrado o modificado.',1;
        IF EXISTS(SELECT 1 FROM dbo.ssi_PaS_ScanRejections
                  WHERE container_id=@container AND status='BLOCKED')
            THROW 52224,'Hay una pieza rechazada pendiente de aprobación.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionRequirements
                      WHERE request_id=@request_id)
            THROW 52225,'Solicitud sin departamentos requeridos.',1;
        IF EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_PalletActionRequirements req
            WHERE req.request_id=@request_id AND NOT EXISTS(
                SELECT 1 FROM dbo.ssi_PaS_PalletActionApprovals a
                WHERE a.request_id=req.request_id AND a.department_id=req.department_id
            )
        ) THROW 52226,'Faltan aprobaciones de departamentos.',1;
        -- El primer autorizador representa el registro legado; todas las aprobaciones
        -- quedan guardadas individualmente en ssi_PaS_PalletActionApprovals.
        SELECT TOP(1) @authorizer=authorized_by_user_id
        FROM dbo.ssi_PaS_PalletActionApprovals
        WHERE request_id=@request_id ORDER BY department_id;
        IF @authorizer IS NULL THROW 52227,'No hay aprobadores.',1;
        SELECT @required=SUM(required_pieces)
        FROM dbo.ssi_PaS_ContainerLevels WITH(UPDLOCK,HOLDLOCK)
        WHERE container_id=@container;
        SELECT @actual=COUNT(*)
        FROM dbo.ssi_PaS_ContainerPieces p WITH(UPDLOCK,HOLDLOCK)
        JOIN dbo.ssi_PaS_ContainerLevels l WITH(UPDLOCK,HOLDLOCK)
            ON l.container_level_id=p.container_level_id
        WHERE l.container_id=@container;
        IF @action='CLOSE_PARTIAL'
        BEGIN
            IF @actual<=0 OR @actual>=@required OR @required IS NULL
                THROW 52228,'El pallet parcial debe tener piezas y estar incompleto.',1;
            UPDATE dbo.ssi_PaS_Containers
            SET status='PARTIAL',closed_at=SYSUTCDATETIME(),closed_by_user_id=@authorizer
            WHERE container_id=@container AND status='OPEN';
            INSERT dbo.ssi_PaS_PartialClosures
                (container_id,authorized_by_user_id,packing_user_id,
                 actual_pieces,required_pieces,reason)
            VALUES (@container,@authorizer,@packing_user_id,@actual,@required,@note);
            INSERT dbo.ssi_PaS_PackingAudit
                (event_type,container_id,actor_user_id,reason)
            VALUES ('CLOSE_PARTIAL',@container,@authorizer,@note);
        END
        ELSE IF @action='CANCEL_EMPTY'
        BEGIN
            IF @actual<>0 OR EXISTS(SELECT 1 FROM dbo.ssi_PaS_ContainerLevels
                                    WHERE container_id=@container AND status='CLOSED')
                THROW 52229,'Solo se cancela un pallet vacío, sin niveles cerrados.',1;
            UPDATE dbo.ssi_PaS_Containers SET status='CANCELLED'
            WHERE container_id=@container AND status='OPEN';
            INSERT dbo.ssi_PaS_EmptyPalletCancellations
                (container_id,packing_user_id,authorized_by_user_id,reason)
            VALUES (@container,@packing_user_id,@authorizer,@note);
            INSERT dbo.ssi_PaS_PackingAudit
                (event_type,container_id,actor_user_id,reason)
            VALUES ('CANCEL_CONTAINER',@container,@authorizer,@note);
        END
        ELSE THROW 52230,'Tipo de operación no reconocido.',1;
        UPDATE dbo.ssi_PaS_PalletActionRequests
        SET status='COMPLETED',completed_at=SYSUTCDATETIME()
        WHERE request_id=@request_id;
        COMMIT TRANSACTION;
        SELECT @container AS container_id,@action AS action_type,'COMPLETED' AS status;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Impedir que software antiguo o conexiones con db_owner usen una sola firma.
-- En adelante solo ssi_PaS_CompletePalletAction realiza estas operaciones.
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
    THROW 52241,'Cierre parcial requiere autorizaciones por departamento. Actualiza PackAndShip.',1;
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
    THROW 52242,'Cancelación requiere autorizaciones por departamento. Actualiza PackAndShip.',1;
END;
GO

-- También cerrar la ruta de cancelación genérica usada por versiones anteriores.
CREATE OR ALTER PROCEDURE dbo.ssi_PaS_CancelContainer
    @container_id INT,
    @actor_user_id INT,
    @reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    THROW 52243,'La cancelación requiere autorizaciones por departamento. Actualiza PackAndShip.',1;
END;
GO

-- Los procedimientos heredados permiten una sola aprobación: deshabilitar su
-- ejecución al rol runtime para no permitir un bypass por interfaz antigua.
IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NOT NULL
BEGIN
    DENY EXECUTE ON dbo.ssi_PaS_ClosePartialContainer TO ssi_PaS_RuntimeRole;
    DENY EXECUTE ON dbo.ssi_PaS_CancelEmptyPalletAuthorized TO ssi_PaS_RuntimeRole;
    DENY EXECUTE ON dbo.ssi_PaS_CancelContainer TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_PalletActionRequests TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_PalletActionRequirements TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_PalletActionApprovals TO ssi_PaS_RuntimeRole;
    GRANT EXECUTE ON dbo.ssi_PaS_BeginPalletAction TO ssi_PaS_RuntimeRole;
    GRANT EXECUTE ON dbo.ssi_PaS_ApprovePalletAction TO ssi_PaS_RuntimeRole;
    GRANT EXECUTE ON dbo.ssi_PaS_CompletePalletAction TO ssi_PaS_RuntimeRole;
END;
GO

SELECT r.request_id,c.container_code,r.action_type,r.status,r.reason,
       d.department_name,a.authorized_by_user_id,a.approved_at
FROM dbo.ssi_PaS_PalletActionRequests r
JOIN dbo.ssi_PaS_Containers c ON c.container_id=r.container_id
JOIN dbo.ssi_PaS_PalletActionRequirements rr ON rr.request_id=r.request_id
JOIN dbo.ssi_PaS_RejectionDepartments d ON d.department_id=rr.department_id
LEFT JOIN dbo.ssi_PaS_PalletActionApprovals a
    ON a.request_id=rr.request_id AND a.department_id=rr.department_id
ORDER BY r.request_id DESC,d.department_id;
