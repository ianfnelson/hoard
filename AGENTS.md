# Agents.md

This file provides guidance to Agentic coding assistants when working with code in this repository.

## Project Overview

Hoard is a portfolio management and investment tracking application with a .NET backend and Vue.js frontend.

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

### EF Core Migrations

```bash
# From src/server directory
dotnet ef migrations add MigrationName --project Hoard.Core --startup-project Hoard.Api
dotnet ef database update --project Hoard.Core --startup-project Hoard.Api
```

## Architecture

### Backend Structure (src/server)

The solution follows a Clean Architecture pattern with event-driven processing:

- **Hoard.Api** - ASP.NET Core Web API with REST endpoints, SignalR hubs for real-time updates
- **Hoard.Bus** - Message handler worker service that processes domain events (Rebus/RabbitMQ)
- **Hoard.Scheduler** - Hangfire-based job scheduler for recurring tasks (e.g., price refresh, daily calculations)
- **Hoard.Core** - Shared business logic and domain:
  - `Application/` - CQRS handlers organized by domain (Commands, Queries per entity type)
  - `Domain/Entities/` - Domain entities (Portfolio, Holding, Transaction, Price, etc.)
  - `Domain/Calculators/` - Business calculation logic
  - `Data/` - EF Core DbContext, migrations, and entity configurations
  - `Infrastructure/` - Service registration, Rebus configuration
- **Hoard.Messages** - Shared message contracts for pub/sub (events and bus commands)
- **Hoard.Test** - xUnit tests organized by project (Unit/Api, Unit/Bus, Unit/Core, Integration)

### Frontend Structure (src/client/hoard-ui)

Vue 3 SPA with Vuetify UI framework:
- `src/api/` - API client modules
- `src/pages/` - Page components
- `src/components/` - Reusable components
- `src/composables/` - Composable functions (with `__tests__/` for unit tests)
- `src/stores/` - Pinia state management
- `src/router/` - Vue Router configuration

### Frontend Code Style

The frontend uses ESLint and Prettier with modern Vue ecosystem conventions:

- **No semi-colons** - omit trailing semi-colons
- **Single quotes** - use `'string'` not `"string"`
- **Trailing commas** - include in multi-line arrays/objects
- **No explicit `any`** - use `catch (e)` not `catch (e: any)`

Run `npm run format` after making changes to ensure consistent formatting.

### Event-Driven Data Flow

The system uses Rebus with RabbitMQ for message-based processing. Two main flows:

**Close-of-day processing:** Start → Holdings + Prices → Positions + Valuations → Performance + Snapshots → End

**Reactive (daytime):** Transactions → Holdings → Positions + Valuations → Performance + Snapshots

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
- **Market Data:** YahooFinanceApiNext for price quotes
- **Telemetry:** Application Insights

## Configuration

Connection strings are configured via user secrets or appsettings:
- `HoardDatabase` - SQL Server connection
- `RabbitMq` - RabbitMQ connection
- `ApplicationInsights` - Application Insights connection
