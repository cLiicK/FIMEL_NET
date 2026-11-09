using System.Text.Json.Serialization;
using Fimel.Api.Seguridad;
using Fimel.Api.Services.Flow;
using Fimel.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Secretos del servidor (llave X-Api-Key, llaves de Flow, etc.) en un archivo fuera de git que se sube una vez
// al servidor. Web Deploy no lo borra (SkipExtraFilesOnServer) y no se publica desde el equipo local (ver csproj).
// Formato en appsettings.Secrets.example.json.
builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: false);

// No anunciar Kestrel en el header Server
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Permite ingresar la llave X-Api-Key en Swagger (botón Authorize) cuando está configurada en local
    c.AddSecurityDefinition(ApiKeyMiddleware.Header, new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = ApiKeyMiddleware.Header,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = ApiKeyMiddleware.Header }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<FimelDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Fimel")));

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Flow: credenciales desde user-secrets / variables de entorno (Flow__ApiKey o FLOW_API_KEY), nunca en appsettings versionado
builder.Services.Configure<FlowOptions>(builder.Configuration.GetSection(FlowOptions.Seccion));
builder.Services.PostConfigure<FlowOptions>(o =>
{
    if (string.IsNullOrWhiteSpace(o.ApiKey)) o.ApiKey = builder.Configuration["FLOW_API_KEY"];
    if (string.IsNullOrWhiteSpace(o.SecretKey)) o.SecretKey = builder.Configuration["FLOW_SECRET_KEY"];
});
builder.Services.AddHttpClient<FlowClient>();

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Llave compartida Site → API (header X-Api-Key). Ver Seguridad/ApiKeyMiddleware.cs
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
