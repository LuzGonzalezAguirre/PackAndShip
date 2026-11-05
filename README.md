# PackAndShip — etapa 1: Login

Aplicación de escritorio VB.NET Windows Forms (.NET 10), con apariencia inicial basada en Q-Wall.

## Requisitos

- Windows y .NET 10 SDK / runtime de escritorio.
- Acceso a la base de datos SQL Server CCS con permisos de lectura sobre `ssi_BusinessUnits`, `ssi_PaS_Users` y `ssi_PaS_Roles`.
- Tablas `ssi_PaS_Users` y `ssi_PaS_Roles` ya creadas.

## Configuración de conexión

La cadena SQL Server se define **directamente en el método `OpenConnection()` de `PackAndShip/PackAndShipData.vb`**, siguiendo la preferencia del proyecto Q-Wall.

Sustituir los marcadores `SERVIDOR_SQL` y `BASE_DE_DATOS` en Visual Studio. El ejemplo usa autenticación integrada de Windows. **No subir claves o contraseñas SQL reales al repositorio**, que actualmente es público.

## Flujo implementado

1. Carga el cliente guardado en `%ProgramData%\PackAndShip\station_config.txt`.
2. Solicita Barcode ID y Password, y los valida contra `ssi_PaS_Users`.
3. Consulta `can_modify_setup` en `ssi_PaS_Roles`.
4. Solo los roles autorizados pueden cambiar el cliente local a través de Setup.
5. La pantalla siguiente es por ahora un marcador sin funciones de escaneo.

**Primera configuración:** dado que el cliente se cambia únicamente después de iniciar sesión,
debe existir al menos un usuario autorizado en `ssi_PaS_Users` para completar el alta inicial.

## Notas de seguridad y pendientes

- El login utiliza temporalmente el esquema SHA2-256 heredado de Q-Wall para coincidir
  con `password_hash VARBINARY(32)`. No es adecuado para contraseñas nuevas en producción;
  antes del despliegue se necesita migrar a un KDF con sal (PBKDF2, Argon2id o similar).
- El archivo de configuración de la estación debe tener permisos de Windows adecuados.
  Los permisos de interfaz no sustituyen los controles del sistema operativo.
- Todavía faltan CRUD de usuarios, configuración de pallets por nivel, editor compartido de
  reglas de escaneo, ajustes generales y reglas de Pallet Scanner.
- Los cambios aún no se han compilado ni probado contra SQL Server real.

## Setup — Users & Roles (primera versión)

El menú **Setup > Users & Roles** abre un formulario Windows Forms con DataGridView,
búsqueda, alta, edición, activación/desactivación y restablecimiento de contraseña
al Barcode ID. Los roles se cargan del catálogo `ssi_PaS_Roles` y los registros
de `ssi_PaS_Users`. Los cambios verifican nuevamente en SQL que el usuario
administrador sigue activo y tiene `can_modify_setup=1`.

La baja es lógica (desactivar), no un borrado físico. Cambiar el Barcode ID
de un empleado **no** restablece automáticamente su contraseña: existe un botón
específico para hacerlo. No se puede modificar la propia cuenta mientras está
iniciada la sesión.

**Pendiente antes de producción:** el esquema temporal SHA2-256 sin sal y las
contraseñas iguales al Barcode ID son inseguros; migrar a credenciales individuales
y un KDF con sal. Esta pantalla aún requiere compilación y pruebas de integración
con la base SQL Server de la planta.

## Setup — Pallet Configuration (primera versión)

Requiere ejecutar `sql/001_pallet_configuration.sql` antes de abrir el formulario.

`Setup > Pallet Configuration` permite seleccionar modelos desde
`ssi_PartNumbers`, filtrarlos por cliente (`ssi_BusinessUnits`) y buscar
por `ssiPN`. Cada `pn_id` tiene como máximo una configuración;
el filtro de cliente **no** crea configuraciones adicionales.

Cada nivel tiene su cantidad obligatoria de piezas (entero positivo).
El total se calcula automáticamente como suma de niveles y se guarda
junto con el detalle, en transacción serializable. Si el modelo ya estaba
configurado, reemplaza sus niveles durante el mismo guardado.
Incluye bandera activa/inactiva y actualización de `updated_at`.

Se consulta nuevamente el rol activo en SQL para autorizar escrituras.
No modifica los catálogos de Q-Wall. El editor aún requiere pruebas
de compilación en Visual Studio y validación funcional contra SQL Server.

Por ahora no contempla historial/versiones de configuración ni reglas
de escaneo del pallet; se definirán con las operaciones de la estación.

## Scan Rules — CRUD compartido (primera versión)

`Setup > Scan Rules` incluye **Add rule**, **Edit selected** y
**Activate / Deactivate**. El editor permite seleccionar `pn_id` del
catálogo, configurar `scan_count`, `requires_match`, `is_active`,
notas y agregar/quitar campos con las columnas existentes de
`quality_scan_fields`. En la edición se cargan todos los campos reales;
se mantienen sin cambios si el usuario no los edita.

Las operaciones usan transacciones SQL y verifican `can_modify_setup`
contra la base. La edición contrasta `updated_at` para detectar cambios
concurrentes. No se ofrece eliminación física de reglas. Las reglas
nuevas se muestran **inactivas** por defecto para evitar efectos
inmediatos en Q-Wall.

**Precauciones:** se utilizan tablas de Q-Wall; un cambio a una regla
activa afecta ambas aplicaciones. Antes de modificar la regla real,
probar altas y edición con una regla inactiva de laboratorio y respaldar
`quality_pn_scan_rules` y `quality_scan_fields`. Los campos de auditoría
`created_by_id`/`updated_by_id` no reciben identificadores de
usuarios PackAndShip porque pertenecen a una identidad distinta.
El esquema actual no tiene auditoría PaS propia para estas operaciones.

Las opciones de `extraction_mode` y otros valores siguen siendo texto
editable para no introducir supuestos no verificados sobre el parser de
Q-Wall. **No hay previsualización ni simulación del resultado de escaneo**:
se necesita confirmar con casos reales antes del uso productivo.
Los cambios requieren compilación y pruebas de integración en Visual
Studio y SQL Server.

## Scan Fields — editor visual por escaneo

El editor de reglas ahora muestra tarjetas por **Scan 1, Scan 2, ...**, de acuerdo con
`scan_count`. Cada tarjeta de campo incluye label, destino, modo de extracción,
separador, posición, prefijo, longitud esperada, índice de segmento, orden y preview.
Se pueden agregar/quitar campos independientemente en cada escaneo. Los valores
legibles se convierten a los existentes en SQL (`por_separador`, `espacio`,
`despues`, `frameSN`, `volvoSerialNumber`). Se conservan como texto editable los
valores de extracción desconocidos para no reemplazar reglas históricas.

El script `sql/003_seed_standard_scan_rules.sql` prepara reglas **inactivas** para
modelos estándar Volvo y John Deere que carezcan de reglas; tiene `@Apply = 0`
como **dry run** por defecto. Revisar cada candidato antes de establecer
`@Apply = 1`. No inserta excepciones de un solo escaneo, auto-generación,
separadores opcionales ni clientes sin representación comprobada en el parser.

El editor y el script no sustituyen una prueba del escáner real. Las reglas
compartidas pueden cambiar el comportamiento de Q-Wall.

## Setup > General: formato de identificador de pallet

El formulario `General` permite ajustar el prefijo (1–5 letras A-Z) y la
cantidad de dígitos (1–12). El valor predeterminado es `P` + 6 dígitos
(ejemplo `P000001`). Incluye una vista previa y una prueba de formato
sin registrar pallets. El valor se almacena por estación en
`%ProgramData%\PackAndShip\pallet_id_format.txt`, independiente del
archivo `station_config.txt` que guarda el cliente predeterminado.
No requiere migración SQL ni modifica ninguna tabla compartida.

**Pendiente:** integrar `PalletIdSettings.IsValid()` al futuro flujo
Pallet Scanner, definir los criterios de unicidad y la generación/asignación
de identificadores reales. Cambiar el formato no renumera pallets existentes.
Para guardar en `ProgramData`, el proceso debe tener permiso de escritura
en la carpeta local.

## General Settings en SQL Server (global)

Ejecutar primero `sql/005_general_settings.sql` en la misma base CCS a la
que apunta PackAndShip. Crea una única fila en
`dbo.ssi_PaS_GeneralSettings` (settings_id=1), con
`pallet_prefix='P'` y `pallet_digits=6`. Es idempotente: no sobrescribe
una configuración ya guardada. Las columnas `updated_at` (UTC) y
`updated_by_user_id` registran quién modificó la configuración.

`Setup > General` lee y actualiza esta fila en SQL. Guardar requiere
autorización `can_modify_setup`, revalidada en SQL. El archivo local
`%ProgramData%\PackAndShip\pallet_id_format.txt` **ya no se usa**;
el cliente predeterminado sigue siendo por estación en
`station_config.txt`. Todas las estaciones consultan la misma
configuración al cargarla; deben volver a leerla antes de validar un
escaneo para evitar usar valores antiguos. Todavía no existe esa
integración en Pallet Scanner.

La primera instalación crea el formato `P` + 6 dígitos en SQL, y
no importa automáticamente archivos locales previos. Se debe respaldar
el valor local si alguna estación había personalizado el formato.

## UI refresh — WinForms design foundation

First visual refactor introduces `PaSTheme.vb` (reusable colors, typography,
button factory) and `SetupDashboardForm.vb` (Setup navigation cards).
The main `Setup` button opens the dashboard instead of the legacy
dropdown. Existing Setup forms, database access, and authorization logic
are unchanged. The old menu wiring is retained in Form1 for the time being
to minimize change risk; it can be removed once the dashboard is verified.

Next visual iteration: apply the shared theme to all dialogs and tables,
then refine login and scanner layouts without changing business rules.
Build and smoke-test from Visual Studio on Windows (no Windows build
environment was available for this change).

## Navegación superior (MenuStrip)

La ventana principal utiliza un `MenuStrip` fijo sobre el header:
**Setup** (Customer Configuration, Users & Roles, Pallet Configuration,
Scan Rules y General Settings), **Ayuda** (About) y **Cerrar sesión**.
Setup y Cerrar sesión permanecen deshabilitados antes del login; Setup
respeta `can_modify_setup`. El botón anterior y el SetupDashboard dejan
de utilizarse en la navegación principal.

El modal **Customer Configuration** fue normalizado con encabezado azul,
fondo claro y botones creados mediante `PaSTheme`; mantiene la
persistencia local del cliente predeterminado. El resto de los modales
todavía requiere unificación visual. Validar compilación y navegación en
Visual Studio.

## Pallet Scanner — primer layout y controles de seguridad

Se agregó `PalletScannerView.vb` en la pantalla autenticada. Muestra:
contenedor, modelo SSI/Client filtrado por Customer, nombre grande del
operador, fecha/hora local, cantidad de piezas y distribución de niveles
del modelo seleccionado, y dos entradas de QR con avance Enter de Scan 1
a Scan 2. Relee `ssi_PaS_GeneralSettings` para comprobar el identificador
de contenedor antes de verificar el formato de los QR.

`ScanFormatValidator.vb` interpreta únicamente reglas activas con
dos escaneos y modo `por_separador` / `despues`, prefijo conocido,
separador configurado y longitud numérica exacta; compara los números
extraídos cuando `requires_match = 1`. Los modos distintos se bloquean
explícitamente, no se interpretan aproximadamente.

**Importante: sólo validación de formato. No acepta ni persiste piezas.**
Se requiere leer PASS desde Q-Wall (según inspecciones y flags), definir
las identidades de piezas, la restricción UNIQUE global para que no pueda
existir una pieza en dos cajas, y un guardado transaccional con cierre de
nivel/contendor únicamente con cantidades exactas. Hasta entonces la
interfaz muestra "PACKING LOCKED" y no existen botones funcionales de
cierre. No debe usarse para registrar producción. Para revisar el esquema
Q-Wall se agregó `sql/006_qwall_pass_schema_review.sql` (solo lectura).

En `Form1.vb` se ajustó también la ubicación del dropdown de Customer
Configuration para separarlo del encabezado. Mantener el modelo
y el cliente de la estación como filtros separados del formato global
de contenedor.

## Layout operativo — Q-Wall + tarjetas (noviembre 2026)

La vista `PalletScannerView.vb` conserva un **header horizontal
compacto tipo Q-Wall** con cinco bloques: Container (entrada), Part Number
(selector), Operator (nombre grande), Customer y Date/Time.

Debajo: columna izquierda con **Active Build**, panel de **dos QR**
independiente y **Scanned Parts**; derecha con **Pallet Progress**
y **Validation Rules**. El diseño usa `PaSTheme`, paneles blancos y
tipografía Segoe UI. Se adapta al ancho del formulario; si es demasiado
estrecho habilita desplazamiento.

Es un cambio visual sobre el escáner preliminar. `Complete pallet`
sigue deshabilitado, el conteo indica 0 piezas y la lista muestra
que no hay piezas aceptadas. El botón Verify QR format **no registra**.
Se mantienen pendientes PASS Q-Wall, duplicados globales y cierres
por cantidades exactas. Requiere compilar y revisar distribución
en Windows con el tamaño de monitor real.

## Single-scan models and automatic Client serial preview

The scanner now follows `quality_pn_scan_rules.scan_count`. For an
active one-scan rule (for example, Volvo 43291.6), scan 1 extracts the
internal numeric serial using its configured extraction field; the Client
serial preview is generated from the selected model's
`ssi_PartNumbers.volvoProductNumber` plus a space and the same serial.
The second textbox is read-only in this mode. Press Enter on the first
scan or click Verify QR format. When the rule requires two scans, the
second QR remains required and the extracted numeric serial values
must match.

The generated Client serial is a **preview only**, not persisted to Q-Wall
or PackAndShip. A one-scan rule without a frameSN mapping, a model
without a Client part number, or an unsupported extraction mode is
rejected. Q-Wall PASS and global uniqueness validation must be integrated
before accepting or saving any piece.

## Q-Wall PASS: integración de consulta (read-only)

El escáner consulta ahora `QWallPassValidator.Verify` **después de
aprobar el formato del QR**. Busca exactamente una fila de `ssi_Products`
con `pn_id`, `frameSN` (SSI completo) y `fpcaSN` (Client completo),
como lo hace `GetOrCreateProductID()` de Q-Wall, pero **sin INSERT**.
Exige una última inspección finalizada en `ssi_Inspections`, con
`overall_result=1` y `is_scrap` no marcado. Si hay un registro en
`ssi_PieceFlagRecords` para esa inspección, devuelve "Quality review"
conservador hasta acordar los criterios de liberación.

La UI cambia el estado `Q-Wall PASS` a VERIFIED o NOT VERIFIED, y lo
restablece al cambiar un QR/contenedor. Aún no hay operación que
incremente piezas o habilite Complete pallet; falta diseñar el control
de duplicados y transacciones en `ssi_PaS_*`.

Para revisar qué guarda Q-Wall para una pieza real, ejecutar
`sql/007_qwall_pass_diagnostic.sql` (solo SELECT) cambiando el SSI y
Client completos. Se debe validar en CCS con inspecciones reales,
incluyendo FAIL, Scrap, flags y varios tipos de inspección, antes de
considerar terminada la integración.

## Stored procedures for operational packaging

After installing `sql/008_packaging_tables.sql`, execute
`sql/009_packaging_procedures.sql` in CCS. Four transactional procedures
are available:

- `ssi_PaS_OpenContainer`: checks active user, current global pallet
  format, active part configuration and level totals; snapshots all levels.
- `ssi_PaS_AddPiece`: holds container/level locks; checks Q-Wall product
  match, most recent finalized PASS inspection, no scrap/flag, remaining
  level capacity and the unique global product constraint; records the
  piece and verified inspection.
- `ssi_PaS_CloseLevel`: only allows closure at the exact required count.
- `ssi_PaS_CloseContainer`: requires all levels CLOSED and exact counts
  per level and across the container.

**The WinForms scanner is not yet wired to these procedures and remains
locked for production writes.** Procedures must be tested in a staging
environment with representative Q-Wall records and parallel workstations.
Direct table writes should be restricted to prevent bypassing the stored
procedure validations. Further policy review is needed for cases where
Q-Wall flags have been authorized or a new inspection is recorded after
a piece is packed.

## PackAndShip scanner: SQL-backed pilot (Nov 2026)

The four stored procedures from `sql/009_packaging_procedures.sql` are now
connected through `PaSPackingData.vb` to the WinForms scanner.

Workflow: select a model, enter the container code, click **Open / Load**,
scan the configured one or two QR labels, click **Verify QR format**, then
explicitly click **Record piece**. This runs `ssi_PaS_AddPiece` which
rechecks Q-Wall PASS and globally unique product assignment in a
transaction. The UI reloads the container, levels and recorded pieces
from SQL after every write. When a level has exactly its required quantity,
**Close level** becomes available; **Complete pallet** requires all levels
closed and exact quantities. Both call the corresponding SQL procedures.

The operators' QR input is never automatically committed on Enter:
verification and recording are separate intentional operations.
An existing container can be loaded again by code, but must match the
selected model. The same container code cannot be created twice.

**Pilot/testing only until validated in Visual Studio and against test
pallets in CCS.** Test one- and two-scan models, PASS/FAIL/Scrap/flags,
duplicates in the same and different containers, full levels, resuming
partially filled containers and concurrent stations. Database roles
should be configured to EXECUTE procedures without unrestricted direct
DML on `ssi_PaS_*`; the current stored procedures do not independently
re-parse QR label format because they expect WinForms validation first.
Do not grant arbitrary clients direct procedure execution without
considering server-side QR validation and identity binding.
