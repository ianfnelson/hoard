namespace Hoard.Core.Extensions;

public static class DateRangeExtensions
{
    public static IEnumerable<DateRange> ChunkByDays(this DateRange range, int maxDays)
    {
        if (maxDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDays), "maxDays must be positive.");

        var start = range.StartDate;
        var end = range.EndDate;

        while (start <= end)
        {
            var chunkEnd = start.AddDays(maxDays - 1);
            if (chunkEnd > end)
                chunkEnd = end;

            yield return new DateRange(start, chunkEnd);

            start = chunkEnd.AddDays(1);
        }
    }
}