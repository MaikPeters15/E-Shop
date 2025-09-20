var builder = WebApplication.CreateBuilder(args);

// In-memory store for exposing logs to the client
var logStore = new InMemoryLogStore(capacity: 2000);

// Configure Serilog to log to Console, rolling files, and in-memory store for diagnostics
builder.Host.UseSerilog((ctx, services, loggerConfiguration) =>
    loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/logs-.txt",
            rollingInterval: RollingInterval.Day,
            shared: true)
        .WriteTo.Sink(new InMemorySink(logStore))
);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Expose log store and CORS (dev)
builder.Services.AddSingleton(logStore);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Log incoming HTTP requests
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors();
}

app.UseHttpsRedirection();

// Simple log endpoints for the Blazor client to consume
app.MapGet("/logs", (InMemoryLogStore store) => Results.Ok(store.GetAll()));
app.MapDelete("/logs", (InMemoryLogStore store) => { store.Clear(); return Results.NoContent(); });

app.MapGet("/hello", () => "Hello World!");

app.Run();
