# 003. Date-Range Cascade Limits

Date: 2026-02-09

## Status

Proposed

## Context

When a transaction is backdated in DaytimeReactive mode, it triggers recalculation of all metrics from the backdated date forward to today. This creates a message cascade proportional to the number of days:

**Cascade formula:**
```
Messages = Days × Cascade_Depth
         = Days × 5 (Holdings → Valuations → Positions → Performance → Snapshots)
```

**Observed behavior:**
- 1 day: ~6 messages, < 1 second ✅ Acceptable
- 7 days: ~35 messages, 5-10 seconds ✅ Acceptable
- 30 days: ~150 messages, 30-60 seconds ⚠️ Slow, deadlock risk
- 90 days: ~450 messages, 2-5 minutes ❌ Unacceptable, high deadlock risk
- 365 days: ~1,825 messages, 10-20 minutes ❌ System degradation

**Problems observed in production:**
1. Users backdating transactions 30+ days cause UI timeouts
2. Concurrent backdated transactions create SQL deadlocks
3. Message queue saturation during bulk edits
4. Database CPU spikes to 100% during long cascades

**Requirements:**
1. Preserve real-time responsiveness for recent changes (1-7 days)
2. Prevent system degradation from deep backdating
3. Maintain data accuracy (all metrics must eventually be correct)
4. Provide clear user feedback when limits are reached

## Decision

Implement **graduated limits based on PipelineMode:**

### Limit 1: DaytimeReactive Mode - 7 Day Maximum

**Rule:** Backdated transactions in DaytimeReactive mode are limited to 7 days from today.

**Enforcement:**
- API validation: Reject transactions dated > 7 days ago with HTTP 400
- Error message: "Transactions older than 7 days require bulk import mode. Please use the import feature or contact support."

**Rationale:**
- 7 days = ~35 messages (acceptable performance)
- Covers typical user corrections (recent mistakes)
- Prevents deadlock scenarios
- Forces bulk operations to use Backfill mode

### Limit 2: Backfill Mode - No Limit (Unlimited)

**Rule:** Backfill mode has no date-range limit.

**Rationale:**
- Backfill doesn't cascade (only 5 messages regardless of date range)
- Designed for bulk historical data
- No deadlock risk

### Limit 3: CloseOfDay Mode - Single Date Only

**Rule:** CloseOfDay mode processes exactly one date (previous business day).

**Rationale:**
- Nightly batch reconciliation for single day
- Saga orchestration ensures consistency
- No user-facing limit (automated process)

### Implementation Details

```csharp
// API layer validation
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Date)
            .Must((command, date) => BeWithinAllowedRange(command.PipelineMode, date))
            .WithMessage("Transactions older than 7 days require bulk import mode");
    }

    private bool BeWithinAllowedRange(PipelineMode mode, DateOnly date)
    {
        if (mode != PipelineMode.DaytimeReactive)
            return true; // No limit for Backfill or CloseOfDay

        var daysAgo = (DateOnly.FromDateTime(DateTime.Today) - date).Days;
        return daysAgo <= 7;
    }
}
```

### User-Facing Changes

**UI warning (soft limit at 3 days):**
```
"This transaction is dated 5 days ago. Recalculation may take 10-15 seconds. Continue?"
```

**UI error (hard limit at 7 days):**
```
"Transactions older than 7 days cannot be created in real-time mode.
Please use the bulk import feature instead."
[Import File] [Cancel]
```

## Consequences

### Positive

1. **Prevents system degradation:** No more 90-day cascades causing timeouts
2. **Eliminates most deadlocks:** 7-day limit greatly reduces SQL lock contention
3. **Preserves real-time UX:** Recent changes (< 7 days) still work normally
4. **Clear user guidance:** Error messages explain alternative (bulk import)
5. **Forces correct mode:** Users importing historical data must use Backfill

### Negative

1. **User friction:** Power users may be frustrated by 7-day limit
2. **Support burden:** Users may contact support to bypass limit
3. **Migration complexity:** Existing backdated transactions may violate new rules
4. **Edge case handling:** What if user needs to correct 8-day-old transaction?

### Neutral

1. **Documentation requirement:** Must clearly document limit and alternatives
2. **Monitoring needed:** Track how often users hit the limit (adjust if needed)

## Implementation Notes

### Phase 1: Soft Launch (Warnings Only)

1. Add validation rule (warn but allow)
2. Log all transactions > 7 days old
3. Monitor frequency and user feedback
4. Gather baseline metrics

### Phase 2: Hard Enforcement

1. Convert warning to error (reject transactions > 7 days)
2. Update API documentation
3. Update UI with error message and import link
4. Monitor support requests

### Phase 3: Optimization (Optional)

1. Add "batch edit" mode for 8-30 day range (queued processing)
2. Implement progressive disclosure: "This will take ~30 seconds. Queue for background processing?"

### File Locations

**Validation:**
- `/Hoard.Api/Validators/CreateTransactionCommandValidator.cs` (new file)
- `/Hoard.Api/Controllers/TransactionsController.cs` (add validator)

**UI Changes:**
- `/src/client/hoard-ui/src/pages/TransactionEdit.vue` (add date range check)
- `/src/client/hoard-ui/src/composables/useTransactionValidation.ts` (new composable)

**Metrics:**
- `/Hoard.Core/Application/Transactions/Commands/CreateTransactionCommandHandler.cs` (add telemetry)

### Monitoring Queries

```kusto
// Application Insights: Track deep cascades
customEvents
| where name == "TransactionCreated"
| extend DaysBackdated = todouble(customDimensions.DaysBackdated)
| where DaysBackdated > 7
| summarize Count=count() by bin(DaysBackdated, 1)
| render barchart
```

### Exception Cases (Support Override)

For legitimate cases requiring > 7 day backdate:
1. Support grants temporary "admin" permission
2. Admin users bypass 7-day check
3. Event logged for audit trail

## Alternatives Considered

### Alternative 1: No Limit (Status Quo)

**Description:** Allow unlimited backdating in DaytimeReactive mode.

**Pros:**
- No user friction
- Maximum flexibility

**Cons:**
- System degradation continues
- Deadlocks persist
- Poor user experience (timeouts)

**Reason for rejection:** Unacceptable performance impact

### Alternative 2: Stricter Limit (1-3 Days)

**Description:** Limit DaytimeReactive to 1-3 days instead of 7.

**Pros:**
- Even better performance
- Lower deadlock risk

**Cons:**
- Higher user friction (weekly reconciliations become difficult)
- More support requests
- May force legitimate use cases to bulk import

**Reason for rejection:** 7 days is a better balance (covers most corrections)

### Alternative 3: Progressive Throttling

**Description:** Allow unlimited backdating, but throttle cascade processing (e.g., max 1 day per second).

**Pros:**
- No hard limit
- Prevents queue saturation

**Cons:**
- Unpredictable completion time (user doesn't know when done)
- Still risks deadlocks (just slower)
- Doesn't address SQL lock contention

**Reason for rejection:** Doesn't solve core problems

### Alternative 4: Automatic Mode Selection

**Description:** API automatically switches to Backfill if date > 7 days.

**Pros:**
- No user friction (transparent)
- Optimal mode selection

**Cons:**
- User doesn't see real-time updates for old transactions (confusing)
- No explicit feedback about mode switch
- May hide performance issues

**Reason for rejection:** Implicit behavior is confusing; explicit limit is clearer

### Alternative 5: Queue-Based Deferred Processing

**Description:** Accept all backdated transactions, but queue > 7 days for background processing.

**Pros:**
- No rejection (better UX)
- Asynchronous processing (no timeouts)

**Cons:**
- Requires new queueing infrastructure
- Unclear completion time
- Still needs eventual limit (can't queue infinite work)

**Reason for rejection:** Adds complexity; doesn't eliminate need for limit

## References

- [Transaction Created Event Flow](../architecture/event-flows/transaction-created-daytime.md)
- [PipelineMode Behavior Matrix](../architecture/pipeline-mode.md)
- ADR-001: PipelineMode Strategy
- GitHub Issue: #789 (Deadlocks from backdated transactions) [example]
