using Hoard.Core.Data;
using Hoard.Core.Domain;

namespace Hoard.Core.Extensions;

public static class HoardContextExtensions
{
    public static IQueryable<Instrument> GetRefreshableInstrumentsAsOf(this HoardContext context, DateOnly date)
    {
        var holdings = context.Holdings
            .Where(h => h.AsOfDate == date)
            .Select(x => x.InstrumentId)
            .ToHashSet();

        return context.Instruments
            .Where(x => x.InstrumentType.IsFxPair || holdings.Contains(x.Id))
            .Where(x => x.EnablePriceUpdates)
            .Where(x => x.TickerApi != null);
    }

    public static IQueryable<Instrument> GetRefreshableInstruments(this HoardContext context)
    {
        return context.GetRefreshableInstrumentsAsOf(DateOnlyHelper.TodayLocal());
    }
}