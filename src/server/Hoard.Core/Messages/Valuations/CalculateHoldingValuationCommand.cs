namespace Hoard.Core.Messages.Valuations;

/// <summary>
/// Calculate valuations of holdings
/// </summary>
/// <param name="CorrelationId">For correlating messages</param>
/// <param name="HoldingId">ID of holding to be valued</param>
public record CalculateHoldingValuationCommand(Guid CorrelationId, int HoldingId);