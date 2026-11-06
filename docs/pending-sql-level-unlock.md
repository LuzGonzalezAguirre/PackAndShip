# Stuck Close Level after restoring a locked pallet

## Symptom
A resumed pallet shows a full level (example: 2/3 pieces total, Level 2: 1/1), but the scanner's buttons remain disabled.

## Cause addressed
The scanner deliberately blocks all packing writes while `%LOCALAPPDATA%\SSI\PackAndShip\pending-operation.json` exists. This may happen when SQL committed a scan but acknowledgement was lost, or a previously rejected SQL operation was marked REVIEW. Before this patch, the scanner did not make the pending action visible or offer a safe in-app resolution. The Verify button could also remain disabled after the pending state had cleared.

## Change
- A **Review pending SQL** button is displayed beside Open / Load when the local pending journal exists.
- Clicking the button queries the real CCS container state. If the exact piece or closure is already committed, the journal is cleared and controls are re-enabled.
- A SQL-rejected attempt, not present in CCS, can be explicitly acknowledged and discarded **only after a successful CCS verification**. This does not delete or change any server records; if the piece is still needed, scan and verify it again.
- An ambiguous action that could still have been written cannot be discarded merely because the operator wants to proceed.
- The scanner shows the type/container of the pending operation and why it is blocked.
- Verify QR availability is recalculated from current part/model/configuration instead of retaining an old disabled state.

## Safe recovery for P456456
1. Pull `master` and rebuild in Visual Studio.
2. Sign into the same Windows account/profile as when the scan occurred.
3. Load the locked pallet P456456 (automatic recovery). If a pending action exists, select **Review pending SQL**.
4. If the pending action was already committed, it is reconciled automatically. If it was explicitly rejected by SQL, review the reason and acknowledge the rejected attempt.
5. Confirm Level 2 is 1/1, then use **Close level**. Continue the third level.
6. Verify that `ssi_PaS_ContainerPieces` still has its original piece count, that the pending file is only cleared after verification, and no duplicate was inserted.

Never delete the pending-operation.json file blindly or update the pallet-level status directly in SQL.

**No CCS data is modified by this software change. Windows compilation and physical validation remain necessary.**
