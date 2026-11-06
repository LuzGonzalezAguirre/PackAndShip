# Cancel Empty Pallet — second-person authorization

## Purpose

If an operator opened a pallet for a model without available Q-Wall PASS serial numbers, they can cancel it without packing a piece. This operation is **different** from Partial Pallet, which requires at least one accepted piece.

## Deployment

1. Run `sql/019_authorized_empty_pallet_cancellation.sql` against CCS after deploying `017_partial_pallet_and_lock.sql`.
2. Pull `master` and compile WinForms in Visual Studio.
3. In Pallet Progress, open a pallet without recording any pieces. The **Cancel empty pallet (authorization)** button is enabled.
4. An independent, active PackAndShip account with a non-Operator/non-Operador role enters Barcode ID, password and a reason of at least 10 characters.
5. After confirmation, CCS records status `CANCELLED`, authorizer, original packing user, reason and `CANCEL_CONTAINER` audit entry. The current workstation lock and Container/QR fields are cleared for the next pallet.
6. Pallet History uses the same second-user authorization for cancellation of an OPEN, empty pallet.

## Invariants

- CCS rejects cancellation if the pallet has one or more packed pieces, or any CLOSED level. Use **Partial pallet** for nonempty pallets.
- Original pallet code is retained by the container record and cannot be reused.
- Original container row, levels, configuration, users, and audit are not deleted.
- SQL verifies authorizer credentials and role again inside the stored procedure. It does not trust UI-only checks.
- Credentials are not persisted locally. On an ambiguous disconnection in the scanner flow, the pending operation records only its identity. Recovery checks for status `CANCELLED`. If still OPEN, fresh authorization is required.
- If already CANCELLED or another terminal status, SQL refuses another cancellation.

## Tests

1. Open a new empty pallet. Confirm partial button disabled and cancel button enabled.
2. Try operator credentials or same signed-in user: reject.
3. Try wrong password, inactive user, and a short reason: reject.
4. Enter another active nonoperator user and valid reason: status becomes CANCELLED and the scanner returns to the empty Container input.
5. Open a different pallet and confirm the workstation lock has been released.
6. Try cancellation on a pallet with one piece, any CLOSED level, or one already CANCELLED: reject.
7. Confirm `ssi_PaS_EmptyPalletCancellations` and `ssi_PaS_PackingAudit` contain the authorization and reason.
8. Simulate lost connection during confirmation; after reconnect, confirm no duplicate cancellation and no saved password.

No live CCS migration or Visual Studio compilation was executed through GitHub.
