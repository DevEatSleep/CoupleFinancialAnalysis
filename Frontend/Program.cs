using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for API calls → backend
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});

// HttpClient for local static files (locale JSON)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddScoped<ApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new ApiClient(factory.CreateClient("Api"));
});
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<ChatState>();
builder.Services.AddScoped<BotService>();
builder.Services.AddScoped<PollingService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new PollingService(
        new ApiClient(factory.CreateClient("Api")),
        sp.GetRequiredService<ChatState>());
});

var host = builder.Build();

// Initialize localization (uses default HttpClient → wwwroot)
var loc = host.Services.GetRequiredService<LocalizationService>();
await loc.InitializeAsync();

await host.RunAsync();
