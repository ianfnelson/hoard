# Hoard

Personal portfolio management and investment tracking system. Tracks holdings, valuations, and performance across multiple portfolios and accounts. Built for a single user (me) and runs on a home server; it's shared as a working example rather than a supported product.

## What it does

- Records transactions (buys, sells, dividends, etc.) against accounts, which are grouped into portfolios
- Fetches end-of-day prices and intraday quotes from Yahoo Finance
- Pulls UK regulatory news (RNS) for held instruments from Investegate
- Calculates holdings, positions, valuations, performance, and daily portfolio snapshots
- Shows portfolio exposure against target allocations to support rebalancing
- Analyses instruments for 52-week highs and lows and abnormal trading volume
- Raises alerts (quote movements, portfolio drawdowns and all-time highs, analysis signals, news) and pushes them to mobile via Pushover
- Stores contract note PDFs in Azure Blob Storage and exports portfolio valuations as PDF
- Pushes live updates to the UI over SignalR
- Exposes an MCP server so Claude (or any MCP client) can query portfolio data and manage transactions conversationally

## Architecture

| Component | What | Tech |
|---|---|---|
| **API** | REST API and SignalR hubs | ASP.NET Core |
| **Bus** | Async message handlers and sagas | Rebus / RabbitMQ worker |
| **Scheduler** | Recurring jobs: quotes, news, start and close of day | Hangfire worker |
| **UI** | Single-page app | Vue 3 / Vuetify |
| **MCP server** | Wraps the REST API as MCP tools over stdio; runs locally alongside the MCP client | .NET, MCP C# SDK |

All are .NET 10 apart from the UI. The API, Bus, Scheduler, and UI run as Docker containers behind nginx on my home server, alongside SQL Server and RabbitMQ.

The backend is event-driven: transaction and price changes ripple through recalculation chains coordinated by Rebus sagas.

**Close of day**, triggered by the scheduler, recalculates everything:

```mermaid
flowchart TD
    A[Start] --> B[Holdings]
    A --> C[Prices]
    B --> D[Positions]
    B --> E[Valuations]
    C --> E
    C --> I[Analysis]
    E --> F[Performance]
    D --> F
    E --> G[Snapshots]
    F --> H[End]
    G --> H
    I --> H
```

**Daytime reactive**, triggered by transaction changes and intraday price and quote updates:

```mermaid
flowchart TD
    A[Transactions] --> B[Holdings]
    C[Prices] --> E[Valuations]
    H[Quotes] --> E
    B --> D[Positions]
    B --> E
    E --> F[Performance]
    D --> F
    E --> G[Snapshots]
```

## Repository layout

```
src/
  server/           .NET solution (Api, Bus, Scheduler, Core, Messages, Test)
  client/hoard-ui/  Vue 3 SPA
  mcp/Hoard.Mcp/    MCP server (separate .csproj, not in the main solution)
docs/               Event flow diagrams (Mermaid source and rendered PNGs)
tools/              Ad-hoc scripts and Postman collections
  contract note downloader/   Python script to pull contract note PDFs
  contract note parser/       Python script to parse contract notes
  postman/                    API collections and environments
```

## Running locally

### Prerequisites

- .NET 10 SDK
- Node.js (current LTS)
- SQL Server
- RabbitMQ
- Azurite, or an Azure Storage account
- A GitHub personal access token with `read:packages`. The news scraper, [Clarion](https://github.com/ianfnelson/clarion), is restored from GitHub Packages, which requires authentication.
- A Pushover app token and user key. These must be set for the services to start; placeholder values work if you don't need notifications.

### Configure

Credentials for the GitHub Packages feed (the source is named `github` in `src/server/NuGet.Config`):

```bash
export NuGetPackageSourceCredentials_github="Username=<github-username>;Password=<token>"
```

Application settings, as environment variables for the API, Bus, and Scheduler (or `dotnet user-secrets` per project, using `:` in place of `__`):

```bash
export ConnectionStrings__HoardDatabase="Server=localhost;Database=Hoard;User Id=sa;Password=<password>;TrustServerCertificate=True;"
export ConnectionStrings__RabbitMq="amqp://guest:guest@localhost/"
export ConnectionStrings__AzureBlobStorage="UseDevelopmentStorage=true"
export Pushover__Token="<token>"
export Pushover__User="<user-key>"
```

### Run

```bash
# Backend, from src/server, each in its own terminal
dotnet run --project Hoard.Api        # applies EF migrations and seeds reference data on startup
dotnet run --project Hoard.Bus
dotnet run --project Hoard.Scheduler

# Frontend, from src/client/hoard-ui
npm install
npm run dev                            # proxies /api and /hubs to the API on localhost:5183

# Tests, from src/server
dotnet test Hoard.sln
```

### MCP server

```bash
# From src/mcp/Hoard.Mcp
dotnet publish -c Release -o ../publish
```

Then register it with your MCP client, pointing it at your API:

```json
{
  "mcpServers": {
    "hoard": {
      "command": "dotnet",
      "args": ["<repo>/src/mcp/publish/Hoard.Mcp.dll"],
      "env": { "HOARD_API_BASE_URL": "http://localhost:5183/api" }
    }
  }
}
```

## Licence

MIT; see `LICENSE`. The bundled Inter fonts in `src/server/Hoard.Core/Infrastructure/Integrations/Fonts` are licensed separately under the SIL Open Font License 1.1; see `OFL.txt` in that folder.
