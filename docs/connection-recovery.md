# CCS connection recovery — deployment and testing

This replaces the removed multi-pallet offline mode. PackAndShip **does not scan or create pallets offline**. Q-Wall remains the online authority.

## Connection monitor

- An authenticated workstation checks CCS every 30 seconds while connected.
- After a connection failure, attempts back off at 5, 10, 20, 40, 80, 160, then 300 seconds (maximum 5 minutes). One check at a time.
- The bottom status bar shows CONNECTED or DISCONNECTED and the next retry interval.
- On the transition from disconnected to connected, the operator receives a modal notification.
- No internet ping is used. The relevant dependency is the **CCS SQL Server** connection.

## Single pending write

Immediately before one of these database actions, a write-ahead intent is durably saved:
- `ADD_PIECE` (both QRs, level, container, actor)
- `CLOSE_LEVEL`
- `CLOSE_PALLET`

Location: `%LOCALAPPDATA%\SSI\PackAndShip\pending-operation.json`.

If SQL confirms success, the intent is deleted. If connection is lost or the acknowledgement is ambiguous, the intent remains and further packing writes are disabled. On reconnect, the app checks the current SQL pallet first. If the exact piece/closure already exists, it clears the intent without duplicating it; otherwise it retries the original operation through the same transactional stored procedures. Q-Wall PASS, flags, Scrap, duplicate prevention and capacity are **always** checked server-side. An explicit SQL business rejection (e.g. duplicate, FAIL, closed pallet) is marked for review and **never** blindly retried.

The queue stores only **one** uncertain operation. There is no offline multi-pallet queue. The local file belongs to the Windows profile. Never delete it while pending. The app requires a successful online login after a full restart; pending actions survive restart.

## Acceptance tests

1. Confirm normal connected piece registration, level closure and pallet closure.
2. Disconnect from CCS before recording the next validated piece, click Record piece once; status must show pending operation. Other packing writes must be blocked.
3. Restore CCS, wait for the next scheduled probe, confirm reconnect alert, piece count and no duplicates.
4. Simulate a server-side commit followed by lost acknowledgement. Reconnect: it should recognize the same piece already in that level and **not** add a second piece.
5. Repeat using Close level and Complete pallet.
6. With Q-Wall FAIL or duplicate, verify SQL rejection is marked REVIEW and cannot silently bypass validation.
7. Restart the app after a failed write, log in when SQL returns, and verify pending operation recovery.
8. Keep SQL unreachable and watch the retry interval increase up to 300 seconds, never polling repeatedly within one interval.

## Transition from former offline version

The offline scanning menu and source files were removed. This release does not read or upload the earlier `offline-queue.json`. If you used that feature, **do not delete its old file**; reconcile any existing queued pallets with CCS before upgrading. Previously deployed `ssi_PaS_OfflineImports` and `ssi_PaS_OfflineOpen` objects are left untouched in CCS to avoid destructive changes; they are not used by this version.

This implementation was edited in GitHub but not compiled in Windows or tested against your live CCS.
