' AUTO-PORT of validated CCS procedure BODIES to parameterized SqlCommand.Text.
' No CREATE/ALTER/EXEC permissions are needed. Requires table-level CRUD permissions.
' Keep transaction, Q-Wall checks, uniqueness, approvals and audit triggers unchanged.
' Source of truth: sql/009,013,014,020,021,022. Do not hand-edit without schema review.
Public NotInheritable Class PaSDirectSql
    Private Sub New()
    End Sub

    Public Shared ReadOnly Property OpenContainer As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "" & vbCrLf & _
            "    DECLARE @code VARCHAR(18) = LTRIM(RTRIM(ISNULL(@container_code, '')));" & vbCrLf & _
            "    DECLARE @configuration_id INT, @prefix VARCHAR(5), @digits INT;" & vbCrLf & _
            "    DECLARE @container_id INT, @expected_level_count INT, @expected_total INT;" & vbCrLf & _
            "" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "            WHERE user_id=@actor_user_id AND is_active=1" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51001, 'An active PackAndShip user is required.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @prefix=pallet_prefix, @digits=pallet_digits" & vbCrLf & _
            "        FROM dbo.ssi_PaS_GeneralSettings WITH (HOLDLOCK)" & vbCrLf & _
            "        WHERE settings_id=1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @prefix IS NULL OR @digits IS NULL" & vbCrLf & _
            "            THROW 51002, 'Global pallet ID settings are missing.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @code COLLATE Latin1_General_BIN2 NOT LIKE" & vbCrLf & _
            "            (@prefix + REPLICATE('[0-9]', @digits)) COLLATE Latin1_General_BIN2" & vbCrLf & _
            "            OR LEN(@code) <> LEN(@prefix) + @digits" & vbCrLf & _
            "            THROW 51003, 'Container code does not match General Settings.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @configuration_id=configuration_id," & vbCrLf & _
            "               @expected_total=total_pieces" & vbCrLf & _
            "        FROM dbo.ssi_PaS_PalletConfigurations WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "        WHERE pn_id=@pn_id AND is_active=1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @configuration_id IS NULL" & vbCrLf & _
            "            THROW 51004, 'Active pallet configuration not found for this part.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @expected_level_count=COUNT(*)" & vbCrLf & _
            "        FROM dbo.ssi_PaS_PalletLevels" & vbCrLf & _
            "        WHERE configuration_id=@configuration_id;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @expected_level_count = 0" & vbCrLf & _
            "           OR (SELECT COALESCE(SUM(required_pieces), 0)" & vbCrLf & _
            "               FROM dbo.ssi_PaS_PalletLevels" & vbCrLf & _
            "               WHERE configuration_id=@configuration_id) <> @expected_total" & vbCrLf & _
            "           OR (SELECT MAX(level_number) FROM dbo.ssi_PaS_PalletLevels" & vbCrLf & _
            "               WHERE configuration_id=@configuration_id) <> @expected_level_count" & vbCrLf & _
            "            THROW 51005, 'Pallet configuration levels are missing or inconsistent.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Container codes are never reusable, including after CANCELLED." & vbCrLf & _
            "        IF EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "            WHERE container_code=@code" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51006, 'This container code is already registered.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        INSERT INTO dbo.ssi_PaS_Containers" & vbCrLf & _
            "            (container_code, configuration_id, status, created_by_user_id)" & vbCrLf & _
            "        VALUES (@code, @configuration_id, 'OPEN', @actor_user_id);" & vbCrLf & _
            "        SET @container_id=CONVERT(INT, SCOPE_IDENTITY());" & vbCrLf & _
            "" & vbCrLf & _
            "        INSERT INTO dbo.ssi_PaS_ContainerLevels" & vbCrLf & _
            "            (container_id, level_number, required_pieces)" & vbCrLf & _
            "        SELECT @container_id, level_number, required_pieces" & vbCrLf & _
            "        FROM dbo.ssi_PaS_PalletLevels" & vbCrLf & _
            "        WHERE configuration_id=@configuration_id;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @@ROWCOUNT <> @expected_level_count" & vbCrLf & _
            "            THROW 51007, 'Could not snapshot all configured pallet levels.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @container_id AS container_id, @code AS container_code," & vbCrLf & _
            "               @pn_id AS pn_id, @expected_level_count AS level_count," & vbCrLf & _
            "               @expected_total AS required_total, 'OPEN' AS status;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH;" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property CloseLevel As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @level_id INT, @required INT, @actual INT;" & vbCrLf & _
            "" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "            WHERE user_id=@actor_user_id AND is_active=1" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51201, 'An active PackAndShip user is required.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "            WHERE container_id=@container_id AND status='OPEN'" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51202, 'Container must be OPEN.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @level_id=container_level_id, @required=required_pieces" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerLevels WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "        WHERE container_id=@container_id AND level_number=@level_number" & vbCrLf & _
            "              AND status='OPEN';" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @level_id IS NULL" & vbCrLf & _
            "            THROW 51203, 'Level is missing or already closed.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @actual=COUNT(*)" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerPieces" & vbCrLf & _
            "        WHERE container_level_id=@level_id;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @actual <> @required" & vbCrLf & _
            "            THROW 51204, 'Level cannot close before the exact required quantity.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        UPDATE dbo.ssi_PaS_ContainerLevels" & vbCrLf & _
            "        SET status='CLOSED', closed_at=SYSUTCDATETIME()," & vbCrLf & _
            "            closed_by_user_id=@actor_user_id" & vbCrLf & _
            "        WHERE container_level_id=@level_id AND status='OPEN';" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @@ROWCOUNT<>1" & vbCrLf & _
            "            THROW 51205, 'Level status changed during closure.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "        SELECT @container_id AS container_id, @level_number AS level_number," & vbCrLf & _
            "               'CLOSED' AS status, @actual AS scanned_pieces;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH;" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property CloseContainer As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @levels INT, @total_required INT, @total_actual INT;" & vbCrLf & _
            "" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "            WHERE user_id=@actor_user_id AND is_active=1" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51301, 'An active PackAndShip user is required.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "            WHERE container_id=@container_id AND status='OPEN'" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51302, 'Container is missing or not OPEN.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Aggregate verifies each level, rather than only the overall total." & vbCrLf & _
            "        IF EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_ContainerLevels AS l" & vbCrLf & _
            "            OUTER APPLY (" & vbCrLf & _
            "                SELECT COUNT(*) AS actual" & vbCrLf & _
            "                FROM dbo.ssi_PaS_ContainerPieces AS p" & vbCrLf & _
            "                WHERE p.container_level_id=l.container_level_id" & vbCrLf & _
            "            ) AS counted" & vbCrLf & _
            "            WHERE l.container_id=@container_id" & vbCrLf & _
            "              AND (l.status<>'CLOSED' OR counted.actual<>l.required_pieces)" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51303, 'All levels must be CLOSED and contain their exact required pieces.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @levels=COUNT(*), @total_required=SUM(required_pieces)" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerLevels" & vbCrLf & _
            "        WHERE container_id=@container_id;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @levels IS NULL OR @levels=0" & vbCrLf & _
            "            THROW 51304, 'Container has no levels.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @total_actual=COUNT(*)" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerPieces AS p" & vbCrLf & _
            "        JOIN dbo.ssi_PaS_ContainerLevels AS l" & vbCrLf & _
            "            ON l.container_level_id=p.container_level_id" & vbCrLf & _
            "        WHERE l.container_id=@container_id;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @total_actual<>@total_required" & vbCrLf & _
            "            THROW 51305, 'Container piece total does not match its required total.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        UPDATE dbo.ssi_PaS_Containers" & vbCrLf & _
            "        SET status='CLOSED', closed_at=SYSUTCDATETIME()," & vbCrLf & _
            "            closed_by_user_id=@actor_user_id" & vbCrLf & _
            "        WHERE container_id=@container_id AND status='OPEN';" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @@ROWCOUNT<>1" & vbCrLf & _
            "            THROW 51306, 'Container status changed during closure.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @container_id AS container_id, 'CLOSED' AS status," & vbCrLf & _
            "               @levels AS closed_levels, @total_actual AS scanned_pieces;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH;" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property AddPiece As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "" & vbCrLf & _
            "    DECLARE @container_level_id INT, @required INT, @count INT;" & vbCrLf & _
            "    DECLARE @pn_id INT, @product_id INT, @inspection_id INT;" & vbCrLf & _
            "    DECLARE @inspection_result BIT, @is_scrap BIT, @finished_at DATETIME2;" & vbCrLf & _
            "    DECLARE @ssi NVARCHAR(200) = LTRIM(RTRIM(ISNULL(@ssi_full_code, N'')));" & vbCrLf & _
            "    DECLARE @client NVARCHAR(200) = LTRIM(RTRIM(ISNULL(@client_full_code, N'')));" & vbCrLf & _
            "" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "            WHERE user_id=@actor_user_id AND is_active=1" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51101, 'An active PackAndShip user is required.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @ssi=N'' OR @client=N''" & vbCrLf & _
            "            THROW 51102, 'Both full SSI and Client codes are required.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Serialize writes per container, including level close and pallet close." & vbCrLf & _
            "        SELECT @pn_id=cfg.pn_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_Containers AS c WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "        INNER JOIN dbo.ssi_PaS_PalletConfigurations AS cfg" & vbCrLf & _
            "            ON cfg.configuration_id=c.configuration_id" & vbCrLf & _
            "        WHERE c.container_id=@container_id AND c.status='OPEN';" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @pn_id IS NULL" & vbCrLf & _
            "            THROW 51103, 'Container is missing or not OPEN.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @container_level_id=container_level_id," & vbCrLf & _
            "               @required=required_pieces" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerLevels WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "        WHERE container_id=@container_id AND level_number=@level_number" & vbCrLf & _
            "              AND status='OPEN';" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @container_level_id IS NULL" & vbCrLf & _
            "            THROW 51104, 'Level is missing or not OPEN.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @count=COUNT(*)" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerPieces WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "        WHERE container_level_id=@container_level_id;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @count >= @required" & vbCrLf & _
            "            THROW 51105, 'Level is already at its required piece quantity.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Require exactly one Q-Wall product for the selected part and full QR codes." & vbCrLf & _
            "        IF (SELECT COUNT(*)" & vbCrLf & _
            "            FROM dbo.ssi_Products" & vbCrLf & _
            "            WHERE pn_id=@pn_id AND frameSN=@ssi AND fpcaSN=@client) <> 1" & vbCrLf & _
            "            THROW 51106, 'Q-Wall product identity is missing or ambiguous.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @product_id=product_id" & vbCrLf & _
            "        FROM dbo.ssi_Products" & vbCrLf & _
            "        WHERE pn_id=@pn_id AND frameSN=@ssi AND fpcaSN=@client;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Unique index also guarantees this across multiple stations." & vbCrLf & _
            "        IF EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_ContainerPieces WITH (UPDLOCK, HOLDLOCK)" & vbCrLf & _
            "            WHERE product_id=@product_id" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51107, 'Piece already belongs to a container.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Only the most recent completed-or-started Q-Wall inspection is authoritative." & vbCrLf & _
            "        SELECT TOP (1)" & vbCrLf & _
            "            @inspection_id=inspection_id," & vbCrLf & _
            "            @inspection_result=overall_result," & vbCrLf & _
            "            @is_scrap=is_scrap," & vbCrLf & _
            "            @finished_at=finished_at" & vbCrLf & _
            "        FROM dbo.ssi_Inspections WITH (HOLDLOCK)" & vbCrLf & _
            "        WHERE product_id=@product_id" & vbCrLf & _
            "        ORDER BY COALESCE(finished_at,started_at) DESC, inspection_id DESC;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @inspection_id IS NULL OR @finished_at IS NULL" & vbCrLf & _
            "           OR ISNULL(@inspection_result,0)<>1 OR ISNULL(@is_scrap,0)<>0" & vbCrLf & _
            "            THROW 51108, 'Latest Q-Wall inspection is not finished PASS.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Confirmed Q-Wall policy: a flag is released when its record" & vbCrLf & _
            "        -- contains authorized_by_user_id and authorized_at. Authorization" & vbCrLf & _
            "        -- is stored on the flag, not necessarily on ssi_Inspections." & vbCrLf & _
            "        -- Past inspection flags do not automatically block a newer PASS." & vbCrLf & _
            "        -- Every flag belonging to the latest inspection must be authorized" & vbCrLf & _
            "        -- and tied to this same Q-Wall product." & vbCrLf & _
            "        IF EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PieceFlagRecords WITH (HOLDLOCK)" & vbCrLf & _
            "            WHERE inspection_id=@inspection_id" & vbCrLf & _
            "              AND (product_id<>@product_id" & vbCrLf & _
            "                   OR authorized_by_user_id IS NULL" & vbCrLf & _
            "                   OR authorized_at IS NULL)" & vbCrLf & _
            "        )" & vbCrLf & _
            "            THROW 51109, 'Latest Q-Wall inspection has a Quality flag without valid authorization.', 1;" & vbCrLf & _
            "" & vbCrLf & _
            "        INSERT INTO dbo.ssi_PaS_ContainerPieces" & vbCrLf & _
            "            (container_level_id, product_id, verified_inspection_id, scanned_by_user_id)" & vbCrLf & _
            "        VALUES (@container_level_id, @product_id, @inspection_id, @actor_user_id);" & vbCrLf & _
            "" & vbCrLf & _
            "        SET @count=@count+1;" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @product_id AS product_id, @inspection_id AS inspection_id," & vbCrLf & _
            "               @container_id AS container_id, @level_number AS level_number," & vbCrLf & _
            "               @count AS scanned_pieces, @required AS required_pieces;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH;" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property ApproveScanRejection As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @user_id INT, @role_id INT, @detector INT;" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "        SELECT @detector=detected_by_user_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ScanRejections WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        WHERE rejection_id=@rejection_id AND status='BLOCKED';" & vbCrLf & _
            "        IF @detector IS NULL THROW 52011,'El rechazo no esta pendiente.',1;" & vbCrLf & _
            "        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionRequirements" & vbCrLf & _
            "                       WHERE rejection_id=@rejection_id AND department_id=@department_id)" & vbCrLf & _
            "            THROW 52012,'El departamento no es requerido para este rechazo.',1;" & vbCrLf & _
            "        SELECT @user_id=user_id,@role_id=role_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "        WHERE barcode_id=UPPER(LTRIM(RTRIM(@barcode))) AND is_active=1" & vbCrLf & _
            "          AND password_hash=HASHBYTES('SHA2_256',CONVERT(VARCHAR(50),@password));" & vbCrLf & _
            "        IF @user_id IS NULL THROW 52013,'Credenciales incorrectas o usuario inactivo.',1;" & vbCrLf & _
            "        IF @user_id=@detector THROW 52014,'El operador del rechazo no puede autorizarse a si mismo.',1;" & vbCrLf & _
            "        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionDepartmentRoles" & vbCrLf & _
            "                       WHERE department_id=@department_id AND role_id=@role_id)" & vbCrLf & _
            "            THROW 52015,'El rol del usuario no esta autorizado para este departamento.',1;" & vbCrLf & _
            "        IF EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionApprovals" & vbCrLf & _
            "                   WHERE rejection_id=@rejection_id AND authorized_by_user_id=@user_id)" & vbCrLf & _
            "            THROW 52016,'Una persona solo puede aprobar un departamento por rechazo.',1;" & vbCrLf & _
            "        IF EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionApprovals" & vbCrLf & _
            "                   WHERE rejection_id=@rejection_id AND department_id=@department_id)" & vbCrLf & _
            "            THROW 52017,'Este departamento ya aprobo el rechazo.',1;" & vbCrLf & _
            "        INSERT dbo.ssi_PaS_RejectionApprovals(rejection_id,department_id,authorized_by_user_id)" & vbCrLf & _
            "        VALUES (@rejection_id,@department_id,@user_id);" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_RejectionRequirements rr" & vbCrLf & _
            "            WHERE rr.rejection_id=@rejection_id AND NOT EXISTS (" & vbCrLf & _
            "                SELECT 1 FROM dbo.ssi_PaS_RejectionApprovals a" & vbCrLf & _
            "                WHERE a.rejection_id=rr.rejection_id AND a.department_id=rr.department_id)" & vbCrLf & _
            "        ) AND EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionRequirements WHERE rejection_id=@rejection_id)" & vbCrLf & _
            "            UPDATE dbo.ssi_PaS_ScanRejections SET status='APPROVED',resolved_at=SYSUTCDATETIME()" & vbCrLf & _
            "            WHERE rejection_id=@rejection_id AND status='BLOCKED';" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "        SELECT status FROM dbo.ssi_PaS_ScanRejections WHERE rejection_id=@rejection_id;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property SaveRejectionDepartment As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "        IF NOT EXISTS(" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Users u" & vbCrLf & _
            "            JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id" & vbCrLf & _
            "            WHERE u.user_id=@actor_user_id AND u.is_active=1 AND r.can_modify_setup=1" & vbCrLf & _
            "        ) THROW 52021,'No tienes permiso para configurar autorizaciones.',1;" & vbCrLf & _
            "        IF NOT EXISTS (SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments WHERE department_id=@department_id)" & vbCrLf & _
            "            THROW 52022,'Departamento inexistente.',1;" & vbCrLf & _
            "        DECLARE @roles TABLE(role_id INT PRIMARY KEY);" & vbCrLf & _
            "        INSERT @roles(role_id)" & vbCrLf & _
            "        SELECT DISTINCT TRY_CONVERT(INT,LTRIM(RTRIM(value)))" & vbCrLf & _
            "        FROM STRING_SPLIT(@roles_csv,',')" & vbCrLf & _
            "        WHERE TRY_CONVERT(INT,LTRIM(RTRIM(value))) IS NOT NULL;" & vbCrLf & _
            "        IF EXISTS(SELECT 1 FROM @roles a WHERE NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Roles r WHERE r.role_id=a.role_id))" & vbCrLf & _
            "            THROW 52023,'Uno de los roles seleccionados no existe.',1;" & vbCrLf & _
            "        IF @is_required=1 AND NOT EXISTS(SELECT 1 FROM @roles)" & vbCrLf & _
            "            THROW 52024,'Un departamento habilitado requiere al menos un rol.',1;" & vbCrLf & _
            "        IF @is_required=0 AND NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments" & vbCrLf & _
            "            WHERE department_id<>@department_id AND is_required=1" & vbCrLf & _
            "        ) THROW 52025,'Debe mantenerse habilitado al menos un departamento.',1;" & vbCrLf & _
            "        UPDATE dbo.ssi_PaS_RejectionDepartments" & vbCrLf & _
            "        SET is_required=@is_required WHERE department_id=@department_id;" & vbCrLf & _
            "        DELETE dbo.ssi_PaS_RejectionDepartmentRoles WHERE department_id=@department_id;" & vbCrLf & _
            "        INSERT dbo.ssi_PaS_RejectionDepartmentRoles(department_id,role_id)" & vbCrLf & _
            "        SELECT @department_id,role_id FROM @roles;" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property StartScanRejection As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @id BIGINT;" & vbCrLf & _
            "    SET @event_token=COALESCE(@event_token,NEWID());" & vbCrLf & _
            "" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- El bloqueo del contenedor serializa altas concurrentes." & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "            WHERE container_id=@container_id AND status='OPEN'" & vbCrLf & _
            "        ) THROW 52101,'Solo se registran rechazos en pallets abiertos.',1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "            WHERE user_id=@actor_user_id AND is_active=1" & vbCrLf & _
            "        ) THROW 52102,'Se requiere un usuario activo.',1;" & vbCrLf & _
            "" & vbCrLf & _
            "        -- Si la misma estación reintenta un evento ya confirmado en CCS," & vbCrLf & _
            "        -- recuperar su rechazo original, sin generar una fila nueva." & vbCrLf & _
            "        SELECT @id=rejection_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ScanRejectionEvents WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        WHERE event_token=@event_token;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @id IS NULL" & vbCrLf & _
            "        BEGIN" & vbCrLf & _
            "            SELECT @id=rejection_id" & vbCrLf & _
            "            FROM dbo.ssi_PaS_ScanRejections WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "            WHERE container_id=@container_id AND status='BLOCKED';" & vbCrLf & _
            "" & vbCrLf & _
            "            IF @id IS NULL" & vbCrLf & _
            "            BEGIN" & vbCrLf & _
            "                INSERT dbo.ssi_PaS_ScanRejections" & vbCrLf & _
            "                    (container_id,rejection_type,ssi_scan,client_scan," & vbCrLf & _
            "                     rejection_reason,detected_by_user_id)" & vbCrLf & _
            "                VALUES" & vbCrLf & _
            "                    (@container_id,@rejection_type,COALESCE(@ssi_scan,N'')," & vbCrLf & _
            "                     @client_scan,LEFT(COALESCE(@reason,N''),1000),@actor_user_id);" & vbCrLf & _
            "" & vbCrLf & _
            "                SET @id=SCOPE_IDENTITY();" & vbCrLf & _
            "" & vbCrLf & _
            "                INSERT dbo.ssi_PaS_RejectionRequirements(rejection_id,department_id)" & vbCrLf & _
            "                SELECT @id,department_id" & vbCrLf & _
            "                FROM dbo.ssi_PaS_RejectionDepartments WHERE is_required=1;" & vbCrLf & _
            "            END;" & vbCrLf & _
            "" & vbCrLf & _
            "            -- Registrar CADA incidencia detectada, aunque el contenedor" & vbCrLf & _
            "            -- ya estuviera bloqueado por un rechazo anterior." & vbCrLf & _
            "            INSERT dbo.ssi_PaS_ScanRejectionEvents" & vbCrLf & _
            "                (event_token,rejection_id,container_id,rejection_type," & vbCrLf & _
            "                 ssi_scan,client_scan,rejection_reason,detected_by_user_id)" & vbCrLf & _
            "            VALUES" & vbCrLf & _
            "                (@event_token,@id,@container_id,@rejection_type," & vbCrLf & _
            "                 COALESCE(@ssi_scan,N''),@client_scan," & vbCrLf & _
            "                 LEFT(COALESCE(@reason,N''),1000),@actor_user_id);" & vbCrLf & _
            "        END;" & vbCrLf & _
            "" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "        SELECT @id AS rejection_id,@event_token AS event_token;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property BeginPalletAction As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON; SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @id BIGINT;" & vbCrLf & _
            "    DECLARE @note NVARCHAR(500)=LTRIM(RTRIM(COALESCE(@reason,N'')));" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "        IF @action_type NOT IN('CLOSE_PARTIAL','CANCEL_EMPTY')" & vbCrLf & _
            "            THROW 52201,'Tipo de operación no permitido.',1;" & vbCrLf & _
            "        IF LEN(@note)<10 THROW 52202,'Motivo obligatorio: mínimo 10 caracteres.',1;" & vbCrLf & _
            "        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "                      WHERE user_id=@packing_user_id AND is_active=1)" & vbCrLf & _
            "            THROW 52203,'El operador no tiene una sesión activa.',1;" & vbCrLf & _
            "        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Containers WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "                      WHERE container_id=@container_id AND status='OPEN')" & vbCrLf & _
            "            THROW 52204,'El pallet debe estar ABIERTO.',1;" & vbCrLf & _
            "        IF EXISTS(SELECT 1 FROM dbo.ssi_PaS_ScanRejections" & vbCrLf & _
            "                  WHERE container_id=@container_id AND status='BLOCKED')" & vbCrLf & _
            "            THROW 52205,'Resuelve primero el rechazo de pieza pendiente.',1;" & vbCrLf & _
            "        SELECT @id=request_id FROM dbo.ssi_PaS_PalletActionRequests WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        WHERE container_id=@container_id AND status='PENDING';" & vbCrLf & _
            "        IF @id IS NOT NULL" & vbCrLf & _
            "        BEGIN" & vbCrLf & _
            "            IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionRequests" & vbCrLf & _
            "                WHERE request_id=@id AND action_type=@action_type" & vbCrLf & _
            "                  AND requested_by_user_id=@packing_user_id AND reason=@note)" & vbCrLf & _
            "                THROW 52206,'Ya existe una solicitud diferente pendiente en este pallet.',1;" & vbCrLf & _
            "        END" & vbCrLf & _
            "        ELSE" & vbCrLf & _
            "        BEGIN" & vbCrLf & _
            "            IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments WHERE is_required=1)" & vbCrLf & _
            "                THROW 52207,'Habilita al menos un departamento autorizador en Setup.',1;" & vbCrLf & _
            "            IF EXISTS(" & vbCrLf & _
            "                SELECT 1 FROM dbo.ssi_PaS_RejectionDepartments d" & vbCrLf & _
            "                WHERE d.is_required=1 AND NOT EXISTS(" & vbCrLf & _
            "                    SELECT 1 FROM dbo.ssi_PaS_RejectionDepartmentRoles m" & vbCrLf & _
            "                    WHERE m.department_id=d.department_id" & vbCrLf & _
            "                )" & vbCrLf & _
            "            ) THROW 52208,'Hay un departamento sin roles autorizados.',1;" & vbCrLf & _
            "            INSERT dbo.ssi_PaS_PalletActionRequests" & vbCrLf & _
            "                (container_id,action_type,requested_by_user_id,reason)" & vbCrLf & _
            "            VALUES(@container_id,@action_type,@packing_user_id,@note);" & vbCrLf & _
            "            SET @id=SCOPE_IDENTITY();" & vbCrLf & _
            "            INSERT dbo.ssi_PaS_PalletActionRequirements(request_id,department_id)" & vbCrLf & _
            "            SELECT @id,department_id FROM dbo.ssi_PaS_RejectionDepartments WHERE is_required=1;" & vbCrLf & _
            "        END;" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "        SELECT @id AS request_id;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property ApprovePalletAction As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON; SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @user_id INT,@role_id INT,@operator INT;" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "        SELECT @operator=requested_by_user_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_PalletActionRequests WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        WHERE request_id=@request_id AND status='PENDING';" & vbCrLf & _
            "        IF @operator IS NULL THROW 52211,'La solicitud ya no está pendiente.',1;" & vbCrLf & _
            "        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionRequirements" & vbCrLf & _
            "                      WHERE request_id=@request_id AND department_id=@department_id)" & vbCrLf & _
            "            THROW 52212,'Este departamento no requiere aprobación.',1;" & vbCrLf & _
            "        SELECT @user_id=user_id,@role_id=role_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "        WHERE barcode_id=UPPER(LTRIM(RTRIM(@barcode))) AND is_active=1" & vbCrLf & _
            "          AND password_hash=HASHBYTES('SHA2_256',CONVERT(VARCHAR(50),@password));" & vbCrLf & _
            "        IF @user_id IS NULL THROW 52213,'Credenciales incorrectas o usuario inactivo.',1;" & vbCrLf & _
            "        IF @user_id=@operator THROW 52214,'El operador solicitante no puede aprobar su propio pallet.',1;" & vbCrLf & _
            "        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_RejectionDepartmentRoles" & vbCrLf & _
            "                      WHERE department_id=@department_id AND role_id=@role_id)" & vbCrLf & _
            "            THROW 52215,'El rol no está autorizado para ese departamento.',1;" & vbCrLf & _
            "        IF EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionApprovals" & vbCrLf & _
            "                  WHERE request_id=@request_id AND authorized_by_user_id=@user_id)" & vbCrLf & _
            "            THROW 52216,'Un usuario solo puede aprobar un departamento.',1;" & vbCrLf & _
            "        INSERT dbo.ssi_PaS_PalletActionApprovals" & vbCrLf & _
            "            (request_id,department_id,authorized_by_user_id)" & vbCrLf & _
            "        VALUES(@request_id,@department_id,@user_id);" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property CompletePalletAction As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON; SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @container INT,@action VARCHAR(20),@note NVARCHAR(500);" & vbCrLf & _
            "    DECLARE @actual INT,@required INT,@authorizer INT;" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "        SELECT @container=container_id,@action=action_type,@note=reason" & vbCrLf & _
            "        FROM dbo.ssi_PaS_PalletActionRequests WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        WHERE request_id=@request_id AND status='PENDING'" & vbCrLf & _
            "          AND requested_by_user_id=@packing_user_id;" & vbCrLf & _
            "        IF @container IS NULL THROW 52221,'Solicitud inexistente, completada o de otro usuario.',1;" & vbCrLf & _
            "        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Users" & vbCrLf & _
            "                      WHERE user_id=@packing_user_id AND is_active=1)" & vbCrLf & _
            "            THROW 52222,'Operador inactivo.',1;" & vbCrLf & _
            "        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_Containers WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "                      WHERE container_id=@container AND status='OPEN')" & vbCrLf & _
            "            THROW 52223,'Pallet cerrado o modificado.',1;" & vbCrLf & _
            "        IF EXISTS(SELECT 1 FROM dbo.ssi_PaS_ScanRejections" & vbCrLf & _
            "                  WHERE container_id=@container AND status='BLOCKED')" & vbCrLf & _
            "            THROW 52224,'Hay una pieza rechazada pendiente de aprobación.',1;" & vbCrLf & _
            "        IF NOT EXISTS(SELECT 1 FROM dbo.ssi_PaS_PalletActionRequirements" & vbCrLf & _
            "                      WHERE request_id=@request_id)" & vbCrLf & _
            "            THROW 52225,'Solicitud sin departamentos requeridos.',1;" & vbCrLf & _
            "        IF EXISTS(" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_PalletActionRequirements req" & vbCrLf & _
            "            WHERE req.request_id=@request_id AND NOT EXISTS(" & vbCrLf & _
            "                SELECT 1 FROM dbo.ssi_PaS_PalletActionApprovals a" & vbCrLf & _
            "                WHERE a.request_id=req.request_id AND a.department_id=req.department_id" & vbCrLf & _
            "            )" & vbCrLf & _
            "        ) THROW 52226,'Faltan aprobaciones de departamentos.',1;" & vbCrLf & _
            "        -- El primer autorizador representa el registro legado; todas las aprobaciones" & vbCrLf & _
            "        -- quedan guardadas individualmente en ssi_PaS_PalletActionApprovals." & vbCrLf & _
            "        SELECT TOP(1) @authorizer=authorized_by_user_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_PalletActionApprovals" & vbCrLf & _
            "        WHERE request_id=@request_id ORDER BY department_id;" & vbCrLf & _
            "        IF @authorizer IS NULL THROW 52227,'No hay aprobadores.',1;" & vbCrLf & _
            "        SELECT @required=SUM(required_pieces)" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerLevels WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        WHERE container_id=@container;" & vbCrLf & _
            "        SELECT @actual=COUNT(*)" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerPieces p WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        JOIN dbo.ssi_PaS_ContainerLevels l WITH(UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "            ON l.container_level_id=p.container_level_id" & vbCrLf & _
            "        WHERE l.container_id=@container;" & vbCrLf & _
            "        IF @action='CLOSE_PARTIAL'" & vbCrLf & _
            "        BEGIN" & vbCrLf & _
            "            IF @actual<=0 OR @actual>=@required OR @required IS NULL" & vbCrLf & _
            "                THROW 52228,'El pallet parcial debe tener piezas y estar incompleto.',1;" & vbCrLf & _
            "            UPDATE dbo.ssi_PaS_Containers" & vbCrLf & _
            "            SET status='PARTIAL',closed_at=SYSUTCDATETIME(),closed_by_user_id=@authorizer" & vbCrLf & _
            "            WHERE container_id=@container AND status='OPEN';" & vbCrLf & _
            "            INSERT dbo.ssi_PaS_PartialClosures" & vbCrLf & _
            "                (container_id,authorized_by_user_id,packing_user_id," & vbCrLf & _
            "                 actual_pieces,required_pieces,reason)" & vbCrLf & _
            "            VALUES (@container,@authorizer,@packing_user_id,@actual,@required,@note);" & vbCrLf & _
            "            INSERT dbo.ssi_PaS_PackingAudit" & vbCrLf & _
            "                (event_type,container_id,actor_user_id,reason)" & vbCrLf & _
            "            VALUES ('CLOSE_PARTIAL',@container,@authorizer,@note);" & vbCrLf & _
            "        END" & vbCrLf & _
            "        ELSE IF @action='CANCEL_EMPTY'" & vbCrLf & _
            "        BEGIN" & vbCrLf & _
            "            IF @actual<>0 OR EXISTS(SELECT 1 FROM dbo.ssi_PaS_ContainerLevels" & vbCrLf & _
            "                                    WHERE container_id=@container AND status='CLOSED')" & vbCrLf & _
            "                THROW 52229,'Solo se cancela un pallet vacío, sin niveles cerrados.',1;" & vbCrLf & _
            "            UPDATE dbo.ssi_PaS_Containers SET status='CANCELLED'" & vbCrLf & _
            "            WHERE container_id=@container AND status='OPEN';" & vbCrLf & _
            "            INSERT dbo.ssi_PaS_EmptyPalletCancellations" & vbCrLf & _
            "                (container_id,packing_user_id,authorized_by_user_id,reason)" & vbCrLf & _
            "            VALUES (@container,@packing_user_id,@authorizer,@note);" & vbCrLf & _
            "            INSERT dbo.ssi_PaS_PackingAudit" & vbCrLf & _
            "                (event_type,container_id,actor_user_id,reason)" & vbCrLf & _
            "            VALUES ('CANCEL_CONTAINER',@container,@authorizer,@note);" & vbCrLf & _
            "        END" & vbCrLf & _
            "        ELSE THROW 52230,'Tipo de operación no reconocido.',1;" & vbCrLf & _
            "        UPDATE dbo.ssi_PaS_PalletActionRequests" & vbCrLf & _
            "        SET status='COMPLETED',completed_at=SYSUTCDATETIME()" & vbCrLf & _
            "        WHERE request_id=@request_id;" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "        SELECT @container AS container_id,@action AS action_type,'COMPLETED' AS status;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH" & vbCrLf & _
            "END;"
        End Get
    End Property

    Public Shared ReadOnly Property RemovePiece As String
        Get
            Return             "BEGIN" & vbCrLf & _
            "    SET NOCOUNT ON;" & vbCrLf & _
            "    SET XACT_ABORT ON;" & vbCrLf & _
            "    DECLARE @piece_id BIGINT, @level_id INT, @inspection_id INT;" & vbCrLf & _
            "    DECLARE @note NVARCHAR(500)=LTRIM(RTRIM(ISNULL(@reason,N'')));" & vbCrLf & _
            "    BEGIN TRY" & vbCrLf & _
            "        BEGIN TRANSACTION;" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Users u" & vbCrLf & _
            "            JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id" & vbCrLf & _
            "            WHERE u.user_id=@actor_user_id AND u.is_active=1 AND r.can_modify_setup=1" & vbCrLf & _
            "        ) THROW 51401,'An active authorized Setup user is required to remove a piece.',1;" & vbCrLf & _
            "        IF LEN(@note)<10 THROW 51402,'Correction reason must be at least 10 characters.',1;" & vbCrLf & _
            "" & vbCrLf & _
            "        IF NOT EXISTS (" & vbCrLf & _
            "            SELECT 1 FROM dbo.ssi_PaS_Containers WITH (UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "            WHERE container_id=@container_id AND status='OPEN'" & vbCrLf & _
            "        ) THROW 51403,'Only OPEN containers can be corrected.',1;" & vbCrLf & _
            "" & vbCrLf & _
            "        SELECT @piece_id=p.container_piece_id," & vbCrLf & _
            "               @level_id=p.container_level_id," & vbCrLf & _
            "               @inspection_id=p.verified_inspection_id" & vbCrLf & _
            "        FROM dbo.ssi_PaS_ContainerPieces p WITH (UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "        JOIN dbo.ssi_PaS_ContainerLevels l WITH (UPDLOCK,HOLDLOCK)" & vbCrLf & _
            "            ON l.container_level_id=p.container_level_id" & vbCrLf & _
            "        WHERE l.container_id=@container_id AND p.product_id=@product_id" & vbCrLf & _
            "          AND l.status='OPEN';" & vbCrLf & _
            "" & vbCrLf & _
            "        IF @piece_id IS NULL THROW 51404,'Piece not found in an OPEN level of this pallet.',1;" & vbCrLf & _
            "" & vbCrLf & _
            "        INSERT dbo.ssi_PaS_PackingAudit" & vbCrLf & _
            "            (event_type,container_id,container_piece_id,container_level_id," & vbCrLf & _
            "             product_id,inspection_id,actor_user_id,reason)" & vbCrLf & _
            "        VALUES ('REMOVE_PIECE',@container_id,@piece_id,@level_id," & vbCrLf & _
            "                @product_id,@inspection_id,@actor_user_id,@note);" & vbCrLf & _
            "" & vbCrLf & _
            "        DELETE dbo.ssi_PaS_ContainerPieces WHERE container_piece_id=@piece_id;" & vbCrLf & _
            "        IF @@ROWCOUNT<>1 THROW 51405,'Piece correction failed; no changes were saved.',1;" & vbCrLf & _
            "        COMMIT TRANSACTION;" & vbCrLf & _
            "        SELECT @container_id AS container_id,@product_id AS removed_product_id;" & vbCrLf & _
            "    END TRY" & vbCrLf & _
            "    BEGIN CATCH" & vbCrLf & _
            "        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;" & vbCrLf & _
            "        THROW;" & vbCrLf & _
            "    END CATCH;" & vbCrLf & _
            "END;"
        End Get
    End Property

End Class
