# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solución y proyectos

**FIMEL_NET** es un sistema de gestión médica (.NET 8) compuesto por cuatro proyectos:

| Proyecto | Tipo | Puerto local | Rol |
|---|---|---|---|
| `Fimel.Api` | Web API | 5091 (HTTP) | Backend REST, acceso a BD, lógica de dominio |
| `Fimel.Site` | MVC | 5037 (HTTP) | Frontend, consume Fimel.Api via HTTP |
| `Fimel.Models` | Classlib | — | Entidades EF Core, DbContext, migraciones |
| `Fimel.Utils` | Classlib | — | APIClient, Utileria (email, PDF, sesión, cifrado) |

## Comandos

```bash
# Compilar toda la solución
dotnet build Fimel.sln

# Ejecutar (abrir dos terminales)
dotnet run --project Fimel.Api
dotnet run --project Fimel.Site

# Nueva migración (apunta a BD de testing por defecto)
dotnet ef migrations add <NombreMigracion> --project Fimel.Models --startup-project Fimel.Api

# Aplicar migración a testing
dotnet ef database update --project Fimel.Models --startup-project Fimel.Api \
  --connection "Database=DB_A6CE9B_FimelDev;Server=SQL5102.site4now.net;User=DB_A6CE9B_FimelDev_admin;Password=fimeldev123;Integrated Security=;Encrypt=false;TrustServerCertificate=true"

# Aplicar migración a producción
dotnet ef database update --project Fimel.Models --startup-project Fimel.Api \
  --connection "Database=DB_A6CE9B_Fimel;Server=SQL5101.site4now.net;User=DB_A6CE9B_Fimel_admin;Password=admin123;Integrated Security=;Encrypt=false;TrustServerCertificate=true"
```

No hay suite de tests automatizados en el repositorio; existen guías de pruebas manuales en `Manual_Pruebas_FIMEL.md` y `Checklist_Pruebas_FIMEL.md` en la raíz.

## Arquitectura y flujo de datos

### Site → API
`Fimel.Site` nunca accede directamente a la BD. Toda operación pasa por `Fimel.Utils.APIClient`, que encapsula `HttpClient` con `Newtonsoft.Json`:

```csharp
// En cualquier controller del Site:
private readonly APIClient APIBase = new APIClient(config["API_URL"]);

var resultado = APIBase.Get<List<Pacientes>>("Pacientes/GetByCriteria", query);
var nuevo     = APIBase.Post<Pacientes>("Pacientes", objeto);
APIBase.Put<Pacientes>($"Pacientes/{id}", objeto);
APIBase.Delete<bool>($"Pacientes/{id}");
```

`API_URL` se resuelve por entorno vía `ASPNETCORE_ENVIRONMENT` (mecanismo estándar de ASP.NET Core: `appsettings.{Environment}.json` sobrescribe a `appsettings.json`). `Development` se fija en `Properties/launchSettings.json`; `Testing`/`Production` se fijan en los publish profiles (`Properties/PublishProfiles/*.pubxml`).

**Excepción a tener en cuenta**: `Fimel.Utils.Utileria` arma su propio `APIClient` estático leyendo *solo* el `appsettings.json` base (`new ConfigurationBuilder().AddJsonFile("appsettings.json")`, sin overlay de entorno), a diferencia de los controllers del Site que reciben `IConfiguration` inyectado (ese sí resuelve por entorno). Esto afecta las llamadas a la API que hace `Utileria` (p.ej. `EnviarCorreo` registrando en `BitacoraMensajerias`): siempre usan el `API_URL` del `appsettings.json` base, independientemente del entorno desplegado.

### Serialización JSON — punto crítico
- **Fimel.Api** usa `System.Text.Json` con policy **camelCase** (predeterminado).
- **APIClient** usa **Newtonsoft.Json** que deserializa de forma case-insensitive a propiedades PascalCase.
- **Fimel.Site** re-serializa con `System.Text.Json` sin policy → **PascalCase**.
- **Consecuencia**: en los controllers del Site usar siempre tipos fuertemente tipados (`Get<List<MiClase>>`), nunca `List<object>` o `List<Dictionary<...>>`, porque Newtonsoft devuelve `JObject` con claves camelCase que el JS del frontend no puede leer con las referencias PascalCase habituales.

### Autenticación y sesión
- Login compara contraseñas en texto plano contra la tabla `Usuarios`.
- El usuario conectado se serializa completo en `HttpContext.Session` bajo la clave `"UsuarioConectado"` (timeout 4 horas).
- La sesión no es in-memory: `Fimel.Site/Program.cs` la respalda con `AddDistributedSqlServerCache` contra la tabla `SessionCache` de la BD `Fimel` (misma connection string que usa `Fimel.Api`), así que persiste entre reinicios del proceso.
- Para recuperar la sesión en cualquier controller del Site:
  ```csharp
  Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
  ```

### Entidades heredadas
`LayerSuperType` es la clase base de la mayoría de modelos: tiene `Id`, `Vigente` (soft-delete) y `FechaCreacion`.

Algunas tablas están **excluidas de las migraciones** porque existen en la BD desde antes del ORM: `Usuarios`, `Reservas`, `Perfiles`, `parExamenes`, `parEspecialidades`, `parTiposConsultas`, `Bitacoras`, `Documentos`, `BitacoraMensajerias`, `Config`, `HorariosAtencion`. Sus entidades existen en el modelo pero tienen `ExcludeFromMigrations()` en `FimelDbContext.OnModelCreating`.

### Servicios en segundo plano
`Fimel.Site/Program.cs` registra cuatro `IHostedService` (todos corren dentro del proceso del Site, no del Api, y calculan su propia demora hasta la próxima hora de ejecución con `Task.Delay`):
- `CumpleanosBackgroundService` — diario a las 08:00, envía correos de cumpleaños.
- `ProximoControlBackgroundService` — notificaciones de próximos controles.
- `RecordatorioBackgroundService` — diario a las 08:00, envío de recordatorios manuales.
- `RecordatorioCitaBackgroundService` — diario a las 08:00, recordatorios de citas.

### Email
`Utileria.EnviarCorreo(EnvioCorreo correo, List<(string Path, string ContentId, string Mime)>? imagenes, string? displayName)` envía vía SMTP `mail.fimel.cl:8889`. Las plantillas HTML están en `Fimel.Site/wwwroot/mails/`. Los logos de institución se almacenan como Base64 en la columna `Instituciones.Logo`.

### PDF
iText7 se usa para generar PDFs desde HTML en `Utileria`. La impresión de recetas desde el navegador usa `window.open` + `window.print()` + `window.onafterprint → window.close()` (sin servidor).

## Convenciones del frontend

- **Bootstrap 5** + **jQuery** + **SweetAlert2** en todas las vistas.
- Cada vista tiene un módulo JS nombrado `Modulo<Nombre>` (IIFE), p.ej. `ModuloConsulta`, `ModuloFichaPaciente`.
- Las URLs de acciones se pasan a JS mediante `<input type="hidden" id="hdnURL_*" value='@Url.Action(...)'>` en la vista.
- Datos del servidor accesibles en JS (doctor, institución, etc.) también se inyectan como hidden inputs desde el ViewBag o el modelo.
- Archivos JS del site en `Fimel.Site/wwwroot/js/Site/`.

## Localización
`Fimel.Site/Program.cs` fija la cultura del thread y del request a `es-CL` (única cultura soportada, sin negociación por navegador) para evitar traducción automática y estandarizar formatos de fecha/número en las vistas.

## Entorno y configuración

Cada proyecto Web (`Fimel.Api`, `Fimel.Site`) tiene `appsettings.Development.json`, `appsettings.Testing.json` y `appsettings.Production.json` con su propio `API_URL` / `URL_SITIO` / connection string; ASP.NET Core selecciona el archivo según `ASPNETCORE_ENVIRONMENT` (ver sección anterior). Las credenciales de BD de testing y producción también están guardadas en la memoria del proyecto (`~/.claude/projects/.../memory/reference_bases_de_datos.md`).
