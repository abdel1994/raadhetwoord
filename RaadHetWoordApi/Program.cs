using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Voeg controllers en Swagger toe
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RaadHetWoord API", Version = "v1" });
});

// CORS (voor Blazor WebAssembly toegang)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Kestrel configureren voor Docker
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Zorgt ervoor dat de API luistert op alle netwerken
});

var app = builder.Build();

// Gebruik Swagger en API-documentatie
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaadHetWoord API v1"));
}

app.UseCors("AllowAll"); // CORS inschakelen
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.Run();
