using CoupleChat.Data;
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

var app = builder.Build();

// Middleware
app.UseRouting();
app.UseCors("AllowFrontend");

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
}

app.Run("http://localhost:5000");
