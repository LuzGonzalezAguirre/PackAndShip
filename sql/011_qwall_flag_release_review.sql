-- 011_qwall_flag_release_review.sql
-- Read-only diagnostics to determine whether Q-Wall has a persistent
-- authorization / release signal for flagged pieces.
-- Execute in CCS and share results before changing PackAndShip acceptance rules.
SET NOCOUNT ON;

-- 1. Complete schema: never assume flags mean approved or rejected.
SELECT t.name AS table_name, c.column_id, c.name AS column_name,
       typ.name AS data_type, c.is_nullable
FROM sys.tables AS t
JOIN sys.columns AS c ON c.object_id=t.object_id
JOIN sys.types AS typ ON typ.user_type_id=c.user_type_id
WHERE t.schema_id=SCHEMA_ID('dbo')
  AND (t.name IN ('ssi_PieceFlagRecords','ssi_Inspections','ssi_Products')
       OR t.name LIKE '%Flag%' OR t.name LIKE '%Release%'
       OR t.name LIKE '%Authorization%')
ORDER BY t.name,c.column_id;

-- 2. Foreign keys: discover which entity a flag/release actually belongs to.
SELECT fk.name AS foreign_key, OBJECT_NAME(fk.parent_object_id) AS from_table,
       COL_NAME(fkc.parent_object_id,fkc.parent_column_id) AS from_column,
       OBJECT_NAME(fk.referenced_object_id) AS to_table,
       COL_NAME(fkc.referenced_object_id,fkc.referenced_column_id) AS to_column
FROM sys.foreign_keys AS fk
JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id=fk.object_id
WHERE OBJECT_NAME(fk.parent_object_id) LIKE '%Flag%'
   OR OBJECT_NAME(fk.referenced_object_id) LIKE '%Flag%'
   OR OBJECT_NAME(fk.parent_object_id) LIKE '%Release%'
   OR OBJECT_NAME(fk.referenced_object_id) LIKE '%Release%'
ORDER BY fk.name,fkc.constraint_column_id;

-- 3. Sample stored flag records including whatever approval columns exist.
IF OBJECT_ID(N'dbo.ssi_PieceFlagRecords',N'U') IS NOT NULL
    EXEC(N'SELECT TOP (20) * FROM dbo.ssi_PieceFlagRecords ORDER BY 1 DESC;');

-- 4. Inspection outcomes for flagged pieces: PASS does not itself prove
-- that a flag has been authorized for packing.
IF OBJECT_ID(N'dbo.ssi_PieceFlagRecords',N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.ssi_PieceFlagRecords',N'inspection_id') IS NOT NULL
BEGIN
    SELECT TOP (30)
        p.product_id, p.pn_id, p.frameSN, p.fpcaSN,
        i.inspection_id, i.inspection_type, i.overall_result,
        i.is_scrap, i.finished_at,
        (SELECT COUNT(*) FROM dbo.ssi_PieceFlagRecords f
         WHERE f.inspection_id=i.inspection_id) AS flag_count
    FROM dbo.ssi_Inspections AS i
    JOIN dbo.ssi_Products AS p ON p.product_id=i.product_id
    WHERE EXISTS (SELECT 1 FROM dbo.ssi_PieceFlagRecords AS f
                  WHERE f.inspection_id=i.inspection_id)
    ORDER BY i.finished_at DESC,i.inspection_id DESC;
END;
