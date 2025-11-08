namespace Hoard.Core.Messages.Valuations;

/// <summary>
/// Event raised when valuations have been calculated for a given date
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
/// <param name="AsOfDate">Date for which valuations calculated</param>
public record ValuationsCalculatedEvent(Guid CorrelationId, DateOnly AsOfDate);