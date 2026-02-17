namespace Hoard.Core.Application.Alerts;

public sealed record AlertRequest
{
    public required int AlertTypeId { get; init; }
    public required DateOnly TriggeredDate { get; init; }
    public int? InstrumentId { get; init; }
    public int? PortfolioId { get; init; }
    public required string SuppressionKey { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? Url { get; init; }
    public string? UrlTitle { get; init; }
}