namespace Hoard.Core;

public static class DateOnlyHelper
{
    public static DateOnly TodayLocal() => DateOnly.FromDateTime(DateTime.Now);
    public static DateOnly TodayUtc()   => DateOnly.FromDateTime(DateTime.UtcNow);
}