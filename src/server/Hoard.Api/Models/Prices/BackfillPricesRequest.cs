using Hoard.Core.Messages.Prices;

namespace Hoard.Api.Models.Prices;

public class BackfillPricesRequest
{
    public int? InstrumentId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public BackfillPricesCommand ToCommand()
    {
        return new BackfillPricesCommand(Guid.NewGuid())
        {
            InstrumentId = InstrumentId,
            StartDate = StartDate,
            EndDate = EndDate
        };
    }
}