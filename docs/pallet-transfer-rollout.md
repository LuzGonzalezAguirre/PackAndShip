# Pallet Transfer — close field cleanup and partial-to-new-pallet transfer

## Deployment

1. **Back up CCS** before schema updates; first test on a staging database if available.
2. Run `sql/018_pallet_transfers.sql` in CCS, after `017_partial_pallet_and_lock.sql`.
3. Run `git pull origin master`, rebuild PackAndShip in Visual Studio.
4. Sign in, open **Pallet Transfers** from the main menu.

## Pallet closure

- When a full pallet is CLOSED, or when it is closed PARTIAL with second-user authorization, PackAndShip unlocks the workstation and **clears the Container textbox, both QR fields, and current piece list**. The existing selected model is preserved so operators can immediately scan a new container code.
- The active pallet lock remains in force while SQL status is OPEN.
- On a reconnect that confirms an earlier closure committed, the scanner also clears the closed pallet.
- Restarting the application resumes any genuinely OPEN pallet; terminal statuses release its lock.

## Transfer

1. Partial closure of the original pallet is performed with the preexisting second-user authorization.
2. From **Pallet Transfers**, scan the old code (must be PARTIAL), review its current quantity, then scan a NEW, unused container code with the global prefix/length.
3. Confirm. SQL executes `ssi_PaS_TransferPartialPallet` as **one transaction**. It revalidates the latest Q-Wall PASS/flags/scrap for **every piece**, checks part and capacity, creates the destination with the source's original level configuration, moves existing `ssi_PaS_ContainerPieces` rows without changing product_id, original operator or scanned_at, closes fully populated destination levels, creates transfer detail records, and marks the source TRANSFERRED.
4. The new pallet remains OPEN with its partial quantity already registered and ready to scan the remaining pieces. The workstation loads/locks it automatically when its model matches the station customer.
5. SQL keeps the source's immutable partial-close snapshot and a per-piece transfer snapshot. History shows origin, destination, original pieces and audit. A TRANSFERRED source cannot be packed again.
6. A previously existing destination code is **rejected**. This version transfers **all**, not a chosen subset, of an authorized partial pallet. The destination must be a new code, not an existing pallet.

## Limitations and safety

- Requires working CCS and Q-Wall; **no offline transfers**.
- If any product no longer has a qualifying latest PASS, the entire transfer is rolled back.
- The transfer is a SQL transaction, but a dropped connection after commit can leave the client unsure whether it succeeded. **Before retrying**, look up source and destination in Pallet History; if source is TRANSFERRED and destination OPEN, load the destination directly. Never create another code to compensate for an ambiguous response.
- In CCS, the original physical source's current packing rows move to the destination; its historical piece list is reconstructed from `ssi_PaS_PalletTransferPieces`, including recorded original scan user/time and Q-Wall inspection.
- Transfers do not allocate or reuse product IDs, so the existing UNIQUE(product_id) guarantee remains intact.
- No live Windows build or CCS execution was possible from this environment.

## Acceptance tests

1. Complete a normal pallet and verify that Container, SSI QR, Client QR, and active list clear immediately.
2. Close another pallet PARTIAL with authorization; verify the same automatic clearing.
3. Open **Pallet Transfers**, scan the original partial code and a new unused code. Confirm correct model and initial quantity in the new pallet.
4. Verify source status TRANSFERRED and destination status OPEN in Pallet History, including the transfer history tab.
5. Continue packing on the new pallet until it reaches its configured level and total quantities; close it normally.
6. Verify Q-Wall inspection references and product IDs unchanged and no duplicate product IDs across container pieces.
7. Try source CLOSED, OPEN, CANCELLED, already TRANSFERRED, empty partial, and a destination ID already present in CCS; all must reject.
8. Change Q-Wall status of a test piece to FAIL before transfer; transfer must reject without creating a destination container.
9. Test SQL disconnection mid-transfer; reconcile the server state before retrying.
