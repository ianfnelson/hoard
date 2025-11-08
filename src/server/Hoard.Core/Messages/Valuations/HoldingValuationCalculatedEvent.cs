namespace Hoard.Core.Messages.Valuations;

/// <summary>
/// Event raised when a holding has been valued for a given date
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
/// <param name="HoldingId">ID of holding that has been valued</param>
/// <param name="AsOfDate">AsOfDate of valued holding</param>
public record HoldingValuationCalculatedEvent(Guid CorrelationId, int HoldingId, DateOnly AsOfDate);