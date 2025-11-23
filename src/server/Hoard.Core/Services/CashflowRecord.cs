namespace Hoard.Core.Services;

public sealed record CashflowRecord(DateOnly Date, decimal Amount, decimal? Units, int CategoryId);