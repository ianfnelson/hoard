namespace Hoard.Core.Application.Prices;

public class PriceDetailDto
{
    public required int Id { get; init; }
    public required int InstrumentId { get; init; }
    public required string InstrumentName { get; init; }
    public required string InstrumentTicker { get; init; }
    public required DateOnly AsOfDate { get; init; }
    public required DateTime RetrievedUtc { get; init; }
    public required string Source { get; init; }
    public required bool IsLocked { get; init; }
    public decimal? Open { get; init; }
    public decimal? High { get; init; }
    public decimal? Low { get; init; }
    public required decimal Close { get; init; }
    public decimal? Volume { get; init; }
    public required decimal AdjustedClose { get; init; }
}
