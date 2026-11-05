-- Q-Wall PASS validation diagnostics: read-only.
-- Replace sample identifiers with a piece that was inspected in Q-Wall.
DECLARE @PartNumber varchar(50) = '43291.6';
DECLARE @SsiScan nvarchar(200) = '43291.632611904006';
DECLARE @ClientScan nvarchar(200) = '24015866-P02 32611904006';

SELECT
    p.product_id, p.pn_id, pn.ssiPN AS SSI_Part,
    p.frameSN AS SSI_Scan, p.fpcaSN AS Client_Scan,
    p.volvoSerialNumber, p.workOrder,
    i.inspection_id, i.inspection_type, i.overall_result,
    i.is_scrap, i.started_at, i.finished_at,
    (SELECT COUNT(*) FROM dbo.ssi_PieceFlagRecords fl
     WHERE fl.inspection_id=i.inspection_id) AS flag_count
FROM dbo.ssi_Products AS p
INNER JOIN dbo.ssi_PartNumbers AS pn ON pn.pn_id=p.pn_id
LEFT JOIN dbo.ssi_Inspections AS i ON i.product_id=p.product_id
WHERE pn.ssiPN=@PartNumber
  AND p.frameSN=@SsiScan
  AND p.fpcaSN=@ClientScan
ORDER BY p.product_id, i.finished_at DESC, i.inspection_id DESC;

-- If no rows are returned, confirm the exact Q-Wall strings without
-- altering any table. The scanner requires the full stored strings.
SELECT TOP (20)
    p.product_id, pn.ssiPN AS SSI_Part,
    p.frameSN, p.fpcaSN, p.volvoSerialNumber, p.created_at
FROM dbo.ssi_Products AS p
INNER JOIN dbo.ssi_PartNumbers AS pn ON pn.pn_id=p.pn_id
WHERE pn.ssiPN=@PartNumber
ORDER BY p.created_at DESC;
