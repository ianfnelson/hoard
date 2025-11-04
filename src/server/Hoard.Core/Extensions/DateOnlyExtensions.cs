namespace Hoard.Core.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly OrTodayIfNull(this DateOnly? value)
    {
        return value ?? DateOnlyHelper.TodayLocal();
    }
    
    public static string ToIsoDateString(this DateOnly date)
        => date.ToString("yyyy-MM-dd");
}