using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaadHetWoordApi.Data;
using RaadHetWoordApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ API moet luisteren op alle IP’s voor Docker-compatibiliteit
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// ✅ Configureer services
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// ✅ Configureer de middleware (volgorde is belangrijk!)
ConfigureMiddleware(app);

Console.WriteLine("🚀 API draait op http://localhost:5000");
app.Run();


// ==========================================
// 🔹 Configureer services
// ==========================================
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // ✅ Voeg controllers en Swagger toe
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "RaadHetWoord API", Version = "v1" });
    });

    // ✅ Databaseverbinding ophalen uit appsettings.json
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

    // ✅ CORS instellen voor Blazor WebAssembly
    services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorFrontend",
            builder => builder
                .WithOrigins("http://localhost:8080") // Blazor frontend URL
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()); // Alleen nodig bij cookies/authenticatie
    });
}

// ==========================================
// 🔹 Configureer middleware
// ==========================================
void ConfigureMiddleware(WebApplication app)
{
    // ✅ CORS moet vóór `UseRouting()` worden geplaatst
    app.UseCors("AllowBlazorFrontend");

    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();

    // ✅ Database controleren en woorden importeren
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("✅ Databaseverbinding succesvol!");
        ImportWoorden(context, "woorden.txt");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Fout bij databaseverbinding: {ex.Message}");
    }

    // ✅ Gebruik Swagger en API-documentatie
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaadHetWoord API v1"));
    }
}

// ==========================================
// 🔹 Woorden importeren in database
// ==========================================
void ImportWoorden(AppDbContext context, string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"❌ Bestand {filePath} niet gevonden.");
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
        Console.WriteLine($"✅ {woorden.Count} woorden geïmporteerd!");
    }
    else
    {
        Console.WriteLine("⚠️ Geen woorden om te importeren.");
    }
}
