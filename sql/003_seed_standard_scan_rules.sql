-- PackAndShip / Q-Wall shared scan-rule seed
-- Creates only the two formats confirmed with existing 'por_separador' values:
--   VOLVO (standard only): 2 scans / SSI + client prefix / 11 digits
--   JOHN DEERE: 1 scan / client prefix / 9 digits
-- Does NOT alter existing rules. New rules are INACTIVE until verified.
-- Excluded: 43292.6, 43291.3, 43301.3, 43291.6; Cummins, TULC,
-- Harley-Davidson, Kautex and batch-only Eaton. Their parsing/generation
-- requirements have not been verified against the shared schema.
-- Confirm the candidate list first; set @Apply = 1 to COMMIT.
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Apply bit = 0;

DECLARE @Candidates TABLE
(
    pn_id int PRIMARY KEY,
    ssi_pn nvarchar(40) NOT NULL,
    bu_id int NOT NULL,
    bu_name nvarchar(100) NOT NULL,
    client_pn nvarchar(100) NOT NULL,
    scans int NOT NULL,
    digits int NOT NULL,
    requires_match bit NOT NULL
);

INSERT INTO @Candidates(pn_id,ssi_pn,bu_id,bu_name,client_pn,scans,digits,requires_match)
SELECT p.pn_id, p.ssiPN, b.bu_id, b.bu_name,
       LTRIM(RTRIM(p.volvoProductNumber)),
       CASE WHEN UPPER(REPLACE(b.bu_name,'_',' ')) = 'VOLVO' THEN 2 ELSE 1 END,
       CASE WHEN UPPER(REPLACE(b.bu_name,'_',' ')) = 'VOLVO' THEN 11 ELSE 9 END,
       CASE WHEN UPPER(REPLACE(b.bu_name,'_',' ')) = 'VOLVO' THEN 1 ELSE 0 END
FROM dbo.ssi_PartNumbers p
JOIN dbo.ssi_BusinessUnits b ON b.bu_id = p.bu_id
WHERE UPPER(REPLACE(b.bu_name,'_',' ')) IN ('VOLVO','JOHN DEERE')
  AND p.ssiPN NOT IN ('43292.6','43291.3','43301.3','43291.6')
  AND LEN(p.ssiPN) BETWEEN 1 AND 40
  AND NULLIF(LTRIM(RTRIM(p.volvoProductNumber)),'') IS NOT NULL
  AND LEN(LTRIM(RTRIM(p.volvoProductNumber))) <= 100
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.quality_pn_scan_rules r
      WHERE r.pn_id = p.pn_id AND r.bu_id = p.bu_id
  );

SELECT pn_id, bu_name AS Customer, ssi_pn AS SSI_PN,
       client_pn AS Client_PN, scans, digits, requires_match
FROM @Candidates ORDER BY bu_name, ssi_pn;

DECLARE @Inserted TABLE(pn_id int, rule_id int);
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @pn int, @ssi nvarchar(40), @bu int, @name nvarchar(100),
            @client nvarchar(100), @scans int, @digits int, @match bit, @id int;

    DECLARE seed CURSOR LOCAL FAST_FORWARD FOR
        SELECT pn_id,ssi_pn,bu_id,bu_name,client_pn,scans,digits,requires_match
        FROM @Candidates ORDER BY pn_id;

    OPEN seed;
    FETCH NEXT FROM seed INTO @pn,@ssi,@bu,@name,@client,@scans,@digits,@match;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Recheck while holding a serializable transaction.
        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.quality_pn_scan_rules WITH (UPDLOCK,HOLDLOCK)
            WHERE pn_id=@pn AND bu_id=@bu
        )
        BEGIN
            INSERT INTO dbo.quality_pn_scan_rules
              (pn_id,ssi_pn,bu_id,bu_name,scan_count,requires_match,
               notes,is_active,created_by_id,created_at,updated_by_id,updated_at)
            VALUES
              (@pn,@ssi,@bu,@name,@scans,@match,
               N'Seed from PackAndShip: review before activation',0,
               NULL,SYSDATETIME(),NULL,SYSDATETIME());

            SET @id = CONVERT(int,SCOPE_IDENTITY());
            INSERT INTO @Inserted(pn_id,rule_id) VALUES (@pn,@id);

            IF @scans = 2
            BEGIN
                INSERT INTO dbo.quality_scan_fields
                  (rule_id,scan_index,extraction_mode,field_target,
                   separator,separator_custom,value_position,
                   segment_index,fixed_length,prefix_value,display_label,sequence_order)
                VALUES
                  (@id,0,N'por_separador',N'frameSN',
                   N'espacio',N'',N'despues',NULL,@digits,@ssi,N'SSI Serial',0),
                  (@id,1,N'por_separador',N'volvoSerialNumber',
                   N'espacio',N'',N'despues',NULL,@digits,@client,N'Client Serial',0);
            END
            ELSE
            BEGIN
                INSERT INTO dbo.quality_scan_fields
                  (rule_id,scan_index,extraction_mode,field_target,
                   separator,separator_custom,value_position,
                   segment_index,fixed_length,prefix_value,display_label,sequence_order)
                VALUES
                  (@id,0,N'por_separador',N'volvoSerialNumber',
                   N'espacio',N'',N'despues',NULL,@digits,@client,N'Client Serial',0);
            END
        END

        FETCH NEXT FROM seed INTO @pn,@ssi,@bu,@name,@client,@scans,@digits,@match;
    END
    CLOSE seed;
    DEALLOCATE seed;

    SELECT i.pn_id, i.rule_id, r.ssi_pn, r.bu_name, r.scan_count, r.is_active
    FROM @Inserted i
    JOIN dbo.quality_pn_scan_rules r ON r.id=i.rule_id
    ORDER BY r.bu_name,r.ssi_pn;

    IF @Apply = 1
    BEGIN
        COMMIT TRANSACTION;
        PRINT 'Rules inserted as INACTIVE.';
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT 'DRY RUN ONLY: rolled back; set @Apply=1 after validating candidates.';
    END
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local','seed') >= -1
    BEGIN
        IF CURSOR_STATUS('local','seed') > -1 CLOSE seed;
        DEALLOCATE seed;
    END
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
