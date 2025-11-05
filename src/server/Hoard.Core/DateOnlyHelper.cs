namespace Hoard.Core;

public static class DateOnlyHelper
{
    public static DateOnly TodayLocal() => DateOnly.FromDateTime(DateTime.Now);
    public static DateOnly OneYearAgoLocal() => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));

    public static DateOnly EpochLocal() => new(2000, 1, 1);
}
