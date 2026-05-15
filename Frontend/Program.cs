using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Frontend.Services;
using Shared;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for API calls → backend
// In development, backend runs on port 5000 while frontend runs on 5091.
// In production (Railway/etc.), the backend serves the frontend from the same origin,
// so we use HostEnvironment.BaseAddress directly.
// NOTE: Environment.GetEnvironmentVariable does not work in Blazor WASM (browser context),
// so we use builder.HostEnvironment.IsDevelopment() instead.
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = builder.HostEnvironment.IsDevelopment()
        ? new Uri("http://localhost:5000/")
        : new Uri(builder.HostEnvironment.BaseAddress);
});

// HttpClient for local static files (locale JSON)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register authentication and API services
builder.Services.AddScoped<AuthService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new AuthService(factory.CreateClient("Api"));
});

builder.Services.AddScoped<ApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var authService = sp.GetRequiredService<AuthService>();
    return new ApiClient(factory.CreateClient("Api"), authService);
});

builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<ChatState>();
builder.Services.AddScoped<BotService>();
builder.Services.AddScoped<PollingService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var authService = sp.GetRequiredService<AuthService>();
    return new PollingService(
        new ApiClient(factory.CreateClient("Api"), authService),
        sp.GetRequiredService<ChatState>());
});

var host = builder.Build();

// Initialize localization (uses default HttpClient → wwwroot)
var loc = host.Services.GetRequiredService<LocalizationService>();
await loc.InitializeAsync();

// Initialize BotService to load INSEE reference data from backend
var botService = host.Services.GetRequiredService<BotService>();
await botService.InitializeAsync();

await host.RunAsync();
