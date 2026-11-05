-- 009_packaging_procedures.sql
-- Run on CCS after sql/008_packaging_tables.sql.
-- This script only creates/replaces PackAndShip stored procedures.
-- It does not insert pieces, change Q-Wall tables or close containers.
-- Each procedure validates the user in ssi_PaS_Users and runs in a transaction.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_OpenContainer
    @container_code VARCHAR(18),
    @pn_id INT,
    @actor_user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @code VARCHAR(18) = LTRIM(RTRIM(ISNULL(@container_code, '')));
    DECLARE @configuration_id INT, @prefix VARCHAR(5), @digits INT;
    DECLARE @container_id INT, @expected_level_count INT, @expected_total INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users
            WHERE user_id=@actor_user_id AND is_active=1
        )
            THROW 51001, 'An active PackAndShip user is required.', 1;

        SELECT @prefix=pallet_prefix, @digits=pallet_digits
        FROM dbo.ssi_PaS_GeneralSettings WITH (HOLDLOCK)
        WHERE settings_id=1;

        IF @prefix IS NULL OR @digits IS NULL
            THROW 51002, 'Global pallet ID settings are missing.', 1;

        IF @code COLLATE Latin1_General_BIN2 NOT LIKE
            (@prefix + REPLICATE('[0-9]', @digits)) COLLATE Latin1_General_BIN2
            OR LEN(@code) <> LEN(@prefix) + @digits
            THROW 51003, 'Container code does not match General Settings.', 1;

        SELECT @configuration_id=configuration_id,
               @expected_total=total_pieces
        FROM dbo.ssi_PaS_PalletConfigurations WITH (UPDLOCK, HOLDLOCK)
        WHERE pn_id=@pn_id AND is_active=1;

        IF @configuration_id IS NULL
            THROW 51004, 'Active pallet configuration not found for this part.', 1;

        SELECT @expected_level_count=COUNT(*)
        FROM dbo.ssi_PaS_PalletLevels
        WHERE configuration_id=@configuration_id;

        IF @expected_level_count = 0
           OR (SELECT COALESCE(SUM(required_pieces), 0)
               FROM dbo.ssi_PaS_PalletLevels
               WHERE configuration_id=@configuration_id) <> @expected_total
           OR (SELECT MAX(level_number) FROM dbo.ssi_PaS_PalletLevels
               WHERE configuration_id=@configuration_id) <> @expected_level_count
            THROW 51005, 'Pallet configuration levels are missing or inconsistent.', 1;

        -- Container codes are never reusable, including after CANCELLED.
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK, HOLDLOCK)
            WHERE container_code=@code
        )
            THROW 51006, 'This container code is already registered.', 1;

        INSERT INTO dbo.ssi_PaS_Containers
            (container_code, configuration_id, status, created_by_user_id)
        VALUES (@code, @configuration_id, 'OPEN', @actor_user_id);
        SET @container_id=CONVERT(INT, SCOPE_IDENTITY());

        INSERT INTO dbo.ssi_PaS_ContainerLevels
            (container_id, level_number, required_pieces)
        SELECT @container_id, level_number, required_pieces
        FROM dbo.ssi_PaS_PalletLevels
        WHERE configuration_id=@configuration_id;

        IF @@ROWCOUNT <> @expected_level_count
            THROW 51007, 'Could not snapshot all configured pallet levels.', 1;

        COMMIT TRANSACTION;

        SELECT @container_id AS container_id, @code AS container_code,
               @pn_id AS pn_id, @expected_level_count AS level_count,
               @expected_total AS required_total, 'OPEN' AS status;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
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

        -- Require exactly one Q-Wall product for the selected part and full QR codes.
        IF (SELECT COUNT(*)
            FROM dbo.ssi_Products
            WHERE pn_id=@pn_id AND frameSN=@ssi AND fpcaSN=@client) <> 1
            THROW 51106, 'Q-Wall product identity is missing or ambiguous.', 1;

        SELECT @product_id=product_id
        FROM dbo.ssi_Products
        WHERE pn_id=@pn_id AND frameSN=@ssi AND fpcaSN=@client;

        -- Unique index also guarantees this across multiple stations.
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_ContainerPieces WITH (UPDLOCK, HOLDLOCK)
            WHERE product_id=@product_id
        )
            THROW 51107, 'Piece already belongs to a container.', 1;

        -- Only the most recent completed-or-started Q-Wall inspection is authoritative.
        SELECT TOP (1)
            @inspection_id=inspection_id,
            @inspection_result=overall_result,
            @is_scrap=is_scrap,
            @finished_at=finished_at
        FROM dbo.ssi_Inspections WITH (HOLDLOCK)
        WHERE product_id=@product_id
        ORDER BY COALESCE(finished_at,started_at) DESC, inspection_id DESC;

        IF @inspection_id IS NULL OR @finished_at IS NULL
           OR ISNULL(@inspection_result,0)<>1 OR ISNULL(@is_scrap,1)<>0
            THROW 51108, 'Latest Q-Wall inspection is not finished PASS.', 1;

        -- Conservative policy: any flag on the latest inspection needs Quality review.
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PieceFlagRecords
            WHERE inspection_id=@inspection_id
        )
            THROW 51109, 'Latest Q-Wall inspection contains a Quality flag.', 1;

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

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_CloseLevel
    @container_id INT,
    @level_number INT,
    @actor_user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @level_id INT, @required INT, @actual INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users
            WHERE user_id=@actor_user_id AND is_active=1
        )
            THROW 51201, 'An active PackAndShip user is required.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK, HOLDLOCK)
            WHERE container_id=@container_id AND status='OPEN'
        )
            THROW 51202, 'Container must be OPEN.', 1;

        SELECT @level_id=container_level_id, @required=required_pieces
        FROM dbo.ssi_PaS_ContainerLevels WITH (UPDLOCK, HOLDLOCK)
        WHERE container_id=@container_id AND level_number=@level_number
              AND status='OPEN';

        IF @level_id IS NULL
            THROW 51203, 'Level is missing or already closed.', 1;

        SELECT @actual=COUNT(*)
        FROM dbo.ssi_PaS_ContainerPieces
        WHERE container_level_id=@level_id;

        IF @actual <> @required
            THROW 51204, 'Level cannot close before the exact required quantity.', 1;

        UPDATE dbo.ssi_PaS_ContainerLevels
        SET status='CLOSED', closed_at=SYSUTCDATETIME(),
            closed_by_user_id=@actor_user_id
        WHERE container_level_id=@level_id AND status='OPEN';

        IF @@ROWCOUNT<>1
            THROW 51205, 'Level status changed during closure.', 1;

        COMMIT TRANSACTION;
        SELECT @container_id AS container_id, @level_number AS level_number,
               'CLOSED' AS status, @actual AS scanned_pieces;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_CloseContainer
    @container_id INT,
    @actor_user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @levels INT, @total_required INT, @total_actual INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Users
            WHERE user_id=@actor_user_id AND is_active=1
        )
            THROW 51301, 'An active PackAndShip user is required.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK, HOLDLOCK)
            WHERE container_id=@container_id AND status='OPEN'
        )
            THROW 51302, 'Container is missing or not OPEN.', 1;

        -- Aggregate verifies each level, rather than only the overall total.
        IF EXISTS (
            SELECT 1 FROM dbo.ssi_PaS_ContainerLevels AS l
            OUTER APPLY (
                SELECT COUNT(*) AS actual
                FROM dbo.ssi_PaS_ContainerPieces AS p
                WHERE p.container_level_id=l.container_level_id
            ) AS counted
            WHERE l.container_id=@container_id
              AND (l.status<>'CLOSED' OR counted.actual<>l.required_pieces)
        )
            THROW 51303, 'All levels must be CLOSED and contain their exact required pieces.', 1;

        SELECT @levels=COUNT(*), @total_required=SUM(required_pieces)
        FROM dbo.ssi_PaS_ContainerLevels
        WHERE container_id=@container_id;

        IF @levels IS NULL OR @levels=0
            THROW 51304, 'Container has no levels.', 1;

        SELECT @total_actual=COUNT(*)
        FROM dbo.ssi_PaS_ContainerPieces AS p
        JOIN dbo.ssi_PaS_ContainerLevels AS l
            ON l.container_level_id=p.container_level_id
        WHERE l.container_id=@container_id;

        IF @total_actual<>@total_required
            THROW 51305, 'Container piece total does not match its required total.', 1;

        UPDATE dbo.ssi_PaS_Containers
        SET status='CLOSED', closed_at=SYSUTCDATETIME(),
            closed_by_user_id=@actor_user_id
        WHERE container_id=@container_id AND status='OPEN';

        IF @@ROWCOUNT<>1
            THROW 51306, 'Container status changed during closure.', 1;

        COMMIT TRANSACTION;

        SELECT @container_id AS container_id, 'CLOSED' AS status,
               @levels AS closed_levels, @total_actual AS scanned_pieces;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- Verify installed procedures (read-only):
SELECT name, create_date, modify_date
FROM sys.procedures
WHERE name IN ('ssi_PaS_OpenContainer','ssi_PaS_AddPiece',
               'ssi_PaS_CloseLevel','ssi_PaS_CloseContainer')
ORDER BY name;
