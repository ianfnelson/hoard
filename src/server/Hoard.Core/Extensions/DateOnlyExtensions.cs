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

    public static DateOnly OrOneYearAgo(this DateOnly dateOnly)
    {
        var oneYearAgo = DateOnlyHelper.OneYearAgoLocal();
        
        return dateOnly == default || dateOnly > oneYearAgo ? DateOnlyHelper.OneYearAgoLocal() : dateOnly;
    }
    
    public static DateOnly OrOneYearAgo(this DateOnly? value)
    {
        return (value ?? default).OrOneYearAgo();
    }
    
    public static string ToIsoDateString(this DateOnly date)
        => date.ToString("yyyy-MM-dd");
}