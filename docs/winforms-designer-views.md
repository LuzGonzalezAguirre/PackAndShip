# PackAndShip views and Visual Studio Designer

## Editable now

- `PalletScannerView.vb` + `PalletScannerView.Designer.vb` — **UserControl**. Main Scanner: container/model/operator/customer/time header; active build; partial and cancel ToolStrip; SSI/Client QR fields; recent scans; pallet progress and level chips.
- `PackingConfirmationForm.vb` + `PackingConfirmationForm.Designer.vb` — **Form**. Large level/final-pallet physical packing confirmation. Its constructor only supplies runtime values and accent colors.

In Solution Explorer, right-click `PalletScannerView.vb` or `PackingConfirmationForm.vb`, then **View Designer** (Shift+F7). Select a control and adjust Size, Location, Anchor, Dock, Font, etc. Save with Ctrl+S. Do not edit generated Designer code by hand unless the VS designer cannot load it. When changing a layout container's Dock, use the Document Outline panel to select nested controls.

## Other views currently constructed at runtime

- `Form1.vb` (main startup window: Login + main menu; contains Scanner after login)
- `PalletHistoryForm.vb` (Pallet History)
- `PalletTransferForm.vb` (Pallet Transfers)
- `PartialPalletAuthorizationForm.vb` (secondary user credential approval)
- `PalletConfigurationForm.vb` (pallet setup)
- `ScanRulesForm.vb` (model QR rules)
- `GeneralSettingsForm.vb` (container format setup)
- `UsersSetupForm.vb` (users and roles)

The remaining views still use code-created controls; they are **not** yet visually editable via their own Designer. Moving them requires additional form-by-form migration. In particular, the Login and navigation controls in Form1 still use `CreateLayout()`.

## Functional preservation

The scanner's existing SQL/Q-Wall validation, atomic piece record, pending action recovery and Enter-driven scan remain in `PalletScannerView.vb`; no database changes are required.

**Not yet checked**: compile and Visual Studio designer loading on Windows. If `View Designer` reports a parse error, retain the exact error and do not overwrite the generated control file.
