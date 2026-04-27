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

No hay suite de tests automatizados en el repositorio.

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

`API_URL` en `Fimel.Site/appsettings.json` controla el entorno: está comentada/descomentada manualmente para local / testing / producción.

### Serialización JSON — punto crítico
- **Fimel.Api** usa `System.Text.Json` con policy **camelCase** (predeterminado).
- **APIClient** usa **Newtonsoft.Json** que deserializa de forma case-insensitive a propiedades PascalCase.
- **Fimel.Site** re-serializa con `System.Text.Json` sin policy → **PascalCase**.
- **Consecuencia**: en los controllers del Site usar siempre tipos fuertemente tipados (`Get<List<MiClase>>`), nunca `List<object>` o `List<Dictionary<...>>`, porque Newtonsoft devuelve `JObject` con claves camelCase que el JS del frontend no puede leer con las referencias PascalCase habituales.

### Autenticación y sesión
- Login compara contraseñas en texto plano contra la tabla `Usuarios`.
- El usuario conectado se serializa completo en `HttpContext.Session` bajo la clave `"UsuarioConectado"` (timeout 4 horas).
- Para recuperar la sesión en cualquier controller del Site:
  ```csharp
  Usuarios usuario = new Utileria().ObtenerSesion(HttpContext.Session.GetString("UsuarioConectado"));
  ```

### Entidades heredadas
`LayerSuperType` es la clase base de la mayoría de modelos: tiene `Id`, `Vigente` (soft-delete) y `FechaCreacion`.

Algunas tablas están **excluidas de las migraciones** porque existen en la BD desde antes del ORM (`Usuarios`, `Reservas`, `Perfiles`). Sus entidades existen en el modelo pero tienen `ExcludeFromMigrations()` en `OnModelCreating`.

### Servicios en segundo plano
`Fimel.Site/Program.cs` registra dos `IHostedService`:
- `CumpleanosBackgroundService` — diario a las 08:00, envía correos de cumpleaños.
- `ProximoControlBackgroundService` — notificaciones de próximos controles.

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

## Entorno y configuración

`Fimel.Site/appsettings.json` contiene las líneas de `API_URL` y `URL_SITIO` para los tres entornos (local / testing / prod) comentadas manualmente. Las credenciales de BD de testing y producción están guardadas en la memoria del proyecto (`~/.claude/projects/.../memory/reference_bases_de_datos.md`).
