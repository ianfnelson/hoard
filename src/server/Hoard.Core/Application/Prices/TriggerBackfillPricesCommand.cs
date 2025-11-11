using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public record TriggerBackfillPricesCommand(
    Guid CorrelationId,
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate) : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillPricesSagaCommand(CorrelationId, InstrumentId, StartDate, EndDate);
}