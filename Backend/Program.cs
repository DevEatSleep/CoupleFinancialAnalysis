using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://127.0.0.1:5000", "http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

// Bot Services
var questionsPath = Path.Combine(Directory.GetCurrentDirectory(), "questions");
builder.Services.AddSingleton(new BotService(questionsPath));
builder.Services.AddSingleton(new NlpProcessor(Path.Combine(Directory.GetCurrentDirectory(), "questions.json")));

// Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
app.UseRouting();
app.UseCors("AllowFrontend");

// Swagger/OpenAPI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Couple Chat API v1");
    options.RoutePrefix = "swagger";
});

// Serve static files from Frontend folder
var frontendPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Frontend");
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath),
    RequestPath = ""
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath),
    RequestPath = ""
});

app.MapControllers();

// Health check
app.MapGet("/health", () => "OK");

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.EnsureCreated();

    // Ensure DomestiqueResponses table exists (safe for existing DBs)
    db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS DomestiqueResponses (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Person TEXT NOT NULL,
        Activite TEXT NOT NULL,
        HeuresParSemaine REAL NOT NULL,
        InseeRefFemme REAL NOT NULL,
        InseeRefHomme REAL NOT NULL,
        CreatedAt TEXT NOT NULL)");
    
    // Seed reference data if table is empty
    if (!db.TravailDomestique.Any())
    {
        const decimal hourlyRate = 15m;
        var donneesInsee = new (string, string, string, int)[]
        {
            ("femme", "cuisine & ménage", "18-24 ans", 120),
            ("homme", "cuisine & ménage", "18-24 ans", 70),
            ("femme", "soins enfants", "18-24 ans", 50),
            ("homme", "soins enfants", "18-24 ans", 30),
            ("femme", "courses", "18-24 ans", 25),
            ("homme", "courses", "18-24 ans", 20),
            ("femme", "bricolage/jardinage", "18-24 ans", 10),
            ("homme", "bricolage/jardinage", "18-24 ans", 20),
            ("femme", "cuisine & ménage", "25-34 ans", 140),
            ("homme", "cuisine & ménage", "25-34 ans", 85),
            ("femme", "soins enfants", "25-34 ans", 95),
            ("homme", "soins enfants", "25-34 ans", 55),
            ("femme", "courses", "25-34 ans", 32),
            ("homme", "courses", "25-34 ans", 28),
            ("femme", "bricolage/jardinage", "25-34 ans", 12),
            ("homme", "bricolage/jardinage", "25-34 ans", 35),
            ("femme", "cuisine & ménage", "35-49 ans", 150),
            ("homme", "cuisine & ménage", "35-49 ans", 90),
            ("femme", "soins enfants", "35-49 ans", 105),
            ("homme", "soins enfants", "35-49 ans", 60),
            ("femme", "courses", "35-49 ans", 34),
            ("homme", "courses", "35-49 ans", 30),
            ("femme", "bricolage/jardinage", "35-49 ans", 15),
            ("homme", "bricolage/jardinage", "35-49 ans", 40),
            ("femme", "cuisine & ménage", "50-64 ans", 130),
            ("homme", "cuisine & ménage", "50-64 ans", 80),
            ("femme", "soins enfants", "50-64 ans", 30),
            ("homme", "soins enfants", "50-64 ans", 15),
            ("femme", "courses", "50-64 ans", 28),
            ("homme", "courses", "50-64 ans", 25),
            ("femme", "bricolage/jardinage", "50-64 ans", 12),
            ("homme", "bricolage/jardinage", "50-64 ans", 35),
        };

        foreach (var (sexe, activite, trancheAge, dureeMinutes) in donneesInsee)
        {
            var dureeHeures = Math.Round(dureeMinutes / 60m, 2);
            var coutJour = Math.Round((dureeHeures * hourlyRate), 2);

            db.TravailDomestique.Add(new TravailDomestique
            {
                Sexe = sexe,
                Activite = activite,
                TrancheAge = trancheAge,
                DureeMinutes = dureeMinutes,
                DureeHeures = dureeHeures,
                CoutJour = coutJour
            });
        }
        
        db.SaveChanges();
    }
}

app.Run("http://localhost:5000");
