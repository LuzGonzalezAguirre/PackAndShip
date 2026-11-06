-- 012_qwall_flag_authorization_cases.sql
-- CCS: read-only review of flagged PASS/FAIL inspection relationships.
-- Having authorized_by_user_id in ssi_PieceFlagRecords does NOT alone
-- establish that a product is cleared for packing.
SET NOCOUNT ON;

SELECT TOP (100)
    f.flag_id,
    f.product_id,
    pn.ssiPN AS part_number,
    p.frameSN AS ssi_serial,
    p.fpcaSN AS client_serial,
    f.inspection_id AS flagged_inspection_id,
    i.inspection_type,
    i.overall_result,
    i.is_scrap,
    i.finished_at,
    i.authorized_by_user_id AS inspection_authorized_by,
    i.authorized_at AS inspection_authorized_at,
    f.authorized_by_user_id AS flag_authorized_by,
    f.authorized_at AS flag_authorized_at,
    f.comment AS flag_comment,
    latest.inspection_id AS latest_inspection_id,
    latest.overall_result AS latest_result,
    latest.is_scrap AS latest_is_scrap,
    latest.finished_at AS latest_finished_at,
    latest.authorized_by_user_id AS latest_inspection_authorized_by,
    latest.authorized_at AS latest_inspection_authorized_at,
    CASE
        WHEN latest.inspection_id IS NULL OR latest.finished_at IS NULL
            THEN 'NO COMPLETED LATEST INSPECTION'
        WHEN latest.overall_result <> 1 OR latest.overall_result IS NULL
            THEN 'LATEST NOT PASS'
        WHEN latest.is_scrap = 1 THEN 'SCRAP'
        WHEN latest.inspection_id <> i.inspection_id
            THEN 'FLAG ON PRIOR INSPECTION - REVIEW'
        WHEN i.overall_result = 1 AND
             i.authorized_by_user_id IS NOT NULL AND i.authorized_at IS NOT NULL
            THEN 'PASS + INSPECTION AUTHORIZATION - REVIEW RELEASE POLICY'
        WHEN i.overall_result = 1 THEN 'PASS + FLAG AUTHORIZATION ONLY - REVIEW'
        ELSE 'REVIEW'
    END AS diagnostic_status
FROM dbo.ssi_PieceFlagRecords AS f
JOIN dbo.ssi_Products AS p ON p.product_id=f.product_id
JOIN dbo.ssi_PartNumbers AS pn ON pn.pn_id=p.pn_id
JOIN dbo.ssi_Inspections AS i ON i.inspection_id=f.inspection_id
OUTER APPLY (
    SELECT TOP (1) v.inspection_id, v.overall_result, v.is_scrap, v.finished_at,
           v.authorized_by_user_id, v.authorized_at
    FROM dbo.ssi_Inspections AS v
    WHERE v.product_id=f.product_id
    ORDER BY COALESCE(v.finished_at,v.started_at) DESC,v.inspection_id DESC
) AS latest
ORDER BY f.flag_id DESC;

-- Compare regularized records with latest inspection and authorization.
SELECT
    i.inspection_id,i.product_id,i.overall_result,i.is_scrap,
    i.authorized_by_user_id,i.authorized_at,
    COUNT(f.flag_id) AS flag_count,
    MIN(f.authorized_at) AS first_flag_authorization,
    MAX(f.authorized_at) AS last_flag_authorization
FROM dbo.ssi_Inspections AS i
JOIN dbo.ssi_PieceFlagRecords AS f ON f.inspection_id=i.inspection_id
WHERE i.overall_result=1
GROUP BY i.inspection_id,i.product_id,i.overall_result,i.is_scrap,
         i.authorized_by_user_id,i.authorized_at
ORDER BY i.inspection_id DESC;
