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
