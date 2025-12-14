namespace Hoard.Core.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly OrToday(this DateOnly dateOnly)
    {
        return dateOnly == default ? DateOnlyHelper.TodayLocal() : dateOnly;
    }
    
    public static DateOnly OrToday(this DateOnly? value)
    {
        return (value ?? default).OrToday();
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