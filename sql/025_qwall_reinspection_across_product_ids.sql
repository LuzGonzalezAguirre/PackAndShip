-- 025_qwall_reinspection_across_product_ids.sql
-- OPTIONAL for clients still using dbo.ssi_PaS_AddPiece stored procedure.
-- Current WinForms PackAndShip executes equivalent inline transactional SQL
-- in PaSDirectSql.AddPiece, so updating the WinForms binary is sufficient there.
-- Execute in CCS only after database-change review; this replaces a procedure
-- but does not change Q-Wall records or current PackAndShip pieces.
-- Do not rerun legacy 009/010/013 scripts afterwards without reapplying 025.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_AddPiece
    @container_id INT,
    @level_number INT,
    @ssi_full_code NVARCHAR(200),
    @client_full_code NVARCHAR(200),
    @actor_user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @container_level_id INT, @required INT, @count INT;
    DECLARE @pn_id INT, @product_id INT, @inspection_id INT;
    DECLARE @inspection_result BIT, @is_scrap BIT, @finished_at DATETIME2;
    DECLARE @ssi NVARCHAR(200) = LTRIM(RTRIM(ISNULL(@ssi_full_code, N'')));
    DECLARE @client NVARCHAR(200) = LTRIM(RTRIM(ISNULL(@client_full_code, N'')));

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users
            WHERE user_id=@actor_user_id AND is_active=1
        )
            THROW 51101, 'An active PackAndShip user is required.', 1;

        IF @ssi=N'' OR @client=N''
            THROW 51102, 'Both full SSI and Client codes are required.', 1;

        -- Serialize writes per container, including level close and pallet close.
        SELECT @pn_id=cfg.pn_id
        FROM dbo.ssi_PaS_Containers AS c WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dbo.ssi_PaS_PalletConfigurations AS cfg
            ON cfg.configuration_id=c.configuration_id
        WHERE c.container_id=@container_id AND c.status='OPEN';

        IF @pn_id IS NULL
            THROW 51103, 'Container is missing or not OPEN.', 1;

        SELECT @container_level_id=container_level_id,
               @required=required_pieces
        FROM dbo.ssi_PaS_ContainerLevels WITH (UPDLOCK, HOLDLOCK)
        WHERE container_id=@container_id AND level_number=@level_number
              AND status='OPEN';

        IF @container_level_id IS NULL
            THROW 51104, 'Level is missing or not OPEN.', 1;

        SELECT @count=COUNT(*)
        FROM dbo.ssi_PaS_ContainerPieces WITH (UPDLOCK, HOLDLOCK)
        WHERE container_level_id=@container_level_id;

        IF @count >= @required
            THROW 51105, 'Level is already at its required piece quantity.', 1;

        -- Q-Wall may create several product rows for the same physical pair of
        -- QR codes after reinspection. They represent ONE piece and one history.
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_Products
            WHERE pn_id=@pn_id AND frameSN=@ssi AND fpcaSN=@client
        )
            THROW 51106, 'Q-Wall piece not found for the two full QR codes.', 1;

        -- Transaction-scoped lock shared by all stations on this physical SN.
        -- Avoid duplicate packing when two different product_id rows exist.
        DECLARE @serial_lock NVARCHAR(255), @lock_result INT;
        SET @serial_lock = N'PaS:serial:' + CONVERT(VARCHAR(64),
            HASHBYTES('SHA2_256', CONCAT(
                CONVERT(NVARCHAR(20),@pn_id), N':', LEN(@ssi), N':', @ssi,
                N':', LEN(@client), N':', @client)), 2);
        EXEC @lock_result = sys.sp_getapplock
            @Resource=@serial_lock, @LockMode='Exclusive',
            @LockOwner='Transaction', @LockTimeout=5000;
        IF @lock_result < 0
            THROW 51110, 'Could not lock serial for packing. Retry.', 1;

        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_ContainerPieces AS cp WITH (UPDLOCK, HOLDLOCK)
            JOIN dbo.ssi_Products AS q ON q.product_id=cp.product_id
            WHERE q.pn_id=@pn_id AND q.frameSN=@ssi AND q.fpcaSN=@client
        )
            THROW 51107, 'Piece already belongs to a container.', 1;

        -- Inspect ALL product records for the same physical SSI/Client SN.
        -- The newest STARTED inspection governs, even if it is unfinished/FAIL.
        SELECT TOP (1)
            @product_id=i.product_id,
            @inspection_id=i.inspection_id,
            @inspection_result=i.overall_result,
            @is_scrap=i.is_scrap,
            @finished_at=i.finished_at
        FROM dbo.ssi_Products AS q WITH (HOLDLOCK)
        JOIN dbo.ssi_Inspections AS i WITH (HOLDLOCK)
          ON i.product_id=q.product_id
        WHERE q.pn_id=@pn_id AND q.frameSN=@ssi AND q.fpcaSN=@client
        ORDER BY COALESCE(i.started_at,i.finished_at) DESC, i.inspection_id DESC;

        IF @inspection_id IS NULL OR @finished_at IS NULL
           OR ISNULL(@inspection_result,0)<>1 OR ISNULL(@is_scrap,0)<>0
            THROW 51108, 'Latest Q-Wall inspection is not finished PASS.', 1;

        -- Confirmed Q-Wall policy: a flag is released when its record
        -- contains authorized_by_user_id and authorized_at. Authorization
        -- is stored on the flag, not necessarily on ssi_Inspections.
        -- Past inspection flags do not automatically block a newer PASS.
        -- Every flag belonging to the latest inspection must be authorized
        -- and tied to this same Q-Wall product.
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PieceFlagRecords WITH (HOLDLOCK)
            WHERE inspection_id=@inspection_id
              AND (product_id<>@product_id
                   OR authorized_by_user_id IS NULL
                   OR authorized_at IS NULL)
        )
            THROW 51109, 'Latest Q-Wall inspection has a Quality flag without valid authorization.', 1;

        INSERT INTO dbo.ssi_PaS_ContainerPieces
            (container_level_id, product_id, verified_inspection_id, scanned_by_user_id)
        VALUES (@container_level_id, @product_id, @inspection_id, @actor_user_id);

        SET @count=@count+1;
        COMMIT TRANSACTION;

        SELECT @product_id AS product_id, @inspection_id AS inspection_id,
               @container_id AS container_id, @level_number AS level_number,
               @count AS scanned_pieces, @required AS required_pieces;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

SELECT name,modify_date FROM sys.procedures
WHERE name=N'ssi_PaS_AddPiece';
