# PackAndShip — Security, audit and history rollout

## Deployment order in CCS

1. Back up CCS or test in a restored staging database first.
2. Ensure sql/013_authorized_quality_flags.sql is already applied (confirmed workflow).
3. Execute **sql/014_security_audit_and_corrections.sql**. Creates ssi_PaS_PackingAudit and audit triggers; adds ssi_PaS_RemovePiece and ssi_PaS_CancelContainer. Does not edit existing pallets.
4. Execute **sql/015_runtime_permissions.sql** after reviewing database permissions. This creates a role but **does NOT attach users**. To enforce DENY for the app, first provision an app-specific NON-db_owner database principal, verify all required SELECT and setup rights, then manually ADD MEMBER to ssi_PaS_RuntimeRole. Never use a sysadmin/db_owner account for runtime.
5. Pull master, rebuild in Visual Studio .NET 10, then open **Pallet History** in the main menu.

## Rules

- Packing is permitted only with latest finished Q-Wall PASS, no Scrap, and authorized latest inspection flags. The existing SQL unique constraint prevents reusing product_id.
- Every newly opened pallet, registered piece, closed level and closed pallet generates audit events via SQL triggers.
- Preexisting records are still visible through their original created/scanned/closed metadata; audit event triggers are NOT retroactive.
- Remove Piece: active PackAndShip user with `ssi_PaS_Roles.can_modify_setup=1`, container OPEN, level OPEN, required reason >=10 characters. Deletion occurs in one transaction with immutable audit snapshot of product/inspection/actor.
- Cancel Pallet: authorized user, OPEN pallet, no closed levels, zero packed pieces, required reason >=10 characters; code remains unavailable forever. Remove any current pieces individually before canceling.
- CLOSED pallets and CLOSED levels are not editable. No reopen procedure is provided.
- History allows browsing all statuses, detail levels, verified Q-Wall inspection IDs, actions and CSV exports.
- Packing report counts **currently packed pieces** grouped by UTC scan day, model, operator. Removed pieces remain in the audit, not in the current-count report.
- The role protection is **opt-in** because the app's existing database login and Setup permissions are deployment-specific. Application-layer visibility is not a substitute for database permissions.
- No automated Windows compilation or CCS execution was performed from GitHub.

## Minimum verification

1. Normal operator can view history and export; cannot see Remove/Cancel buttons.
2. Setup-authorized operator removes a product from OPEN level with a reason: piece disappears; REMOVE_PIECE audit persists.
3. Same product can be recorded again with a valid Q-Wall PASS (the SQL unique index is no longer occupied).
4. Reject remove on CLOSED level, CLOSED pallet, wrong pallet, and reasons below 10 chars.
5. Cancel empty OPEN pallet with a reason; audit shows CANCEL_CONTAINER; same pallet code cannot be recreated.
6. Reject cancellation with packed pieces or closed levels.
7. Confirm auditing of new OPEN, ADD, CLOSE_LEVEL and CLOSE_CONTAINER actions.
8. Export pallet list, detail pieces, audit and grouped packing report, and verify UTC date meaning.
9. After manually activating runtime role, test `EXECUTE AS USER` in a test transaction: direct UPDATE/DELETE/INSERT on packing tables are denied, while stored procedures and SELECTs remain usable. Test all Setup screens before deployment.
