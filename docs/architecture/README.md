# Hoard Architecture Documentation

Last Updated: 2026-02-09

## Overview

This directory contains comprehensive documentation of Hoard's event-driven architecture, focusing on the message-based processing pipeline that handles portfolio calculations.

## Quick Start

**New to the codebase?** Start here:

1. [PipelineMode Behavior Matrix](pipeline-mode.md) - Understand the three processing modes
2. [Handler Dependencies](handler-dependencies.md) - See the full event cascade map
3. [Close of Day Saga](sagas/close-of-day-saga.md) - Understand nightly batch processing

**Debugging a deadlock?** Read:
- [Transaction Created Event Flow](event-flows/transaction-created-daytime.md)

## Architecture at a Glance

Hoard uses an event-driven architecture with Rebus (message bus) and RabbitMQ:

```
User Transaction → Holdings → Valuations → Positions → Performance → Snapshots
                      ↓           ↓            ↓             ↓            ↓
                   (Events)   (Events)     (Events)      (Events)    (Events)
```

### Three Processing Modes

| Mode | Use Case | Performance | Cascading |
|------|----------|-------------|-----------|
| **DaytimeReactive** | Real-time user changes | Slow | Full cascade |
| **Backfill** | Bulk historical import | Fast | Minimal |
| **CloseOfDay** | Nightly batch reconciliation | Medium | Orchestrated |

See: [PipelineMode Behavior Matrix](pipeline-mode.md)

### Cascade Depth

**Maximum depth: 5 levels**

```
Level 1: Transaction
Level 2: Holdings
Level 3: Valuations
Level 4: Positions
Level 5: Performance
Level 6: Snapshots
```

**Message amplification:**
- Single transaction dated 30 days ago = ~150 messages (30 days × 5 levels)

## Documentation Structure

```
docs/
├── architecture/
│   ├── README.md (this file)
│   ├── pipeline-mode.md                    # Core: PipelineMode decision matrix
│   ├── handler-dependencies.md             # Core: Full event cascade map
│   ├── event-flows/                        # Detailed event sequence diagrams
│   │   └── transaction-created-daytime.md  # Example: Transaction cascade
│   └── sagas/                              # Saga state machines
│       └── close-of-day-saga.md            # Nightly batch orchestration
├── adr/                                    # Architecture Decision Records
│   ├── README.md
│   ├── template.md
│   ├── 001-pipeline-mode-strategy.md       # Why three modes exist
│   ├── 002-saga-correlation-keys.md        # How sagas correlate events
│   └── 003-date-range-cascade-limits.md    # Proposed: 7-day limit for DaytimeReactive
```

## Key Concepts

### PipelineMode

Controls whether calculations trigger cascading downstream events.

- **DaytimeReactive:** Full cascade (real-time updates)
- **Backfill:** Minimal cascade (fast bulk import)
- **CloseOfDay:** Saga-orchestrated (nightly reconciliation)

**Critical files:**
- `/Hoard.Messages/PipelineMode.cs`
- `/Hoard.Bus/*/EventHandler.cs` (search for `PipelineMode.DaytimeReactive`)

### Sagas

Multi-step orchestration using Rebus sagas:

- **CloseOfDaySaga:** Master nightly batch coordinator
- **RefreshPricesSaga:** Price fetching orchestration
- **CalculateValuationsSaga:** Two-phase valuation calculation
- **CalculatePerformanceSaga:** Two-phase performance calculation

**Critical files:**
- `/Hoard.Bus/Chrono/CloseOfDaySaga.cs`
- `/Hoard.Bus/Valuations/CalculateValuationsSaga.cs`
- `/Hoard.Bus/Performance/CalculatePerformanceSaga.cs`

### Event Cascading

When an event triggers downstream events:

**DaytimeReactive Mode (full cascade):**
```
TransactionCreated
  → HoldingChangedEvent (reactive)
    → HoldingValuationsChangedEvent (reactive)
      → PortfolioValuationChangedEvent (reactive)
        → PerformanceCalculatedEvent (reactive)
```

**Backfill Mode (minimal cascade):**
```
TransactionCreated
  → HoldingsCalculatedEvent (end-of-stage only)
```

### Deadlock Scenarios

**Scenario 1: Overlapping Date Ranges**
- User A: Transaction dated 2024-01-15 (processes 15th to today)
- User B: Transaction dated 2024-01-20 (processes 20th to today)
- Overlap causes SQL row lock contention → deadlock

**Scenario 2: Deep Backdating**
- Single transaction dated 90 days ago
- Creates ~450 messages
- SQL lock escalation (row locks → table lock)
- All other operations blocked

**Mitigation:**
- Limit DaytimeReactive to 7 days (proposed in ADR-003)
- Use Backfill mode for bulk imports

## Common Tasks

### Debugging a Stuck Saga

```sql
-- Find stuck saga instances
SELECT TOP 10
    [SagaType],
    JSON_VALUE([Data], '$.Date') AS [Date],
    [Revision],
    [Id]
FROM [RebusMessages].[Sagas]
WHERE [SagaType] = 'CloseOfDaySaga'
ORDER BY [Revision] DESC;
```

### Tracing an Event Cascade

```csharp
// Add correlation ID to all events
public record TransactionCreatedEvent(
    Guid TransactionId,
    Guid CorrelationId // ← Use to trace cascade
);

// Application Insights query
traces
| where customDimensions.CorrelationId == "abc-123"
| order by timestamp asc
| project timestamp, message, customDimensions.EventType
```

### Identifying Deadlocks

```kusto
// Application Insights query
exceptions
| where outerMessage contains "deadlock"
| summarize Count=count() by bin(timestamp, 1h)
| render timechart
```

## Performance Characteristics

### DaytimeReactive Mode

| Backdate Days | Messages | Processing Time | Deadlock Risk |
|---------------|----------|-----------------|---------------|
| 1 day | ~6 | < 1 second | Low |
| 7 days | ~42 | 5-10 seconds | Medium |
| 30 days | ~180 | 30-60 seconds | High |
| 90 days | ~540 | 2-5 minutes | Critical |

### Backfill Mode

| Backdate Days | Messages | Processing Time | Deadlock Risk |
|---------------|----------|-----------------|---------------|
| Any | ~5 | < 5 seconds | Minimal |

### CloseOfDay Mode

| Operation | Duration |
|-----------|----------|
| Prices | 10-30 seconds |
| Holdings | 5-15 seconds |
| Valuations | 10-30 seconds |
| Performance | 15-45 seconds |
| Snapshots | 5-10 seconds |
| **Total** | **45-120 seconds** |

## Related Documentation

### In This Repository

- [Agents.md](/Agents.md) - Agent coding guidelines
- [CLAUDE.md](/CLAUDE.md) - Points to Agents.md

### External Resources

- [Rebus Documentation](https://github.com/rebus-org/Rebus/wiki)
- [Mermaid Diagram Syntax](https://mermaid.js.org/)
- [ADR GitHub Organization](https://adr.github.io/)
- [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/)

## Maintenance

### Updating Documentation

When making architectural changes:

1. **Update affected diagrams** (Mermaid sequence/state diagrams)
2. **Update PipelineMode matrix** if behavior changes
3. **Create new ADR** for significant decisions
4. **Update "Last Updated" date** at top of modified files
5. **Include documentation updates in PR**

### Review Cadence

- **Weekly:** Check for stale "Last Updated" dates
- **Monthly:** Review ADRs for accuracy
- **Quarterly:** Audit diagrams against actual code

### Tools

**Mermaid Preview:**
- VS Code: Install "Mermaid Preview" extension
- JetBrains Rider: Built-in support
- GitHub: Native rendering in .md files

**Mermaid Live Editor:**
- https://mermaid.live/ (for prototyping diagrams)

## Contributing

When adding new sagas, handlers, or event flows:

1. Add corresponding documentation:
   - Event flow diagram (if new cascade)
   - Saga state machine (if new saga)
   - Update handler dependencies map
2. Document PipelineMode behavior (if applicable)
3. Add ADR for significant architectural decisions
4. Include code references (file paths + line numbers)

## Questions?

- **Architecture questions:** Review ADRs first
- **Deadlock debugging:** See [Transaction Created Event Flow](event-flows/transaction-created-daytime.md)
- **Saga troubleshooting:** See [Close of Day Saga](sagas/close-of-day-saga.md)
- **Mode selection:** See [PipelineMode Behavior Matrix](pipeline-mode.md)
