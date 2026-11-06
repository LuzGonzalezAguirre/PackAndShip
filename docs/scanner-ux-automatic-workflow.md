# Scanner UX — Enter-first packing and automatic level confirmation

## No database migration required
Visual Basic WinForms changes only. Pull `master` and rebuild the solution.

## Scanner operation
1. Select a configured model. Scan the container ID; the barcode scanner's Enter opens/loads it automatically. **There is no separate open confirmation dialog**.
2. Scan SSI QR; Enter moves focus to the Client QR for two-label models. In one-label models, Enter generates the Client serial and immediately validates the piece.
3. Scan Client QR; Enter performs QR format and Q-Wall PASS validation and, **only after successful verification**, calls `ssi_PaS_AddPiece`. SQL still checks PASS, authorized flags, Scrap, duplicates and capacity. Invalid scans are never recorded.
4. After a successful piece recording that fills the current level, a large level confirmation dialog asks whether pieces are physically packed.
5. If **YES** on an intermediate level, the app calls the existing `ssi_PaS_CloseLevel`, refreshes CCS and focuses SSI for the next level.
6. If **YES** on the last level, it calls the existing level-close procedure, then the existing pallet-close procedure, and clears Container and QR fields.
7. If **NOT YET**, nothing is closed. The progress area shows **Review packed level** so the operator can confirm later; no further pieces can be recorded into a full level.
8. Partial pallet and empty pallet cancellation remain in the top ToolStrip and **still require the existing second-person authorization**.
9. A pending SQL operation remains visible through **Review pending SQL** in the ToolStrip; the scanner cannot bypass it.

## Layout
- Shorter header holds Container, Model, Operator, Customer, Date/Time and Active Build.
- Compact Scan Labels column on the left, Recent Scans below.
- Larger Pallet Progress column on the right, with numeric percent, color-coded level chips, full-level visual state, and progress bar.
- Validation Rules card removed; Q-Wall validation itself is unchanged.
- The previous Open / Load, Verify QR, Record piece, Close level and Complete pallet controls are no longer displayed. Their existing validation/write methods remain in use behind Enter and confirmations.

## Tests on a Windows workstation
- Scan container + Enter; opens without mouse click or extra popup.
- Two-label model: SSI + Enter focuses Client; Client + Enter validates against Q-Wall and saves exactly one piece.
- One-label model: SSI + Enter generates Client code, validates, records once.
- Invalid QR, Q-Wall FAIL/Scrap/unauthorized flag, and duplicate: no piece recorded, visible rejection.
- Intermediate level full: confirmation YES closes it and focuses next scan; NOT YET leaves it OPEN and offers a manual review confirmation.
- Last level full: confirmation YES closes level AND pallet, then empties Container input; NOT YET leaves pallet OPEN.
- Partial/cancel actions in top toolbar still require second-user credentials.
- Disconnect CCS while recording: pending operation is persisted and can be reconciled, with new scanning blocked until resolved.
- Reopen a locked pallet with a full level: **Review packed level** is accessible.

Windows build and live CCS tests were not executed in the GitHub editing environment.
