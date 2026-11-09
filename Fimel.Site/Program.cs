using System.Text.Json.Serialization;
using System.Globalization;
using Fimel.Site.Services;

var builder = WebApplication.CreateBuilder(args);

// Secretos del servidor (llave X-Api-Key, llaves de Flow, etc.) en un archivo fuera de git que se sube una vez
// al servidor. Web Deploy no lo borra (SkipExtraFilesOnServer) y no se publica desde el equipo local (ver csproj).
// Formato en appsettings.Secrets.example.json.
builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: false);

// Llave compartida con Fimel.Api (header X-Api-Key). Desde user-secrets / variable de entorno
// ApiSeguridad__ApiKey o FIMEL_API_KEY; nunca en appsettings versionado. Se asigna antes de crear cualquier APIClient.
Fimel.Utils.APIClient.ApiKeyCompartida = builder.Configuration["ApiSeguridad:ApiKey"] ?? builder.Configuration["FIMEL_API_KEY"];

// WhatsApp Cloud API (Meta): sección "WhatsApp" de la configuración; el AccessToken va en appsettings.Secrets.json
Fimel.Utils.WhatsAppCloud.Configurar(builder.Configuration);

// Add services to the container.

// Configurar globalización para español de Chile
var cultureInfo = new CultureInfo("es-CL");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.AddControllersWithViews(o => o.Filters.Add<Fimel.Site.Filters.SuscripcionActivaFilter>())
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.PropertyNamingPolicy = null;
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHostedService<CumpleanosBackgroundService>();
builder.Services.AddHostedService<ProximoControlBackgroundService>();
builder.Services.AddHostedService<RecordatorioBackgroundService>();
builder.Services.AddHostedService<RecordatorioCitaBackgroundService>();
builder.Services.AddHostedService<SuscripcionesBackgroundService>();

builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Fimel");
    options.SchemaName = "dbo";
    options.TableName = "SessionCache";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler("/Error/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Configurar localización
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("es-CL"),
    SupportedCultures = new List<CultureInfo> { new CultureInfo("es-CL") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("es-CL") }
});

app.UseSession();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
