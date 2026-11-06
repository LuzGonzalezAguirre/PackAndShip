-- 018_pallet_transfers.sql
-- CCS deployment after 017_partial_pallet_and_lock.sql.
-- Transfer ALL pieces from one authorized PARTIAL pallet to a NEW pallet code.
-- One transaction, original Q-Wall identity preserved, complete transfer audit.
-- Source status TRANSFERRED is terminal; destination status OPEN until normal close.
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.ssi_PaS_PartialClosures',N'U') IS NULL OR
       OBJECT_ID(N'dbo.ssi_PaS_PackingAudit',N'U') IS NULL
        THROW 51800,'Run SQL scripts 014 and 017 before pallet transfers.',1;

    ALTER TABLE dbo.ssi_PaS_Containers DROP CONSTRAINT CK_ssi_PaS_Containers_Status;
    ALTER TABLE dbo.ssi_PaS_Containers
        ADD CONSTRAINT CK_ssi_PaS_Containers_Status
        CHECK (status IN ('OPEN','CLOSED','CANCELLED','PARTIAL','TRANSFERRED'));

    ALTER TABLE dbo.ssi_PaS_Containers DROP CONSTRAINT CK_ssi_PaS_Containers_Closure;
    ALTER TABLE dbo.ssi_PaS_Containers
        ADD CONSTRAINT CK_ssi_PaS_Containers_Closure
        CHECK ((status IN ('CLOSED','PARTIAL','TRANSFERRED')
                AND closed_at IS NOT NULL AND closed_by_user_id IS NOT NULL)
               OR (status IN ('OPEN','CANCELLED')
                AND closed_at IS NULL AND closed_by_user_id IS NULL));

    ALTER TABLE dbo.ssi_PaS_PackingAudit DROP CONSTRAINT CK_ssi_PaS_PackingAudit_Event;
    ALTER TABLE dbo.ssi_PaS_PackingAudit ADD CONSTRAINT CK_ssi_PaS_PackingAudit_Event
        CHECK (event_type IN ('OPEN_CONTAINER','ADD_PIECE','REMOVE_PIECE',
            'CLOSE_LEVEL','CLOSE_CONTAINER','CANCEL_CONTAINER',
            'CLOSE_PARTIAL','TRANSFER_OUT','TRANSFER_IN'));

    IF OBJECT_ID(N'dbo.ssi_PaS_PalletTransfers',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_PalletTransfers (
            transfer_id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            source_container_id INT NOT NULL,
            destination_container_id INT NOT NULL,
            transferred_pieces INT NOT NULL,
            transferred_by_user_id INT NOT NULL,
            transferred_at DATETIME2(3) NOT NULL
                CONSTRAINT DF_ssi_PaS_PalletTransfers_Date DEFAULT SYSUTCDATETIME(),
            CONSTRAINT UQ_ssi_PaS_PalletTransfers_Source UNIQUE(source_container_id),
            CONSTRAINT UQ_ssi_PaS_PalletTransfers_Destination UNIQUE(destination_container_id),
            CONSTRAINT FK_ssi_PaS_PalletTransfers_Source FOREIGN KEY(source_container_id)
                REFERENCES dbo.ssi_PaS_Containers(container_id),
            CONSTRAINT FK_ssi_PaS_PalletTransfers_Destination FOREIGN KEY(destination_container_id)
                REFERENCES dbo.ssi_PaS_Containers(container_id),
            CONSTRAINT FK_ssi_PaS_PalletTransfers_User FOREIGN KEY(transferred_by_user_id)
                REFERENCES dbo.ssi_PaS_Users(user_id),
            CONSTRAINT CK_ssi_PaS_PalletTransfers_Different
                CHECK(source_container_id <> destination_container_id),
            CONSTRAINT CK_ssi_PaS_PalletTransfers_Quantity CHECK(transferred_pieces > 0)
        );
    END;

    IF OBJECT_ID(N'dbo.ssi_PaS_PalletTransferPieces',N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ssi_PaS_PalletTransferPieces (
            transfer_id BIGINT NOT NULL,
            container_piece_id BIGINT NOT NULL,
            product_id INT NOT NULL,
            verified_inspection_id INT NOT NULL,
            original_scanned_by_user_id INT NOT NULL,
            original_scanned_at DATETIME2(3) NOT NULL,
            source_level_id INT NOT NULL,
            destination_level_id INT NOT NULL,
            CONSTRAINT PK_ssi_PaS_PalletTransferPieces
                PRIMARY KEY(transfer_id,container_piece_id),
            CONSTRAINT FK_ssi_PaS_PalletTransferPieces_Transfer FOREIGN KEY(transfer_id)
                REFERENCES dbo.ssi_PaS_PalletTransfers(transfer_id),
            CONSTRAINT FK_ssi_PaS_PalletTransferPieces_Product FOREIGN KEY(product_id)
                REFERENCES dbo.ssi_Products(product_id),
            CONSTRAINT FK_ssi_PaS_PalletTransferPieces_SourceLevel FOREIGN KEY(source_level_id)
                REFERENCES dbo.ssi_PaS_ContainerLevels(container_level_id),
            CONSTRAINT FK_ssi_PaS_PalletTransferPieces_DestLevel FOREIGN KEY(destination_level_id)
                REFERENCES dbo.ssi_PaS_ContainerLevels(container_level_id)
        );
    END;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

CREATE OR ALTER PROCEDURE dbo.ssi_PaS_TransferPartialPallet
    @source_code VARCHAR(18),
    @destination_code VARCHAR(18),
    @actor_user_id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @src VARCHAR(18)=UPPER(LTRIM(RTRIM(ISNULL(@source_code,''))));
    DECLARE @dst VARCHAR(18)=UPPER(LTRIM(RTRIM(ISNULL(@destination_code,''))));
    DECLARE @source_id INT,@destination_id INT,@config_id INT,@pn_id INT;
    DECLARE @prefix VARCHAR(5),@digits INT,@required INT,@actual INT,@transfer_id BIGINT;
    DECLARE @source_closed_user INT,@source_closed_at DATETIME2(3);
    DECLARE @movement TABLE (
        piece_id BIGINT NOT NULL PRIMARY KEY, product_id INT NOT NULL,
        inspection_id INT NOT NULL, scanned_by_user_id INT NOT NULL,
        scanned_at DATETIME2(3) NOT NULL,
        from_level_id INT NOT NULL, to_level_id INT NOT NULL
    );

    BEGIN TRY
        BEGIN TRANSACTION;
        IF @src='' OR @dst='' OR @src=@dst
            THROW 51801,'Scan two different pallet codes.',1;

        IF NOT EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_Users WITH (HOLDLOCK)
            WHERE user_id=@actor_user_id AND is_active=1
        ) THROW 51802,'An active PackAndShip user is required.',1;

        SELECT @prefix=pallet_prefix,@digits=pallet_digits
        FROM dbo.ssi_PaS_GeneralSettings WITH (HOLDLOCK) WHERE settings_id=1;
        IF @prefix IS NULL OR @digits IS NULL OR
           @dst COLLATE Latin1_General_BIN2 NOT LIKE
              (@prefix+REPLICATE('[0-9]',@digits)) COLLATE Latin1_General_BIN2 OR
           LEN(@dst)<>LEN(@prefix)+@digits
           THROW 51803,'Destination code does not match General Settings.',1;

        SELECT @source_id=c.container_id,@config_id=c.configuration_id,
               @source_closed_user=c.closed_by_user_id,
               @source_closed_at=c.closed_at,@pn_id=cfg.pn_id
        FROM dbo.ssi_PaS_Containers c WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.ssi_PaS_PalletConfigurations cfg ON cfg.configuration_id=c.configuration_id
        WHERE c.container_code=@src AND c.status='PARTIAL';

        IF @source_id IS NULL
            THROW 51804,'Source pallet is not PARTIAL or does not exist.',1;
        IF NOT EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_PartialClosures
            WHERE container_id=@source_id
        ) THROW 51805,'Source has no authorized partial-close record.',1;
        IF EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_PalletTransfers WITH (UPDLOCK,HOLDLOCK)
            WHERE source_container_id=@source_id
        ) THROW 51806,'This source pallet was already transferred.',1;
        IF EXISTS(
            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)
            WHERE container_code=@dst
        ) THROW 51807,'Destination pallet code already exists.',1;

        SELECT @required=SUM(required_pieces)
        FROM dbo.ssi_PaS_ContainerLevels WITH (HOLDLOCK)
        WHERE container_id=@source_id;

        SELECT @actual=COUNT(*)
        FROM dbo.ssi_PaS_ContainerPieces p WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.ssi_PaS_ContainerLevels l
            ON l.container_level_id=p.container_level_id
        WHERE l.container_id=@source_id;

        IF @required IS NULL OR @actual IS NULL OR @actual<=0 OR @actual>=@required
            THROW 51808,'Source must contain an incomplete, nonempty pallet.',1;

        -- Q-Wall may have changed since packing. Recheck the latest inspection
        -- before any piece is reassigned to the new physical container.
        IF EXISTS(
            SELECT 1
            FROM dbo.ssi_PaS_ContainerPieces cp
            JOIN dbo.ssi_PaS_ContainerLevels sl ON sl.container_level_id=cp.container_level_id
            JOIN dbo.ssi_Products q ON q.product_id=cp.product_id
            OUTER APPLY (
                SELECT TOP (1) i.inspection_id,i.overall_result,i.is_scrap,i.finished_at
                FROM dbo.ssi_Inspections i WITH (HOLDLOCK)
                WHERE i.product_id=cp.product_id
                ORDER BY COALESCE(i.finished_at,i.started_at) DESC,i.inspection_id DESC
            ) latest
            WHERE sl.container_id=@source_id
              AND (q.pn_id<>@pn_id OR latest.inspection_id IS NULL
                   OR latest.finished_at IS NULL
                   OR ISNULL(latest.overall_result,0)<>1 OR ISNULL(latest.is_scrap,0)<>0
                   OR EXISTS(
                       SELECT 1 FROM dbo.ssi_PieceFlagRecords f WITH (HOLDLOCK)
                       WHERE f.inspection_id=latest.inspection_id
                         AND (f.product_id<>cp.product_id
                              OR f.authorized_by_user_id IS NULL
                              OR f.authorized_at IS NULL)
                   ))
        ) THROW 51809,'Q-Wall no longer confirms PASS for every source piece.',1;

        -- Preserve the source configuration snapshot even if Setup was changed
        -- after that partial pallet was first created.
        INSERT dbo.ssi_PaS_Containers
            (container_code,configuration_id,status,created_by_user_id)
        VALUES(@dst,@config_id,'OPEN',@actor_user_id);
        SET @destination_id=CONVERT(INT,SCOPE_IDENTITY());

        INSERT dbo.ssi_PaS_ContainerLevels
            (container_id,level_number,required_pieces)
        SELECT @destination_id,level_number,required_pieces
        FROM dbo.ssi_PaS_ContainerLevels
        WHERE container_id=@source_id;

        -- Preserve the exact original scan timestamp/operator and product_id.
        -- Map pieces sequentially into the new pallet's level snapshot.
        ;WITH SourceOrdered AS (
            SELECT cp.container_piece_id,cp.product_id,cp.verified_inspection_id,
                   cp.scanned_by_user_id,cp.scanned_at,
                   cp.container_level_id AS source_level_id,
                   ROW_NUMBER() OVER(ORDER BY sl.level_number,cp.scanned_at,cp.container_piece_id) AS rn
            FROM dbo.ssi_PaS_ContainerPieces cp
            JOIN dbo.ssi_PaS_ContainerLevels sl
                ON sl.container_level_id=cp.container_level_id
            WHERE sl.container_id=@source_id
        ),
        DestLevels AS (
            SELECT container_level_id,required_pieces,
                SUM(required_pieces) OVER (ORDER BY level_number ROWS UNBOUNDED PRECEDING) AS end_piece
            FROM dbo.ssi_PaS_ContainerLevels
            WHERE container_id=@destination_id
        )
        INSERT @movement(piece_id,product_id,inspection_id,scanned_by_user_id,scanned_at,
                         from_level_id,to_level_id)
        SELECT s.container_piece_id,s.product_id,s.verified_inspection_id,
               s.scanned_by_user_id,s.scanned_at,s.source_level_id,d.container_level_id
        FROM SourceOrdered s
        JOIN DestLevels d ON s.rn > d.end_piece-d.required_pieces AND s.rn<=d.end_piece;

        IF (SELECT COUNT(*) FROM @movement)<>@actual
            THROW 51810,'Could not assign every source piece to a destination level.',1;

        INSERT dbo.ssi_PaS_PalletTransfers
            (source_container_id,destination_container_id,transferred_pieces,transferred_by_user_id)
        VALUES(@source_id,@destination_id,@actual,@actor_user_id);
        SET @transfer_id=CONVERT(BIGINT,SCOPE_IDENTITY());

        INSERT dbo.ssi_PaS_PalletTransferPieces
            (transfer_id,container_piece_id,product_id,verified_inspection_id,
             original_scanned_by_user_id,original_scanned_at,
             source_level_id,destination_level_id)
        SELECT @transfer_id,piece_id,product_id,inspection_id,scanned_by_user_id,
               scanned_at,from_level_id,to_level_id
        FROM @movement;

        UPDATE cp SET container_level_id=m.to_level_id
        FROM dbo.ssi_PaS_ContainerPieces cp
        JOIN @movement m ON m.piece_id=cp.container_piece_id;
        IF @@ROWCOUNT<>@actual
            THROW 51811,'Not all pieces were transferred. All changes rolled back.',1;

        -- Close only the full destination levels; its first partial level stays OPEN.
        UPDATE dl SET status='CLOSED',closed_at=SYSUTCDATETIME(),
                      closed_by_user_id=@actor_user_id
        FROM dbo.ssi_PaS_ContainerLevels dl
        CROSS APPLY (
            SELECT COUNT(*) AS amount
            FROM dbo.ssi_PaS_ContainerPieces dp
            WHERE dp.container_level_id=dl.container_level_id
        ) n
        WHERE dl.container_id=@destination_id AND dl.status='OPEN'
          AND n.amount=dl.required_pieces;

        -- Terminal source status: no additional pieces can be added.
        UPDATE dbo.ssi_PaS_Containers
        SET status='TRANSFERRED'
        WHERE container_id=@source_id AND status='PARTIAL';
        IF @@ROWCOUNT<>1
            THROW 51812,'Source status changed. Transfer was rolled back.',1;

        INSERT dbo.ssi_PaS_PackingAudit(event_type,container_id,actor_user_id,reason)
        VALUES
            ('TRANSFER_OUT',@source_id,@actor_user_id,'Transferred all accepted pieces to '+@dst),
            ('TRANSFER_IN',@destination_id,@actor_user_id,'Received '+CAST(@actual AS VARCHAR(12))+' pieces from '+@src);

        COMMIT TRANSACTION;

        SELECT @transfer_id AS transfer_id,@source_id AS source_container_id,
               @destination_id AS destination_container_id,
               @actual AS transferred_pieces,@required AS required_pieces,
               @src AS source_code,@dst AS destination_code;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

IF DATABASE_PRINCIPAL_ID(N'ssi_PaS_RuntimeRole') IS NOT NULL
BEGIN
    GRANT EXECUTE ON dbo.ssi_PaS_TransferPartialPallet TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_PalletTransfers TO ssi_PaS_RuntimeRole;
    GRANT SELECT ON dbo.ssi_PaS_PalletTransferPieces TO ssi_PaS_RuntimeRole;
    DENY INSERT,UPDATE,DELETE ON dbo.ssi_PaS_PalletTransfers TO ssi_PaS_RuntimeRole;
    DENY INSERT,UPDATE,DELETE ON dbo.ssi_PaS_PalletTransferPieces TO ssi_PaS_RuntimeRole;
END;
GO
