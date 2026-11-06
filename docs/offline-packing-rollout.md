# Offline Packing / outbox — deployment and acceptance tests

## Purpose

Keep packing when the workstation cannot reach **CCS/Q-Wall** (including LAN, VPN, server outage). Operators may open and complete multiple offline pallets. Every scan is committed to a local on-disk queue before the UI reports success. The SQL sync runs every 30 seconds while signed in, and can be started from **Offline Packing > Sync now**.

**Important:** An offline piece is only **FORMAT CHECKED / PENDING Q-WALL VERIFICATION**, not a PASS. A pallet completed offline is not SQL-confirmed or released for shipping until its status becomes **SYNCED**. Offline and server duplicate prevention are separate checks.

## Setup (must be done while connected)

1. Run `sql/016_offline_import_identity.sql` in CCS. This creates `ssi_PaS_OfflineImports` and `ssi_PaS_OfflineOpen`. The stored procedure uses one transaction to bind a local pallet GUID to a CCS pallet ID and permits safe retries after a lost response. If `ssi_PaS_RuntimeRole` exists, it receives EXECUTE/SELECT permissions.
2. Pull the latest `master`, rebuild with Visual Studio .NET 10.
3. Sign in normally while CCS is reachable; the scanner caches the pallet ID prefix/length.
4. Select each model that might be used offline in the main scanner at least once. This caches active pallet configuration, label scan rule, and field mappings locally.
5. Open/load the current pallet while online (if it already exists). The scanner caches its latest SQL snapshot after every successful refresh.
6. Keep PackAndShip signed in. The offline screen is available from the top menu.

## Offline procedure

1. When the connection is unavailable, use **Offline Packing**.
2. For a pallet already started online, select it from the dropdown; its SQL-confirmed pieces and closed levels are prepopulated as `UPLOADED`.
3. For a new pallet, select a **cached model**, enter a new pallet ID and choose **Create offline pallet**. Reuse of a local pallet code is rejected.
4. Scan SSI/Client labels. One-scan models generate the Client code using the cached model. A piece cannot be repeated anywhere in the local queue. The app enforces configured quantities and level closing.
5. Use **Close level locally** for each full level, and **Complete pallet locally** after all levels close.
6. Repeat for additional pallets. They all stay on disk.
7. When CCS is reachable again, completed pallets replay one by one. The server rechecks identity, PASS, Scrap, flag authorization, capacity and global duplicates using existing SQL procedures.
8. Only status **SYNCED** confirms that the entire pallet was stored and closed in CCS. A **REVIEW** result retains the local queue and error; fix the CCS/configuration/inspection issue and retry.

## Durability and limitations

- Local queue: `%LOCALAPPDATA%\SSI\PackAndShip\offline-queue.json` (one Windows profile / workstation). Do **not** delete this file until all pallets are SYNCED and backed up.
- Writes use a durable temporary file and same-volume atomic replace; no accepted scan is intentionally left only in memory.
- A completed pallet may contain an invalid Q-Wall inspection or a product already packed online. That pallet stays REVIEW for human correction; **never automatically bypass SQL**.
- If a pallet code already exists in CCS and is not associated with the same local ID, synchronization rejects it rather than merging pallets.
- Cached configuration is validated against the actual SQL pallet level layout before piece replay; drift needs review.
- SQL `scanned_at` records the **upload time** for offline scans, not their physical scan time; the original scan timestamp is retained in the local queue.
- Current offline UI requires an **already authenticated session**. After restarting PackAndShip during a full CCS outage, offline work is not yet accessible until online login succeeds; the saved queue remains intact.
- This queue is workstation-scoped. Do not expect offline progress to follow a user to a different PC.
- Setup/user permissions still require SQL connectivity; offline work only records scans/pallet progress.
- No offline shipment authorization is provided.
- Only use this with reliable local disk storage and backups. The queue is not a substitute for SQL backups.

## Validation matrix

1. Connected: select and load an OPEN pallet with a few pieces. Disconnect CCS. Open Offline Packing: existing pieces and levels appear.
2. Disconnect, register 2 new pieces on that pallet, complete its levels. Create 5 more offline pallets and complete them.
3. Stop/restart only the **Offline Packing window**, not the signed-in application; verify all pieces and progress persist.
4. Check duplicate local SSI/client serial is rejected, and a level cannot close before exact target quantity.
5. Reconnect. Watch automatic sync (30-second interval) and confirm each completed pallet becomes SYNCED.
6. Query CCS to confirm the expected total levels/pieces and that each `product_id` occurs only once globally.
7. Simulate failure after SQL pallet creation but before client save. Retry: local GUID must resolve the same CCS pallet and never create a new container.
8. Simulate failure after piece insertion: retry must detect same SSI+Client in the same level and avoid a second insert.
9. Test Q-Wall FAIL, Scrap and already-packed serials offline. They must be accepted as *provisional only* and blocked at server upload with REVIEW.
10. Test change of configured level quantities between caching and reconnection: must be REVIEW, no mismatched piece upload.

**Not yet verified in this environment:** Windows compile, CCS procedure deployment, and physical network interruption tests.
