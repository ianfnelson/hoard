namespace Hoard.Core;

public static class DateOnlyHelper
{
    public static readonly TimeZoneInfo UkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    public static DateTime NowUk(TimeProvider timeProvider) =>
        TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, UkTimeZone);

    public static DateOnly TodayLocal(TimeProvider timeProvider) => DateOnly.FromDateTime(NowUk(timeProvider));

    public static DateOnly EpochLocal() => new(2000, 1, 1);
}
