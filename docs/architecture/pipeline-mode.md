# PipelineMode Behavior Matrix

Last Updated: 2026-02-09

## Overview

PipelineMode controls whether calculations trigger cascading downstream events. This is the critical control mechanism that prevents deadlocks during bulk operations while enabling real-time reactivity for user changes.

## Three Pipeline Modes

| Mode | Use Case | Cascade Behavior | Performance |
|------|----------|------------------|-------------|
| **Backfill** | Initial historical data load (7+ days) | Minimal - only "Calculated" events | Fast |
| **DaytimeReactive** | Real-time user changes, price updates | Full cascade - "Changed" + "Calculated" events | Slower, deadlock risk |
| **CloseOfDay** | Nightly batch reconciliation | Orchestrated via saga, no intermediate cascades | Medium, controlled |

## Event Publishing by Mode

| Event | Backfill | DaytimeReactive | CloseOfDay | Published By |
|-------|----------|-----------------|------------|--------------|
| `HoldingChangedEvent` | ❌ No | ✅ Yes | ❌ No | HoldingsEventHandler |
| `HoldingsCalculatedEvent` | ✅ Yes | ✅ Yes | ✅ Yes | HoldingsEventHandler |
| `HoldingValuationsChangedEvent` | ❌ No | ✅ Yes | ❌ No | ValuationsEventHandler |
| `HoldingValuationsCalculatedEvent` | ✅ Yes | ✅ Yes | ✅ Yes | ValuationsEventHandler |
| `PortfolioValuationChangedEvent` | ❌ No | ✅ Yes | ❌ No | ValuationsEventHandler |
| `PositionsCalculatedEvent` | ✅ Yes | ✅ Yes | ✅ Yes | PositionsEventHandler |
| `PositionPerformanceCalculatedEvent` | ✅ Yes | ✅ Yes | ✅ Yes | PerformanceEventHandler |

## Handler Reactions by Mode

| Handler | Reacts to "Changed" Events? | Notes |
|---------|---------------------------|-------|
| **HoldingsEventHandler** | Only in DaytimeReactive | `if (message.PipelineMode == PipelineMode.DaytimeReactive)` check |
| **ValuationsEventHandler** | Only in DaytimeReactive | Guards against backfill cascades |
| **PositionsEventHandler** | Only in DaytimeReactive | Prevents unnecessary recalculation |
| **PerformanceEventHandler** | Only in DaytimeReactive | Performance-sensitive operation |

## Code Locations

Search for `PipelineMode.DaytimeReactive` checks:

- `/Hoard.Bus/Holdings/HoldingsEventHandler.cs`
- `/Hoard.Bus/Valuations/ValuationsEventHandler.cs`
- `/Hoard.Bus/Positions/PositionsEventHandler.cs`
- `/Hoard.Bus/Performance/PerformanceEventHandler.cs`

Mode definition: `/Hoard.Messages/PipelineMode.cs`

## Decision Guidelines

### Use Backfill when:
- Loading 7+ days of historical data
- Migrating or importing bulk data
- Speed is critical, eventual consistency is acceptable
- Rebuilding metrics from scratch

### Use DaytimeReactive when:
- User makes a single transaction change
- Price updates during market hours
- Immediate UI feedback required
- Working with current/recent dates only (< 7 days)

### Use CloseOfDay when:
- Nightly batch reconciliation
- Rebuilding all metrics for accuracy
- Controlled orchestration required
- End-of-day snapshots

## Anti-Patterns

❌ **Don't:** Use DaytimeReactive for bulk backdating (30+ days)
- Creates hundreds of cascading messages
- High deadlock risk due to concurrent date-range processing
- Poor performance (30 days = ~120+ messages)

✅ **Do:** Switch to Backfill mode for bulk operations, then trigger single CloseOfDay recalc

❌ **Don't:** Use Backfill for single user transactions
- No immediate UI feedback
- Stale data until next CloseOfDay run

✅ **Do:** Use DaytimeReactive for individual user changes (1-5 days max)

## Performance Characteristics

### Backfill Mode
- Fastest processing time
- Minimal message volume
- No intermediate cascades
- Eventual consistency only

### DaytimeReactive Mode
- Slowest processing time
- Maximum message volume
- Full cascade chain: Transaction → Holdings → Valuations → Positions → Performance → Snapshots
- Immediate consistency
- **Deadlock risk with concurrent operations**

### CloseOfDay Mode
- Moderate processing time
- Controlled message volume via saga orchestration
- Sequential stage execution
- Guaranteed consistency at end of day

## Cascade Depth by Mode

```
DaytimeReactive (single backdated transaction):
  Transaction → Holdings (30×) → Valuations (30×) → Performance (30×) → Snapshots (30×)
  Total: 30 days × 5 layers = ~150 messages

Backfill (same transaction):
  Transaction → Holdings → Valuations → Performance → Snapshots
  Total: 5 messages (one per layer)

CloseOfDay (nightly run):
  Orchestrated via CloseOfDaySaga
  Total: ~20-30 messages (portfolio count × stages)
```

## Deadlock Scenarios

### Scenario 1: Concurrent Backdated Transactions
```
User A: Creates transaction dated 2024-01-15 (DaytimeReactive)
User B: Creates transaction dated 2024-01-20 (DaytimeReactive)

Both trigger overlapping date-range calculations:
- A calculates: 2024-01-15 to 2024-02-09
- B calculates: 2024-01-20 to 2024-02-09

Overlapping dates (2024-01-20 to 2024-02-09) cause:
- SQL row lock contention
- Rebus message ordering conflicts
- Potential deadlock
```

**Mitigation:** Limit backdating in DaytimeReactive to < 7 days

### Scenario 2: Invalidation Loop
```
PortfolioValuationInvalidationHandler → PortfolioValuationsInvalidated
  → PortfolioPerformanceInvalidationHandler → PortfolioPerformanceInvalidated
    → PortfolioValuationInvalidationHandler (circular dependency)
```

**Mitigation:** Both handlers check PipelineMode and defer actual recalculation

## Migration from Legacy Behavior

If you're removing PipelineMode checks:
1. Document the reason in an ADR
2. Load test with 30+ day backdating
3. Monitor for deadlocks in production
4. Consider adding date-range limits instead

## References

- [Transaction Created Event Flow](event-flows/transaction-created-daytime.md)
- [Close of Day Saga](sagas/close-of-day-saga.md)
- [Handler Dependencies](handler-dependencies.md)
- ADR-001: PipelineMode Strategy (to be created)
