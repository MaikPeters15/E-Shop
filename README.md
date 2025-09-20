# E‑Shop (ASP.NET Core 10, .NET 10, EF Core 10, Blazor, SQL Server 2022)

Ein moderner, wartbarer E‑Commerce‑Starter basierend auf .NET 10 mit ASP.NET Core 10, Entity Framework Core 10, Blazor (Server) und Microsoft SQL Server 2022. Fokus auf Clean Code, Clean Architecture und Best Practices.

## Inhalte
- Features
- Architektur & Projektstruktur
- Voraussetzungen
- Schnellstart
- Konfiguration
- Datenbank & Migrationen
- Entwicklung & Qualität
- Tests
- Deployment (Docker)
- Roadmap
- Lizenz

---

## Features
- Blazor Server UI mit reaktiven Komponenten (SSR, SignalR)
- Saubere Schichtenarchitektur (Domain, Application, Infrastructure, Web)
- EF Core 10 mit Code‑First, Fluent Mappings und Migrations
- SQL Server 2022 als persistente Datenbank
- Konfigurierbare Connection Strings über Umgebungsvariablen/Secrets
- Valides Domain‑Modell (Entities, Value Objects, Aggregates)
- Fehlerbehandlung mit ProblemDetails
- Logging (erweiterbar, z. B. Serilog)
- Seeding für Demo‑Daten (optional)
- CI/CD‑bereit (GitHub Actions Vorlage optional)

---

## Architektur & Projektstruktur

Clean Architecture: Abhängigkeiten zeigen von außen nach innen (Web → Application → Domain); die Infrastructure implementiert Ports aus der Application.

```
src/
  Domain/
    Entities/
    ValueObjects/
    Events/
    Abstractions/
  Application/
    DTOs/
    Interfaces/            # Ports (z. B. IProductRepository, IUnitOfWork)
    Services/              # Use-Cases / Orchestrierung
    Validation/            # z. B. FluentValidation
    Mapping/               # z. B. AutoMapper/Mapster Profile
  Infrastructure/
    Persistence/
      EShopDbContext.cs
      Configurations/      # Fluent API Mappings
      Migrations/
    Repositories/
    Services/
Client/                     # Blazor Server + Minimal APIs (optional)
    Pages/
    Components/
    Services/
    appsettings.json
tests/
  Domain.Tests/
  Application.Tests/
  Infrastructure.Tests/    # optional: mit Testcontainers für SQL Server
  Web.Tests/               # UI-/Browsertests optional (bunit/Playwright)
```

Leitprinzipien:
- Single Responsibility, klare Schnittstellen (SOLID)
- Keine Domain‑Abhängigkeit von Frameworks
- Explizite Fehler (Result/ProblemDetails), kein Schweigen bei Fehlern
- Nullability + Analyzers + Warnungen als Fehler in CI
- Konsistenter Code‑Stil über .editorconfig

---

## Voraussetzungen

- .NET SDK 10.0.x
- SQL Server 2022 (lokal oder via Docker)
- (Optional) Docker Desktop für lokale DB/Container
- (Optional) EF Core Tools:
  - Installation: 
    ```bash
    dotnet tool install -g dotnet-ef
    ```

---

## Schnellstart

1) Repository klonen
```bash
git clone https://github.com/<owner>/<repo>.git
cd <repo>
```

2) SQL Server 2022 via Docker starten (empfohlen)
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
  -p 1433:1433 --name eshop-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

3) Konfiguration setzen (dev)
- Variante A: appsettings.Development.json in `src/Web` pflegen (siehe Beispiel unten).
- Variante B: Umgebungsvariable:
  - Windows (PowerShell):
    ```powershell
    $Env:ConnectionStrings__Default="Server=localhost,1433;Database=EShop;User ID=sa;Password=Your_password123;TrustServerCertificate=True"
    ```
  - macOS/Linux (bash):
    ```bash
    export ConnectionStrings__Default="Server=localhost,1433;Database=EShop;User ID=sa;Password=Your_password123;TrustServerCertificate=True"
    ```

4) Datenbank migrieren
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

5) Anwendung starten
```bash
dotnet run --project src/Web
```

Die App ist standardmäßig erreichbar unter http://localhost:5000 (oder https://localhost:5001).

---

## Konfiguration

Beispiel `appsettings.Development.json` (unter `src/Web`):

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=EShop;User ID=sa;Password=Your_password123;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Konfigurationshierarchie:
1. appsettings.json
2. appsettings.{Environment}.json
3. Benutzergeheimnisse (dotnet user-secrets) in Entwicklung
4. Umgebungsvariablen (empfohlen in Containern)
5. Kommandozeilenargumente

---

## Datenbank & Migrationen

Neue Migration erstellen:
```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure --startup-project src/Web
```

Migration anwenden:
```bash
dotnet ef database update \
  --project src/Infrastructure --startup-project src/Web
```

## Entwicklung & Qualität

Empfohlene Einstellungen:
- Nullable Reference Types: aktiv
- TreatWarningsAsErrors: true
- Code‑Analyzers (z. B. Microsoft.CodeAnalysis.NetAnalyzers, StyleCop.Analyzers)
- Konsistente Formatierung per `.editorconfig`
- PR‑Validierung über GitHub Actions (Build, Tests, Format, Analyzers)

Beispiel `.editorconfig` (Ausschnitt):
```ini
[*.cs]
dotnet_diagnostic.IDE0005.severity = warning
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = true:suggestion
```

Fehlerbehandlung:
- HTTP APIs: RFC 7807 ProblemDetails
- UI: Benutzerfreundliche Validierungsfehler
- Logging: strukturierte Logs (z. B. Serilog) mit Korrelation (ActivityId)

Leistung:
- AsNoTracking für reine Lesequeries
- Pagination, Filter, Sortierung serverseitig
- Caching (MemoryCache/OutputCache) bei Bedarf

Sicherheit:
- Immer HTTPS
- Secrets nie im Code
- Prepared Statements (EF Core standard)
- Validierung serverseitig
- OWASP ASVS/Top10 berücksichtigen

---

## Tests

- Unit Tests (Domain, Application): reine Geschäftslogik
- Integrationstests (Infrastructure): echte DB (z. B. Testcontainers)
- UI‑Tests (Web): bUnit/Playwright (optional)

Beispiel Testcontainers (Ausschnitt, C#):
```csharp
var msSqlContainer = new MsSqlContainerBuilder()
    .WithPassword("Your_password123")
    .Build();
await msSqlContainer.StartAsync();

// ConnectionString: msSqlContainer.GetConnectionString()
```

---

## Deployment (Docker)

Dockerfile (Beispiel für Blazor Server):

```dockerfile
# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Erwartet ConnectionStrings__Default als Umgebungsvariable
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Web.dll"]
```

Docker Compose (App + SQL Server):
```yaml
version: "3.9"
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Your_password123"
    ports:
      - "1433:1433"
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "Your_password123", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 10

  web:
    build:
      context: .
      dockerfile: ./Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: "Production"
      ConnectionStrings__Default: "Server=db,1433;Database=EShop;User ID=sa;Password=Your_password123;TrustServerCertificate=True"
    depends_on:
      db:
        condition: service_healthy
    ports:
      - "8080:8080"
```

Start:
```bash
docker compose up --build
```

---

## Roadmap (Vorschläge)
- Identität & Auth (ASP.NET Core Identity, JWT, Rollen)
- Warenkorb, Checkout, Zahlungen (Stripe/PayPal)
- Katalog: Kategorien, Varianten, Inventar
- Bestellungen, Versand, Rechnungen
- Internationalisierung (i18n), Mehrwährung, Steuern
- Observability (OpenTelemetry, Grafana/Prometheus)
- Mehrmandantenfähigkeit
- Blazor WebAssembly Hosted Option

---

## Lizenz

MIT (oder projektkonform). Füge deine `LICENSE` hinzu.

---

## Hinweise

- Diese Vorlage nimmt Blazor Server an. Eine Hosted‑WASM‑Variante ist problemlos möglich (separates Client‑Projekt und API).
- Stelle sicher, dass du .NET SDK 10.0 verwendest und kompatible NuGet‑Pakete (ASP.NET Core 10 / EF Core 10).
