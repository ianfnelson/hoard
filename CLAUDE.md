# CLAUDE.md

## Project Overview

Hoard is a portfolio management and investment tracking application with a .NET backend, Vue.js frontend, and an MCP server for AI-assisted interaction.

## Roadmap

Issues, bugs, and feature requests are tracked in GitHub: https://github.com/ianfnelson/hoard/issues

## Build and Run Commands

### Server (.NET 10)

```bash
# From src/server directory
dotnet restore Hoard.sln
dotnet build Hoard.sln
dotnet test Hoard.sln

# Run individual projects
dotnet run --project Hoard.Api
dotnet run --project Hoard.Bus
dotnet run --project Hoard.Scheduler

# Run a single test
dotnet test Hoard.sln --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

### Client (Vue 3 + Vite)

```bash
# From src/client/hoard-ui directory
npm install
npm run dev        # Development server (proxies /api and /hubs to localhost:5183)
npm run build      # Production build
npm run lint       # Check for linting issues
npm run lint:fix   # Auto-fix linting issues
npm run format     # Format code with Prettier
npm run test       # Run tests in watch mode
npm run test:run   # Run tests once
npm run type-check # Type-check without emitting
```

Pre-commit hooks (via Husky + lint-staged) automatically run ESLint and Prettier on staged files.

### MCP Server

```bash
# From src/mcp/Hoard.Mcp directory
dotnet build
dotnet run          # Runs via stdio transport

# Publish for Claude Code usage
dotnet publish -c Release -o ../publish
```

The MCP server is configured in `.mcp.json` at the repo root. It communicates with the Hoard API over HTTP (base URL configurable via `HOARD_API_BASE_URL` env var, defaults to `http://hoard.iannelson.uk/api`).

### EF Core Migrations

```bash
# From src/server directory
dotnet ef migrations add MigrationName --project Hoard.Core --startup-project Hoard.Api
dotnet ef database update --project Hoard.Core --startup-project Hoard.Api
```

## Architecture

### Repository Layout

```
src/
  server/          # Main .NET solution (Hoard.sln) - 6 projects
  client/hoard-ui/ # Vue 3 SPA
  mcp/Hoard.Mcp/   # Standalone MCP server (separate from main solution)
```

### Backend Structure (src/server)

The solution follows Clean Architecture with event-driven processing:

- **Hoard.Api** - ASP.NET Core Web API with REST endpoints, SignalR hubs for real-time updates. Served behind nginx with path base `/api`.
- **Hoard.Bus** - Message handler worker service (Rebus/RabbitMQ). Handlers are split by concern:
  - `CommandHandlers` - Receive bus commands and dispatch to the mediator
  - `Propagators` - Handle domain events and trigger downstream bus commands
  - `Sagas` - Coordinate multi-step workflows (quote refresh, instrument analysis, backfill/recalculation operations)
- **Hoard.Scheduler** - Hangfire-based job scheduler for recurring tasks (price refresh, daily calculations). Uses UK timezone for schedules.
- **Hoard.Core** - Shared business logic and domain:
  - `Application/` - CQRS handlers organized by domain (Commands, Queries per entity type)
  - `Domain/Entities/` - Domain entities
  - `Domain/Calculators/` - Business calculation logic
  - `Data/` - EF Core DbContext (with DateTimeKind.Utc on reads), migrations, entity configurations
  - `Infrastructure/` - Service registration, Rebus configuration, and `Integrations/` (external service adapters)
- **Hoard.Messages** - Shared message contracts for pub/sub (events and bus commands)
- **Hoard.Test** - xUnit tests organized by project (Unit/Api, Unit/Bus, Unit/Core, Integration)

### Domain Entities

Core: Portfolio, Account, Instrument, Transaction, Holding, Position, Price, Quote, Valuation, HoldingValuation, PortfolioValuation, PortfolioSnapshot, Performance, PortfolioPerformance, PositionPerformance.

Reference data: AssetClass, AssetSubclass, Currency, InstrumentType, TransactionType, DataFeed, InstrumentDataFeed, DataFeedConfiguration, RunType, RunLog, AlertType.

Features: Alert, NewsArticle, NewsAlertSuppressionRule, InstrumentAnalysis, TargetAllocation.

### MCP Server (src/mcp)

A standalone .NET MCP server that exposes Hoard's REST API as MCP tools via stdio transport. It is **not** part of the main solution — it's a separate project with its own `.csproj`.

Tools are organized into 6 modules:
- **PortfolioTools** - List, detail, positions, exposure/rebalancing, snapshots, valuations, valuation series, instrument types
- **InstrumentTools** - Search, detail, lookup, prices, price series, valuation series
- **TransactionTools** - Full CRUD (create, read, update, delete) plus listing with filters
- **AccountTools** - List and detail
- **ReferenceDataTools** - Asset classes, currencies, instrument types, transaction types
- **NewsTools** - Article listing and detail

Tools are auto-discovered via `[McpServerToolType]` and `[McpServerTool]` attributes. The server wraps the REST API through `HoardApiClient`.

### Frontend Structure (src/client/hoard-ui)

Vue 3 SPA with Vuetify UI framework:
- `src/api/` - API client modules
- `src/pages/` - Page components (portfolios, instruments, transactions, news, alerts, system operations)
- `src/components/` - Reusable components (including searchable instrument autocomplete)
- `src/composables/` - Composable functions (with `__tests__/` for unit tests)
- `src/constants/` - Shared constants
- `src/layouts/` - Layout components (responsive with mobile hamburger menu)
- `src/stores/` - Pinia state management (instrument, navigation, portfolio, referenceData)
- `src/styles/` - Global styles
- `src/router/` - Vue Router configuration
- `src/utils/` - Utility functions

### Frontend Code Style

The frontend uses ESLint and Prettier with modern Vue ecosystem conventions:

- **No semi-colons** - omit trailing semi-colons
- **Single quotes** - use `'string'` not `"string"`
- **Trailing commas** - include in multi-line arrays/objects
- **No explicit `any`** - use `catch (e)` not `catch (e: any)`

Run `npm run format` after making changes to ensure consistent formatting.

### Event-Driven Data Flow

The system uses Rebus with RabbitMQ for message-based processing. Multi-step workflows use Rebus sagas for coordination. Two main flows:

**Close-of-day processing:** Start -> Holdings + Prices -> Positions + Valuations -> Performance + Snapshots -> End

**Reactive (daytime):** Transactions -> Holdings -> Positions + Valuations -> Performance + Snapshots

Key event chains:
- Transaction changes trigger holding recalculation
- Price/quote changes trigger valuation recalculation
- Valuations trigger performance and snapshot calculations

### CQRS Pattern

The codebase uses a custom Mediator pattern (`Hoard.Core/Application/Mediator.cs`):
- Commands implement `ICommand` or `ICommand<TResult>`
- Queries implement `IQuery<TResult>`
- Handlers are auto-registered via Scrutor assembly scanning

## External Dependencies

- **Database:** SQL Server (EF Core)
- **Message Bus:** RabbitMQ (Rebus)
- **Job Scheduling:** Hangfire (SQL Server storage)
- **Market Data:** YahooFinanceApiNext for price quotes, configured per instrument via the DataFeed/InstrumentDataFeed model
- **Regulatory News:** Clarion (in-house NuGet package, `ianfnelson/clarion`) - scrapes and parses UK regulatory announcements from Investegate by ticker. Runs in-process, not as a separate service.
- **Blob Storage:** Azure Blob Storage for contract note PDFs
- **Telemetry:** Application Insights
- **Notifications:** Pushover (alerts/events pushed to mobile)

## Configuration

Connection strings are configured via user secrets or appsettings:
- `HoardDatabase` - SQL Server connection
- `RabbitMq` - RabbitMQ connection
- `AzureBlobStorage` - Azure Blob Storage connection
- `ApplicationInsights` - Application Insights connection

Additional settings:
- `BlobStorage:ContainerName` - Blob container name (default: "documents")
- `Pushover:Token` - Pushover API token
- `Pushover:User` - Pushover user key

## Deployment

Hoard runs on Puck (home server, AMD64 Ubuntu) as Docker containers, with images published to GHCR (`ghcr.io/ianfnelson/hoard-*`). Infrastructure config lives in the separate `puck` repository.

### Containers

| Container | Image | Exposed Port |
|---|---|---|
| hoard-api | ghcr.io/ianfnelson/hoard-api | internal (8080) |
| hoard-bus | ghcr.io/ianfnelson/hoard-bus | internal |
| hoard-scheduler | ghcr.io/ianfnelson/hoard-scheduler | 7000:8080 |
| hoard-ui | ghcr.io/ianfnelson/hoard-ui | internal (80) |
| hoard-proxy | nginx:alpine | 80:80 |

### Networking

All Hoard containers share a `puck-shared` external Docker network with SQL Server and RabbitMQ (which run as separate compose stacks on Puck).

### Nginx Routing (hoard.iannelson.uk)

- `/` -> hoard-ui (frontend)
- `/api/*` -> hoard-api (REST API)
- `/hubs/*` -> hoard-api (SignalR, with WebSocket upgrade headers)

### Environment Variables

The `.env` file on Puck maps secrets to the .NET configuration pattern:

| Env Var | Maps To |
|---|---|
| `SQL_CONNECTION` | `ConnectionStrings__HoardDatabase` |
| `RABBIT_CONNECTION` | `ConnectionStrings__RabbitMq` |
| `APPINSIGHTS_CONNECTION` | `ConnectionStrings__ApplicationInsights` |
| `AZURE_STORAGE_CONNECTION` | `ConnectionStrings__AzureBlobStorage` |
| `PUSHOVER_TOKEN` | `Pushover__Token` |
| `PUSHOVER_USER` | `Pushover__User` |
