# Active pallet lock and authorized partial closure

## Installation

1. Before upgrading, back up CCS and test the migration in staging if possible.
2. Execute `sql/017_partial_pallet_and_lock.sql` in CCS after the existing `014_security_audit_and_corrections.sql` deployment.
3. Pull the latest `master` and rebuild the .NET 10 WinForms project.
4. Test the new **Partial pallet (authorization)** button and the **PARTIAL** status in Pallet History.

## Rules

- Loading or creating an OPEN pallet locks the workstation to that pallet. The CONTAINER field and MODEL selection cannot be changed until the pallet is CLOSED or PARTIAL (or cancelled through the existing approved workflow).
- The active pallet code is saved under `%PROGRAMDATA%\PackAndShip\active_pallet.txt`, survives sign-out and restarts, and is restored automatically when the application opens online.
- Changing the workstation customer while a pallet is locked is prevented.
- Normal completion still requires all levels fully closed with exact quantities.
- Partial closure is permitted only for an OPEN pallet with at least one accepted piece and fewer than the configured total. A full pallet must use the normal closure.
- A **different active account with a non-Operator/non-Operador role** must provide barcode ID and password and at least 10 characters of reason. Authentication occurs on the form and again inside the CCS stored procedure. The operator's session is unaffected.
- Partial closure is terminal: container status = PARTIAL, closed_at and closed_by_user_id set, remaining incomplete levels retain their stored counts but can no longer accept pieces.
- `ssi_PaS_PartialClosures` records actual and required counts, original packing operator, authorized user, reason, timestamp. `ssi_PaS_PackingAudit` records `CLOSE_PARTIAL`.
- Existing pallets are untouched by installation. Existing Q-Wall checks and duplicate prevention continue unchanged.
- When network drops during partial closure, the local last-action journal persists only the operation identity, NOT credentials. On reconnect it checks whether SQL committed PARTIAL. Otherwise it clears the ambiguous intent and requires fresh authorization rather than replaying a credential.

## Acceptance tests

1. Open pallet Pxxxxx and scan 2+ PASS pieces; try to type a different pallet or select another model: blocked.
2. Log out, log in, confirm the same open pallet is restored and still locked.
3. Close complete pallet normally; confirm the fields unlock.
4. Open another pallet, scan fewer than required, click **Partial pallet (authorization)**.
5. Operator credentials or own account: rejected. Wrong supervisor password: rejected.
6. Correct non-operator second account + reason shorter than 10 characters: rejected.
7. Correct second-account credentials + valid reason: status becomes PARTIAL, stored actual vs required matches counts, audit has CLOSE_PARTIAL and reason; another pallet can open.
8. Try to record an additional piece to the partial pallet: SQL rejects because status no longer OPEN.
9. Test a pallet with zero pieces: Partial pallet disabled and SQL rejects direct execution.
10. Test network interruption after authorizer confirms: on reconnect the system identifies either PARTIAL already committed or requires reauthorization; it never saves a password locally.

**Windows compilation and CCS execution must be validated in the user's environment.**
