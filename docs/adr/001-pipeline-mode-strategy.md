# 001. PipelineMode Strategy

Date: 2026-02-09

## Status

Accepted (documenting existing implementation)

## Context

Hoard's event-driven architecture features deep cascading calculations: when a transaction is created, it triggers holdings recalculation, which triggers valuations, which triggers performance, which triggers snapshots. For a single backdated transaction (e.g., 30 days ago), this creates 30 × 5 = ~150 messages.

**Problems observed:**
1. **Deadlocks:** Concurrent backdated transactions create overlapping date ranges, causing SQL row lock contention and deadlocks
2. **Performance:** Bulk data imports (100+ transactions) trigger thousands of cascading messages, taking hours to process
3. **Predictability:** Nightly batch reconciliation needs controlled, sequential execution without race conditions

**Requirements:**
1. Real-time updates for single user transactions (immediate UI feedback)
2. Fast bulk data imports without cascading recalculations
3. Reliable nightly batch processing with guaranteed consistency

## Decision

Introduce a `PipelineMode` enum with three values that control event cascading behavior:

### 1. DaytimeReactive
**Use case:** Real-time user changes (1-5 transactions, recent dates only)

**Behavior:**
- Publishes ALL events: `*ChangedEvent` + `*CalculatedEvent`
- Handlers react immediately to `*ChangedEvent`
- Full cascade chain: Transaction → Holdings → Valuations → Performance → Snapshots
- Immediate consistency for UI

**Trade-off:** Slow for bulk operations, deadlock risk for backdating

### 2. Backfill
**Use case:** Bulk historical data import (100+ transactions, 7+ days)

**Behavior:**
- Publishes ONLY `*CalculatedEvent` (no `*ChangedEvent`)
- Handlers skip reactive cascading
- Minimal message volume
- Eventual consistency (stale until next CloseOfDay)

**Trade-off:** Fast but no real-time updates

### 3. CloseOfDay
**Use case:** Nightly batch reconciliation

**Behavior:**
- Orchestrated via CloseOfDaySaga (sequential stages)
- Publishes only `*CalculatedEvent`
- Explicit saga coordination instead of reactive cascading
- Guaranteed consistency at end of saga

**Trade-off:** Only runs once per day (not for real-time)

### Implementation

All events and commands carry a `PipelineMode` property:

```csharp
public record TransactionCreatedEvent(
    Guid TransactionId,
    DateOnly Date,
    PipelineMode PipelineMode // ← Mode propagated through cascade
);
```

Handlers check mode before publishing cascading events:

```csharp
// HoldingsEventHandler.cs
if (message.PipelineMode == PipelineMode.DaytimeReactive)
{
    // Only cascade in DaytimeReactive mode
    await _bus.Publish(new HoldingValuationsChangedEvent(...));
}
```

## Consequences

### Positive

1. **Eliminates bulk import deadlocks:** Backfill mode processes 100+ transactions in seconds instead of timing out
2. **Predictable nightly processing:** CloseOfDay saga ensures consistent execution order and completion
3. **Real-time responsiveness preserved:** DaytimeReactive mode still provides immediate UI feedback for user changes
4. **Explicit control:** Mode is visible in logs, making debugging easier ("Why didn't this cascade?" → "Oh, it was Backfill mode")

### Negative

1. **Complexity:** Three different code paths for the same logical operation (calculate holdings)
2. **Scattered conditional logic:** PipelineMode checks in 15+ handler files
3. **Fragile consistency:** Easy to forget mode check in new handler, causing subtle bugs
4. **Testing burden:** Must test all operations in all three modes

### Neutral

1. **Mode selection responsibility:** Caller must choose correct mode (API layer decides based on operation type)
2. **Stale data during Backfill:** Acceptable trade-off for performance, but users must be aware

## Implementation Notes

### File Locations

**PipelineMode enum:**
- `/Hoard.Messages/PipelineMode.cs`

**Mode checks (search for `PipelineMode.DaytimeReactive`):**
- `/Hoard.Bus/Holdings/HoldingsEventHandler.cs`
- `/Hoard.Bus/Valuations/ValuationsEventHandler.cs`
- `/Hoard.Bus/Positions/PositionsEventHandler.cs`
- `/Hoard.Bus/Performance/PerformanceEventHandler.cs`

**Mode selection:**
- `/Hoard.Api/Controllers/TransactionsController.cs` - Chooses DaytimeReactive for single user changes
- `/Hoard.Scheduler/Jobs/CloseOfDayJob.cs` - Uses CloseOfDay for nightly batch
- `/Hoard.Api/Controllers/ImportController.cs` - Uses Backfill for bulk imports

### Migration Guide

If removing PipelineMode (not recommended):
1. Add date-range locking to prevent overlapping recalculations
2. Use Rebus `MaxParallelism = 1` to force sequential processing
3. Accept slower bulk imports
4. Monitor for deadlocks in production

## Alternatives Considered

### Alternative 1: Single Reactive Mode + Distributed Lock

**Description:** Keep only DaytimeReactive mode, but add distributed locking to prevent concurrent date-range processing.

**Pros:**
- Simpler codebase (no mode checks)
- Single code path for all operations

**Cons:**
- Bulk imports still slow (must process all 150+ messages)
- Lock contention becomes bottleneck
- Deadlock risk remains (SQL locks still occur)

**Reason for rejection:** Performance unacceptable for bulk imports

### Alternative 2: Eventual Consistency Only (Backfill-like)

**Description:** Never cascade reactively; always defer to nightly CloseOfDay batch.

**Pros:**
- Simplest implementation
- No deadlock risk
- Fastest processing

**Cons:**
- No real-time UI updates (users see stale data)
- Poor user experience for single transaction changes
- Defeats purpose of event-driven architecture

**Reason for rejection:** User experience requirement for real-time updates

### Alternative 3: CQRS with Separate Read Models

**Description:** Event-source all writes; maintain denormalized read models; rebuild read models asynchronously.

**Pros:**
- True separation of concerns
- Eventual consistency by design
- Scalable read performance

**Cons:**
- Major architectural change (months of work)
- Increased operational complexity (event store + read store)
- Eventual consistency still requires mode selection for writes

**Reason for rejection:** Too large a change for current problem; may revisit in future

### Alternative 4: Queue-Based Rate Limiting

**Description:** Use single reactive mode, but throttle message processing rate to prevent deadlocks.

**Pros:**
- Simpler than PipelineMode
- Still allows cascading

**Cons:**
- Bulk imports still slow (just rate-limited)
- Unpredictable completion time
- Doesn't address SQL lock contention

**Reason for rejection:** Doesn't solve performance problem

## References

- [PipelineMode Behavior Matrix](../architecture/pipeline-mode.md)
- [Transaction Created Event Flow](../architecture/event-flows/transaction-created-daytime.md)
- [Close of Day Saga](../architecture/sagas/close-of-day-saga.md)
- ADR-003: Date-Range Cascade Limits
- GitHub Issue: #123 (Deadlocks during bulk import) [example]
