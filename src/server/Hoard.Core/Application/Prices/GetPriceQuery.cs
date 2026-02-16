using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Prices;

public record GetPriceQuery(int PriceId) : IQuery<PriceDetailDto?>;

public class GetPriceQueryHandler(HoardContext context, ILogger<GetPriceQueryHandler> logger)
    : IQueryHandler<GetPriceQuery, PriceDetailDto?>
{
    public async Task<PriceDetailDto?> HandleAsync(GetPriceQuery request, CancellationToken cancellationToken)
    {
        var price = await context.Prices
            .AsNoTracking()
            .Where(p => p.Id == request.PriceId)
            .Select(p => new PriceDetailDto
            {
                Id = p.Id,
                InstrumentId = p.InstrumentId,
                InstrumentName = p.Instrument.Name,
                InstrumentTicker = p.Instrument.TickerDisplay,
                AsOfDate = p.AsOfDate,
                RetrievedUtc = p.RetrievedUtc,
                Source = p.Source,
                IsLocked = p.IsLocked,
                Open = p.Open,
                High = p.High,
                Low = p.Low,
                Close = p.Close,
                Volume = p.Volume,
                AdjustedClose = p.AdjustedClose
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (price == null)
        {
            logger.LogWarning("Price with ID {PriceId} not found", request.PriceId);
        }

        return price;
    }
}
