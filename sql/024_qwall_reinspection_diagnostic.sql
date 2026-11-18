-- 024_qwall_reinspection_diagnostic.sql
-- Diagnóstico de una pieza rechazada anteriormente y reinspeccionada en Q-Wall.
-- SOLO LECTURA. No actualiza inspecciones, rechazos, firmas ni empaques.
-- Cambia estos tres valores por los códigos COMPLETOS que entrega el escáner.
-- Ejecuta en CCS, la misma base de datos que utiliza Pack And Ship.
SET NOCOUNT ON;

DECLARE @PartNumber NVARCHAR(100) = N'REEMPLAZAR_PARTE';
DECLARE @SsiFullCode NVARCHAR(200) = N'REEMPLAZAR_QR_SSI_COMPLETO';
DECLARE @ClientFullCode NVARCHAR(200) = N'REEMPLAZAR_QR_CLIENTE_COMPLETO';

-- 1) Identidad EXACTA exigida por PackAndShip: debe producir una sola fila.
SELECT p.product_id, pn.pn_id, pn.ssiPN, p.frameSN AS qr_ssi,
       p.fpcaSN AS qr_cliente
FROM dbo.ssi_Products AS p
JOIN dbo.ssi_PartNumbers AS pn ON pn.pn_id = p.pn_id
WHERE pn.ssiPN = @PartNumber
  AND p.frameSN = @SsiFullCode
  AND p.fpcaSN = @ClientFullCode;

-- 2) Historial de TODAS las inspecciones de ese producto, incluida la segunda.
-- RN=1 equivale EXACTAMENTE a la inspección consultada en el cliente VB.NET
-- y en la inserción transaccional PaSDirectSql.AddPiece.
;WITH historial AS (
    SELECT i.product_id, i.inspection_id, i.started_at, i.finished_at,
           i.overall_result, i.is_scrap,
           ROW_NUMBER() OVER (
               PARTITION BY i.product_id
               ORDER BY COALESCE(i.started_at, i.finished_at) DESC, i.inspection_id DESC
           ) AS rn
    FROM dbo.ssi_Inspections AS i
    JOIN dbo.ssi_Products AS p ON p.product_id = i.product_id
    JOIN dbo.ssi_PartNumbers AS pn ON pn.pn_id = p.pn_id
    WHERE pn.ssiPN = @PartNumber
      AND p.frameSN = @SsiFullCode
      AND p.fpcaSN = @ClientFullCode
)
SELECT h.product_id, h.inspection_id, h.started_at, h.finished_at,
       h.overall_result, h.is_scrap, h.rn AS orden,
       CASE WHEN h.rn = 1 THEN 'ULTIMA - DECIDE EL EMPAQUE'
            ELSE 'HISTORICA' END AS prioridad,
       CASE WHEN h.finished_at IS NULL THEN 'PENDIENTE'
            WHEN ISNULL(h.is_scrap, 0) = 1 THEN 'SCRAP'
            WHEN ISNULL(h.overall_result, 0) <> 1 THEN 'FAIL'
            WHEN EXISTS (
                SELECT 1
                FROM dbo.ssi_PieceFlagRecords f
                WHERE f.inspection_id = h.inspection_id
                  AND (f.product_id <> h.product_id
                       OR f.authorized_by_user_id IS NULL
                       OR f.authorized_at IS NULL)
            ) THEN 'BANDERA SIN AUTORIZAR'
            ELSE 'PASS - CANDIDATA A EMPAQUE'
       END AS resultado_packship,
       (SELECT COUNT(*) FROM dbo.ssi_PieceFlagRecords f
        WHERE f.inspection_id = h.inspection_id) AS banderas_totales
FROM historial AS h
ORDER BY h.product_id, h.rn;

-- 3) Una pieza también se bloquea si ya está en cualquier otro contenedor.
SELECT cp.product_id, c.container_code, c.status AS contenedor_estado,
       cl.level_number, cp.scanned_at
FROM dbo.ssi_PaS_ContainerPieces AS cp
JOIN dbo.ssi_PaS_ContainerLevels AS cl
  ON cl.container_level_id = cp.container_level_id
JOIN dbo.ssi_PaS_Containers AS c ON c.container_id = cl.container_id
JOIN dbo.ssi_Products AS p ON p.product_id = cp.product_id
JOIN dbo.ssi_PartNumbers AS pn ON pn.pn_id = p.pn_id
WHERE pn.ssiPN = @PartNumber
  AND p.frameSN = @SsiFullCode
  AND p.fpcaSN = @ClientFullCode
ORDER BY cp.scanned_at DESC;

-- 4) Aunque Q-Wall tenga PASS posterior, un rechazo BLOCKED en CCS conserva
-- las firmas pendientes: no liberarlo ni borrarlo automáticamente.
SELECT r.rejection_id, c.container_code, r.rejection_type, r.status,
       r.rejection_reason, r.detected_at
FROM dbo.ssi_PaS_ScanRejections AS r
JOIN dbo.ssi_PaS_Containers AS c ON c.container_id = r.container_id
WHERE r.ssi_scan = @SsiFullCode
  AND COALESCE(r.client_scan, N'') = @ClientFullCode
ORDER BY r.detected_at DESC, r.rejection_id DESC;

-- Lectura de resultados:
-- A) Cero filas en #1: PN o códigos completos no coinciden con ssi_Products.
-- B) #2: FAIL seguido de PASS finalizada con orden=1 debe ser aceptable.
-- C) #2: orden=1 PENDIENTE / FAIL / SCRAP / BANDERA -> rechazo correcto.
-- D) #3 con filas: duplicado global, aunque la inspección reciente sea PASS.
-- E) #4 status BLOCKED: resolver autorizaciones antes de volver a escanear.
