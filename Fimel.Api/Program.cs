using System.Text.Json.Serialization;
using Fimel.Api.Services.Flow;
using Fimel.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
