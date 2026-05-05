using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Services;
using Microsoft.EntityFrameworkCore;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs for Azure - detect if running on Azure App Service
// WEBSITE_INSTANCE_ID is only set on Azure App Service
var isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

if (isAzure)
{
    // On Azure, listen on all interfaces and the PORT environment variable (default 8080)
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
else
{
    // Local development
    builder.WebHost.UseUrls("http://localhost:5000");
}

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(Constants.Network.CorsPolicyName, policy =>
    {
        policy.WithOrigins(Constants.Network.AllowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite($"Data Source={Constants.Paths.DatabaseFile}"));

// Domestic work services
builder.Services.AddScoped<DomestiqueReferenceService>();

// Bot Services
var questionsPath = Path.Combine(Directory.GetCurrentDirectory(), Constants.Paths.QuestionsDirectory);
builder.Services.AddSingleton(new BotService(questionsPath));

// Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
app.UseRouting();
app.UseCors(Constants.Network.CorsPolicyName);

// Serve static files from wwwroot (Frontend)
app.UseStaticFiles();

// Swagger/OpenAPI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(Constants.Routes.SwaggerEndpoint, Constants.Routes.SwaggerTitle);
    options.RoutePrefix = Constants.Routes.Swagger;
});

app.MapControllers();

// Health check
app.MapGet(Constants.Routes.Health, () => Constants.Routes.HealthResponse);

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    // EnsureCreated will create all tables based on DbContext configuration, including DomestiqueResponses
    await db.Database.EnsureCreatedAsync();

    // Seed reference data if table is empty
    if (!await db.TravailDomestique.AnyAsync())
    {
        // INSEE reference data: (activity, age range, femme minutes, homme minutes)
        var donneesInsee = new (string, string, int, int)[]
        {
            // 18-24 ans
            (Constants.Domestique.Activities.CuisineEtMenage, Constants.Domestique.AgeRanges.Range18_24, 120, 70),
            (Constants.Domestique.Activities.SoinsEnfants, Constants.Domestique.AgeRanges.Range18_24, 50, 30),
            (Constants.Domestique.Activities.Courses, Constants.Domestique.AgeRanges.Range18_24, 25, 20),
            (Constants.Domestique.Activities.BricolagJardinage, Constants.Domestique.AgeRanges.Range18_24, 10, 20),
            // 25-34 ans
            (Constants.Domestique.Activities.CuisineEtMenage, Constants.Domestique.AgeRanges.Range25_34, 140, 85),
            (Constants.Domestique.Activities.SoinsEnfants, Constants.Domestique.AgeRanges.Range25_34, 95, 55),
            (Constants.Domestique.Activities.Courses, Constants.Domestique.AgeRanges.Range25_34, 32, 28),
            (Constants.Domestique.Activities.BricolagJardinage, Constants.Domestique.AgeRanges.Range25_34, 12, 35),
            // 35-49 ans
            (Constants.Domestique.Activities.CuisineEtMenage, Constants.Domestique.AgeRanges.Range35_49, 150, 90),
            (Constants.Domestique.Activities.SoinsEnfants, Constants.Domestique.AgeRanges.Range35_49, 105, 60),
            (Constants.Domestique.Activities.Courses, Constants.Domestique.AgeRanges.Range35_49, 34, 30),
            (Constants.Domestique.Activities.BricolagJardinage, Constants.Domestique.AgeRanges.Range35_49, 15, 40),
            // 50-64 ans
            (Constants.Domestique.Activities.CuisineEtMenage, Constants.Domestique.AgeRanges.Range50_64, 130, 80),
            (Constants.Domestique.Activities.SoinsEnfants, Constants.Domestique.AgeRanges.Range50_64, 30, 15),
            (Constants.Domestique.Activities.Courses, Constants.Domestique.AgeRanges.Range50_64, 28, 25),
            (Constants.Domestique.Activities.BricolagJardinage, Constants.Domestique.AgeRanges.Range50_64, 12, 35),
        };

        // Generate TravailDomestique records from the data (2 sexes per activity/age combo)
        foreach (var (activite, trancheAge, femmeMinutes, hommeMinutes) in donneesInsee)
        {
            foreach (var (sexe, dureeMinutes) in new[] { 
                (Constants.Domestique.Sexe.Femme, femmeMinutes), 
                (Constants.Domestique.Sexe.Homme, hommeMinutes) 
            })
            {
                var dureeHeures = Math.Round((decimal)dureeMinutes / 60m, 2);
                var coutJour = Math.Round(dureeHeures * Constants.Domestique.HourlyRate, 2);

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
        }

        await db.SaveChangesAsync();
    }
}

await app.RunAsync();

