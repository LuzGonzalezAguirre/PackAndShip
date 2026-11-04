# PackAndShip — etapa 1: Login

Aplicación de escritorio VB.NET Windows Forms (.NET 10), con apariencia inicial basada en Q-Wall.

## Requisitos

- Windows y .NET 10 SDK / runtime de escritorio.
- Acceso a la base de datos SQL Server CCS con permisos de lectura sobre `ssi_BusinessUnits`, `ssi_PaS_Users` y `ssi_PaS_Roles`.
- Tablas `ssi_PaS_Users` y `ssi_PaS_Roles` ya creadas.

## Configuración de conexión

No se almacena ninguna contraseña de SQL en el repositorio. Configurar la variable de entorno
`PACKANDSHIP_CONNECTION_STRING` en la computadora de Pack and Ship. Para autenticación
integrada de Windows, un ejemplo (ajustar servidor y nombre de base de datos) es:

```powershell
[Environment]::SetEnvironmentVariable(
    "PACKANDSHIP_CONNECTION_STRING",
    "Server=SERVIDOR;Database=CCS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;",
    "User"
)
```

Reiniciar Visual Studio después de configurar la variable.

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
