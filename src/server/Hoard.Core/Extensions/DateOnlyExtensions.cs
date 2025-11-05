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
}