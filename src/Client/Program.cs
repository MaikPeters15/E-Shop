var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Create a shared in-memory log store and register it
var logStore = new InMemoryLogStore(capacity: 2000);
builder.Services.AddSingleton(logStore);

// Configure Serilog for Blazor WebAssembly (browser) - logs go to the browser console and in-memory store
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.BrowserConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .WriteTo.Sink(new InMemorySink(logStore))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Local", options.ProviderOptions);
});

try
{
    Log.Information("Starting Blazor WebAssembly client");
    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Client terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
