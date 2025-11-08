namespace Hoard.Messages.Holdings;

/// <summary>
/// Event raised when holdings have been calculated.
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
/// <param name="AsOfDate">Date for which holdings have been calculated.</param>
public record HoldingsCalculatedEvent(Guid CorrelationId, DateOnly AsOfDate);