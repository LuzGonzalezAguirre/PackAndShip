-- Cummins - proposed scan rules. Shared with Q-Wall.
-- First execute with @Apply=0. No rows persist in dry-run.
-- Allowed extraction_mode 'completo' stores the raw scan; prefix/optional-separator
-- validation still requires parser support
-- before these rules can be activated in PackAndShip.
-- NVARCHAR sizes in the schema are reported in bytes: separator 40 bytes = 20 chars,
-- prefix_value 100 bytes = 50 chars, ssi_pn 40 bytes = 20 chars.
-- Allowed input: CLIENTPN'12345678901, CLIENTPN-12345678901,
--                CLIENTPN12345678901.
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Apply bit = 0;

DECLARE @Candidates TABLE (
  pn_id int PRIMARY KEY,
  ssi_pn nvarchar(40) NOT NULL,
  bu_id int NOT NULL,
  bu_name nvarchar(100) NOT NULL,
  client_pn nvarchar(100) NOT NULL
);

INSERT INTO @Candidates (pn_id, ssi_pn, bu_id, bu_name, client_pn)
SELECT p.pn_id, p.ssiPN, b.bu_id, b.bu_name,
       LTRIM(RTRIM(p.volvoProductNumber))
FROM dbo.ssi_PartNumbers AS p
INNER JOIN dbo.ssi_BusinessUnits AS b ON b.bu_id=p.bu_id
WHERE UPPER(LTRIM(RTRIM(b.bu_name)))='CUMMINS'
  AND LEN(p.ssiPN) BETWEEN 1 AND 20
  AND NULLIF(LTRIM(RTRIM(p.volvoProductNumber)), '') IS NOT NULL
  AND LEN(LTRIM(RTRIM(p.volvoProductNumber)))<=50
  AND LEN(b.bu_name)<=50
  AND NOT EXISTS (
    SELECT 1 FROM dbo.quality_pn_scan_rules AS r
    WHERE r.pn_id=p.pn_id AND r.bu_id=p.bu_id
  );

SELECT pn_id, ssi_pn AS SSI_PN, client_pn AS Client_PN,
       bu_name AS Customer,
       'completo' AS ProposedExtractionMode,
       'INACTIVE' AS ProposedStatus
FROM @Candidates ORDER BY ssi_pn, pn_id;

DECLARE @Inserted TABLE (pn_id int, rule_id int);
BEGIN TRY
  BEGIN TRANSACTION;
  DECLARE @pn int, @ssi nvarchar(40), @bu int,
          @buName nvarchar(100), @client nvarchar(100), @id int;
  DECLARE rows_to_seed CURSOR LOCAL FAST_FORWARD FOR
      SELECT pn_id,ssi_pn,bu_id,bu_name,client_pn
      FROM @Candidates ORDER BY pn_id;

  OPEN rows_to_seed;
  FETCH NEXT FROM rows_to_seed INTO @pn,@ssi,@bu,@buName,@client;
  WHILE @@FETCH_STATUS=0
  BEGIN
    IF NOT EXISTS (
      SELECT 1 FROM dbo.quality_pn_scan_rules WITH (UPDLOCK,HOLDLOCK)
      WHERE pn_id=@pn AND bu_id=@bu
    )
    BEGIN
      INSERT INTO dbo.quality_pn_scan_rules
        (pn_id, ssi_pn, bu_id, bu_name, scan_count, requires_match,
         notes, is_active, created_by_id, created_at, updated_by_id, updated_at)
      VALUES
        (@pn, @ssi, @bu, @buName, 1, 0,
         N'Cummins: Client PN + optional apostrophe/hyphen + exactly 11 digits. Parser pending: validate prefix, optional apostrophe/hyphen, and 11 digits before use.',
         0, NULL, SYSDATETIME(), NULL, SYSDATETIME());
      SET @id=CONVERT(int,SCOPE_IDENTITY());
      INSERT INTO @Inserted VALUES (@pn,@id);

      -- Q-Wall Form1 assigns the same scanned serial to both fields.
      -- These two extraction mappings intentionally share scan_index 0.
      -- 'completo' is valid under current CHECK constraints and retains the raw scan.
      -- It does NOT enforce prefix/length/optional separators by itself.
      INSERT INTO dbo.quality_scan_fields
        (rule_id, scan_index, extraction_mode, field_target,
         separator, separator_custom, value_position, segment_index,
         fixed_length, prefix_value, display_label, sequence_order)
      VALUES
        (@id,0,N'completo',N'frameSN',
         N'ninguno',N'',N'completo',NULL,NULL,@client,
         N'Internal serial',0),
        (@id,0,N'completo',N'volvoSerialNumber',
         N'ninguno',N'',N'completo',NULL,NULL,@client,
         N'Client serial',1);
    END
    FETCH NEXT FROM rows_to_seed INTO @pn,@ssi,@bu,@buName,@client;
  END
  CLOSE rows_to_seed;
  DEALLOCATE rows_to_seed;

  SELECT i.pn_id, i.rule_id, r.ssi_pn, r.bu_name, r.scan_count, r.is_active
  FROM @Inserted i
  JOIN dbo.quality_pn_scan_rules r ON r.id=i.rule_id
  ORDER BY r.ssi_pn;

  IF @Apply=1
  BEGIN
    COMMIT TRANSACTION;
    PRINT 'Cummins rules saved INACTIVE. Parser implementation is still required.';
  END
  ELSE
  BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'DRY RUN: no changes were saved. Review candidates before setting @Apply=1.';
  END
END TRY
BEGIN CATCH
  IF CURSOR_STATUS('local','rows_to_seed')>=-1
  BEGIN
    IF CURSOR_STATUS('local','rows_to_seed')>-1 CLOSE rows_to_seed;
    DEALLOCATE rows_to_seed;
  END
  IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
  THROW;
END CATCH;
