# Hoard

Personal portfolio management and investment tracking system. Tracks holdings, valuations, and performance across multiple portfolios and accounts. Built for a single user (me) running on a home server.

## What it does

- Records transactions (buys, sells, dividends, etc.) against accounts grouped into portfolios
- Fetches market prices from Yahoo Finance and regulatory news from Investegate
- Calculates holdings, positions, valuations, performance, and portfolio snapshots
- Pushes alerts to mobile via Pushover
- Exposes an MCP server so Claude can query and manage portfolio data conversationally

## Architecture

Four runtime components, all containerised and deployed to Puck (home server) behind nginx at `hoard.iannelson.uk`:

| Component | What | Tech |
|---|---|---|
| **API** | REST API + SignalR hubs | ASP.NET Core |
| **Bus** | Async message handlers, sagas | Rebus / RabbitMQ worker |
| **Scheduler** | Recurring jobs (price refresh, daily calcs) | Hangfire worker |
| **UI** | SPA | Vue 3 / Vuetify |

Plus **SQL Server** and **RabbitMQ** running as separate stacks on Puck.

The backend is event-driven: transaction and price changes ripple through recalculation chains coordinated by Rebus sagas. Two main flows are documented in `docs/`:

**Close-of-day** — triggered by scheduler, recalculates everything:

```
Start -> Holdings + Prices -> Positions + Valuations -> Performance + Snapshots -> End
```

**Daytime reactive** — triggered by user activity or intraday price updates:

```
Transactions -> Holdings -> Positions + Valuations -> Performance + Snapshots
```

## Repository layout

```
src/
  server/           .NET solution (Api, Bus, Scheduler, Core, Messages, Test)
  client/hoard-ui/  Vue 3 SPA
  mcp/Hoard.Mcp/    Standalone MCP server (separate .csproj, not in main solution)
docs/               Event flow diagrams (Mermaid source + rendered PNGs)
tools/              Ad-hoc scripts and Postman collections
  contract note downloader/   Python script to pull contract note PDFs
  contract note parser/       Python script to parse contract notes
  postman/                    API collections and environments
```

## Getting started

See `CLAUDE.md` for build commands, configuration, deployment details, and full architectural notes. That file is the authoritative developer reference; this README is just the orientation.

A default server build needs no package credentials, but it leaves out regulatory news because Clarion, the library that fetches it, is a private package. News refreshes still run and complete; they just find no articles. Building with the MSBuild property `UseClarion=true` compiles Clarion in. The published images are built that way, which needs read access to the private GitHub Packages feed.

## Licence

MIT; see `LICENSE`. The bundled Inter fonts in `src/server/Hoard.Core/Infrastructure/Integrations/Fonts` are licensed separately under the SIL Open Font License 1.1; see `OFL.txt` in that folder.
