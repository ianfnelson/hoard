namespace Hoard.Messages.Prices;

public record StartBackfillPricesSagaCommand(
    Guid CorrelationId,
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate);
