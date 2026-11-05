-- 010_fix_qwall_nullable_scrap.sql
-- Hotfix for CCS: in Q-Wall, is_scrap NULL means not flagged as scrap.
-- Replaces only ssi_PaS_AddPiece. Does not change existing data.
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
           OR ISNULL(@inspection_result,0)<>1 OR ISNULL(@is_scrap,0)<>0
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

SELECT name, modify_date FROM sys.procedures
WHERE name = N'ssi_PaS_AddPiece';
