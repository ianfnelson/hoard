using Hoard.Messages.Prices;

namespace Hoard.Api.Models.Prices;

public class BackfillPricesRequest
{
    public int? InstrumentId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public StartBackfillPricesSagaCommand ToCommand()
    {
        return new StartBackfillPricesSagaCommand(Guid.NewGuid())
        {
            InstrumentId = InstrumentId,
            StartDate = StartDate,
            EndDate = EndDate
        };
    }
}