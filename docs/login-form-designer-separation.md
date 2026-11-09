# Independent LoginForm (Visual Studio Designer)

## Files
- `PackAndShip/LoginForm.vb` — login event handling, SQL authentication via `PackAndShipData.Authenticate`, session state.
- `PackAndShip/LoginForm.Designer.vb` — all visible login components, positioned directly on the form.
- `PackAndShip/Form1.vb` — startup shell / scanner. Shows LoginForm on first display; closing LoginForm without valid login exits the app. Logout clears the session and reopens LoginForm.
- `PackAndShip/Form1.Designer.vb` — no duplicate login controls.

## Edit visually (without fixed layout)
Open `LoginForm.vb` -> View Designer (Shift+F7). The form is resizable (`FormBorderStyle=Sizable`, `MinimumSize=0,0`, `MaximumSize=0,0`). No visual login control has `Dock=Fill`, no `TableLayoutPanel`, and no design-time repositioning in `LoginForm.vb`. All controls are direct children of the form with default `Dock=None` and `Anchor=Top,Left`, so select/move/resize with mouse. Change `ClientSize` to resize the window; change each control's `Location` and `Size` using handles or Properties.

Controls: `titleLabel`, `sectionLogin`, `barcodeLabel`, `inputBarcode`, `passwordLabel`, `inputPassword`, `sectionStation`, `customerCaption`, `labelCustomer`, `buttonLogin`.

The configured customer comes from `StationSettings` via `Form1.RefreshCustomer()` and is shown on the login form. SQL authentication and role permissions have not changed. LoginForm is independent from the Scanner's visual layout; users edit Form1/Scanner separately.

## Acceptance checks
1. Rebuild Solution and open LoginForm View Designer; drag form edge and child controls.
2. Run app; LoginForm shows without duplicate login controls in Form1.
3. Invalid badge or password: stays on login; valid credentials open scanner, retaining station customer and role.
4. Log out: login reopens; cancel login: app closes.
5. Test an active pallet restored at station after successful login.

No SQL migration. Windows Visual Studio designer and database integration tests remain to be run locally.
