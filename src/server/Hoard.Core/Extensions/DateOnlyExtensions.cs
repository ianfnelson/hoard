namespace Hoard.Core.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly OrToday(this DateOnly dateOnly, TimeProvider timeProvider)
    {
        return dateOnly == default ? DateOnlyHelper.TodayLocal(timeProvider) : dateOnly;
    }

    public static DateOnly OrToday(this DateOnly? value, TimeProvider timeProvider)
    {
        return (value ?? default).OrToday(timeProvider);
    }

    public static bool IsWeekday(this DateOnly date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    public static string ToIsoDateString(this DateOnly date)
        => date.ToString("yyyy-MM-dd");
        
    public static DateOnly PreviousTradingDay(this DateOnly date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Saturday => date.AddDays(-2),
            DayOfWeek.Sunday => date.AddDays(-3),
            _ => date.AddDays(-1)
        };
    }
}