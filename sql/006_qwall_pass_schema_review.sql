-- Read-only catalog discovery for the final PackAndShip scanner implementation.
-- Run on CCS. This script does not insert, update or delete any data.
-- Share the metadata results so PASS and product identity can be verified.
SET NOCOUNT ON;

SELECT
    t.name AS table_name,
    c.column_id,
    c.name AS column_name,
    ty.name AS sql_type,
    c.max_length,
    c.is_nullable
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE s.name = 'dbo'
  AND (
        t.name IN ('ssi_Products','ssi_PieceFlagRecords',
                   'ssi_PartNumbers','ssi_RejectionPhotos')
        OR t.name LIKE '%Inspection%'
        OR t.name LIKE '%Quality%'
      )
ORDER BY t.name, c.column_id;

SELECT
    fk.name AS foreign_key_name,
    parent_t.name AS parent_table,
    parent_c.name AS parent_column,
    referenced_t.name AS referenced_table,
    referenced_c.name AS referenced_column
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
JOIN sys.tables parent_t ON parent_t.object_id=fkc.parent_object_id
JOIN sys.columns parent_c ON parent_c.object_id=fkc.parent_object_id
    AND parent_c.column_id=fkc.parent_column_id
JOIN sys.tables referenced_t ON referenced_t.object_id=fkc.referenced_object_id
JOIN sys.columns referenced_c ON referenced_c.object_id=fkc.referenced_object_id
    AND referenced_c.column_id=fkc.referenced_column_id
WHERE parent_t.name IN ('ssi_Products','ssi_PieceFlagRecords','ssi_PartNumbers')
   OR referenced_t.name IN ('ssi_Products','ssi_PieceFlagRecords','ssi_PartNumbers')
ORDER BY fk.name, fkc.constraint_column_id;

SELECT
    t.name AS table_name, i.name AS index_name,
    i.is_unique, i.is_primary_key,
    STRING_AGG(QUOTENAME(c.name), ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS key_columns
FROM sys.tables t
JOIN sys.indexes i ON i.object_id=t.object_id AND i.index_id>0
JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE t.name IN ('ssi_Products','ssi_PieceFlagRecords')
  AND ic.is_included_column=0 AND ic.key_ordinal>0
GROUP BY t.name,i.name,i.is_unique,i.is_primary_key
ORDER BY t.name,i.name;
