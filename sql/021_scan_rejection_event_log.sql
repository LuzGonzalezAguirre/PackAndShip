-- 021_scan_rejection_event_log.sql
-- CCS: registro INMEDIATO e INDEPENDIENTE de credenciales y autorizaciones.
-- Ejecutar despues de 020_scan_rejection_authorizations.sql.
-- No elimina ni modifica pallets, inspecciones de Q-Wall ni autorizaciones previas.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ssi_PaS_ScanRejectionEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ssi_PaS_ScanRejectionEvents
    (
        event_id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_ssi_PaS_ScanRejectionEvents PRIMARY KEY,
        event_token UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT UQ_ssi_PaS_ScanRejectionEvents_Token UNIQUE,
        rejection_id BIGINT NOT NULL
            CONSTRAINT FK_ssi_PaS_ScanRejectionEvents_Rejection
                REFERENCES dbo.ssi_PaS_ScanRejections(rejection_id),
        container_id INT NOT NULL
            CONSTRAINT FK_ssi_PaS_ScanRejectionEvents_Container
                REFERENCES dbo.ssi_PaS_Containers(container_id),
        rejection_type VARCHAR(30) NOT NULL,
        ssi_scan NVARCHAR(200) NOT NULL,
        client_scan NVARCHAR(200) NULL,
        rejection_reason NVARCHAR(1000) NOT NULL,
        detected_by_user_id INT NOT NULL
            CONSTRAINT FK_ssi_PaS_ScanRejectionEvents_Actor
                REFERENCES dbo.ssi_PaS_Users(user_id),
        detected_at DATETIME2(3) NOT NULL
            CONSTRAINT DF_ssi_PaS_ScanRejectionEvents_DetectedAt
                DEFAULT SYSUTCDATETIME()
    );
END;
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id=OBJECT_ID(N'dbo.ssi_PaS_ScanRejectionEvents')
      AND name=N'IX_ssi_PaS_ScanRejectionEvents_Detected'
)
    CREATE INDEX IX_ssi_PaS_ScanRejectionEvents_Detected
        ON dbo.ssi_PaS_ScanRejectionEvents(detected_at DESC,event_id DESC);
GO

-- Migrar un registro histórico por cada rechazo anterior, si los hubiera.
-- No se conocen escaneos repetidos anteriores a la creación de esta tabla.
INSERT dbo.ssi_PaS_ScanRejectionEvents
    (event_token,rejection_id,container_id,rejection_type,ssi_scan,
     client_scan,rejection_reason,detected_by_user_id,detected_at)
SELECT NEWID(),r.rejection_id,r.container_id,r.rejection_type,r.ssi_scan,
       r.client_scan,r.rejection_reason,r.detected_by_user_id,r.detected_at
FROM dbo.ssi_PaS_ScanRejections r
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ssi_PaS_ScanRejectionEvents e
    WHERE e.rejection_id=r.rejection_id
);
GO

-- El incidente se escribe en la MISMA transacción que crea/busca el bloqueo.
-- @event_token hace idempotente el reenvío después de un timeout (sin duplicar eventos).
-- No se solicitan credenciales de aprobadores para registrar el evento.
CREATE OR ALTER PROCEDURE dbo.ssi_PaS_StartScanRejection
    @container_id INT,
    @actor_user_id INT,
    @rejection_type VARCHAR(30),
    @ssi_scan NVARCHAR(200),
    @client_scan NVARCHAR(200)=NULL,
    @reason NVARCHAR(1000),
    @event_token UNIQUEIDENTIFIER=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @id BIGINT;
    SET @event_token=COALESCE(@event_token,NEWID());

    BEGIN TRY
        BEGIN TRANSACTION;

        -- El bloqueo del contenedor serializa altas concurrentes.
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)
            WHERE container_id=@container_id AND status='OPEN'
        ) THROW 52101,'Solo se registran rechazos en pallets abiertos.',1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users
            WHERE user_id=@actor_user_id AND is_active=1
        ) THROW 52102,'Se requiere un usuario activo.',1;

        -- Si la misma estación reintenta un evento ya confirmado en CCS,
        -- recuperar su rechazo original, sin generar una fila nueva.
        SELECT @id=rejection_id
        FROM dbo.ssi_PaS_ScanRejectionEvents WITH(UPDLOCK,HOLDLOCK)
        WHERE event_token=@event_token;

        IF @id IS NULL
        BEGIN
            SELECT @id=rejection_id
            FROM dbo.ssi_PaS_ScanRejections WITH(UPDLOCK,HOLDLOCK)
            WHERE container_id=@container_id AND status='BLOCKED';

            IF @id IS NULL
            BEGIN
                INSERT dbo.ssi_PaS_ScanRejections
                    (container_id,rejection_type,ssi_scan,client_scan,
                     rejection_reason,detected_by_user_id)
                VALUES
                    (@container_id,@rejection_type,COALESCE(@ssi_scan,N''),
                     @client_scan,LEFT(COALESCE(@reason,N''),1000),@actor_user_id);

                SET @id=SCOPE_IDENTITY();

                INSERT dbo.ssi_PaS_RejectionRequirements(rejection_id,department_id)
                SELECT @id,department_id
                FROM dbo.ssi_PaS_RejectionDepartments WHERE is_required=1;
            END;

            -- Registrar CADA incidencia detectada, aunque el contenedor
            -- ya estuviera bloqueado por un rechazo anterior.
            INSERT dbo.ssi_PaS_ScanRejectionEvents
                (event_token,rejection_id,container_id,rejection_type,
                 ssi_scan,client_scan,rejection_reason,detected_by_user_id)
            VALUES
                (@event_token,@id,@container_id,@rejection_type,
                 COALESCE(@ssi_scan,N''),@client_scan,
                 LEFT(COALESCE(@reason,N''),1000),@actor_user_id);
        END;

        COMMIT TRANSACTION;
        SELECT @id AS rejection_id,@event_token AS event_token;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NOT NULL
BEGIN
    GRANT SELECT ON dbo.ssi_PaS_ScanRejectionEvents TO ssi_PaS_RuntimeRole;
    GRANT EXECUTE ON dbo.ssi_PaS_StartScanRejection TO ssi_PaS_RuntimeRole;
END;
GO

-- Consulta para consultar el histórico completo, CON o SIN aprobaciones.
SELECT TOP (100)
    e.event_id AS [Evento],
    e.detected_at AS [Fecha UTC],
    c.container_code AS [Contenedor],
    e.rejection_type AS [Tipo de rechazo],
    e.ssi_scan AS [Serial SSI],
    e.client_scan AS [Serial Cliente],
    e.rejection_reason AS [Motivo],
    u.name AS [Detectado por],
    r.status AS [Estado autorización],
    e.rejection_id AS [Folio rechazo]
FROM dbo.ssi_PaS_ScanRejectionEvents e
JOIN dbo.ssi_PaS_Containers c ON c.container_id=e.container_id
JOIN dbo.ssi_PaS_Users u ON u.user_id=e.detected_by_user_id
JOIN dbo.ssi_PaS_ScanRejections r ON r.rejection_id=e.rejection_id
ORDER BY e.event_id DESC;
