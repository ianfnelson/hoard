namespace Hoard.Core.Messages.Prices;

/// <summary>
/// Refresh daily prices for active instruments and currencies.
/// This will be scheduled to run after the close of the trading day.
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
public record RefreshPricesCommand(Guid CorrelationId)
{
    /// <summary>
    /// Date for which to refresh daily prices.
    /// If not specified, will default to today.
    /// </summary>
    public DateOnly? AsOfDate { get; init; }
}