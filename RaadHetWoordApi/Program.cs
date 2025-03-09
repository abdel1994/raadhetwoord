
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.OpenApi.Models;
using RaadHetWoordApi.Data;
using RaadHetWoordApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Voeg controllers en Swagger toe
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RaadHetWoord API", Version = "v1" });
});

// connectionstring ophalen uit appsettings.json, dit is de connectie met de database, in dit geval postgresql
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

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

// haal de woorden op uit een bestand en importeer ze in de database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ImportWoorden(context, "woorden.txt");
}


// Gebruik Swagger en API-documentatie
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaadHetWoord API v1"));
}

// Importeer woorden in de database waarbij elk woord op een nieuwe regel staat
void ImportWoorden(AppDbContext context, string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Bestand {filePath} niet gevonden.");
        return;
    }

    var woorden = File.ReadAllLines(filePath)
                      .Where(w => !string.IsNullOrWhiteSpace(w))
                      .Select(w => new Woord { Tekst = w.Trim() })
                      .ToList();

    if (woorden.Count > 0)
    {
        context.Woorden.AddRange(woorden);
        context.SaveChanges();
        Console.WriteLine($"{woorden.Count} woorden geïmporteerd!");
    }
    else
    {
        Console.WriteLine("Geen woorden om te importeren.");
    }
}


app.UseCors("AllowAll"); // CORS inschakelen
app.UseRouting();    // Routing inschakelen , routing is het proces van het bepalen van de beste manier om een pakket van de bron naar de bestemming te sturen
app.UseAuthorization();  // Autorisatie inschakelen, autorisatie is het proces van het bepalen of een gebruiker toegang heeft tot een bepaalde bron of actie
app.MapControllers();  // Controllers inschakelen, controllers zijn klassen die verantwoordelijk zijn voor het verwerken van inkomende verzoeken en het retourneren van antwoorden aan de client
app.Run();
