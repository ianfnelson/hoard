using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Instruments;

public record GetInstrumentQuery(int InstrumentId) : IQuery<InstrumentDetailDto?>;

public sealed class GetInstrumentHandler(HoardContext context, ILogger<GetInstrumentHandler> logger)
    : IQueryHandler<GetInstrumentQuery, InstrumentDetailDto?>
{
    public async Task<InstrumentDetailDto?> HandleAsync(GetInstrumentQuery query, CancellationToken ct = default)
    {
        var dto = await context.Instruments
            .AsNoTracking()
            .Where(i => i.Id == query.InstrumentId)
            .Select(i => new InstrumentDetailDto
            {
                Id = i.Id,
                Name = i.Name,
                TickerDisplay = i.TickerDisplay,
                Isin = i.Isin,
                CreatedUtc = i.CreatedUtc,
                InstrumentTypeId = i.InstrumentTypeId,
                InstrumentTypeName = i.InstrumentType.Name,
                AssetClassId = i.AssetSubclass.AssetClassId,
                AssetClassName = i.AssetSubclass.AssetClass.Name,
                AssetSubclassId = i.AssetSubclassId,
                AssetSubclassName = i.AssetSubclass.Name,
                CurrencyId = i.CurrencyId,
                CurrencyName = i.Currency.Name,
                Quote = i.Quote == null ? null : new InstrumentQuoteDto
                {
                    RetrievedUtc = i.Quote.RetrievedUtc,
                    Bid = i.Quote.Bid,
                    Ask = i.Quote.Ask,
                    FiftyTwoWeekHigh = i.Quote.FiftyTwoWeekHigh,
                    FiftyTwoWeekLow = i.Quote.FiftyTwoWeekLow,
                    RegularMarketPrice = i.Quote.RegularMarketPrice,
                    RegularMarketChange = i.Quote.RegularMarketChange,
                    RegularMarketChangePercent = i.Quote.RegularMarketChangePercent
                }
            }).SingleOrDefaultAsync(ct);
        
        if (dto == null)
        {
            logger.LogWarning(
                "Instrument with id {InstrumentId} not found",
                query.InstrumentId);
        }
        
        return dto;
    }
}