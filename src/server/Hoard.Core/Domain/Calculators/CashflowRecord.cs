namespace Hoard.Core.Domain.Calculators;

public sealed record CashflowRecord(DateOnly Date, decimal Amount, decimal? Units, int CategoryId);