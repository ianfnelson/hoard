namespace Hoard.Core.Extensions;

internal static class DecimalExtensions
{
    internal static decimal? RoundTo2Dp(this decimal? value)
    {
        return value?.RoundTo2Dp();
    }
    
    internal static decimal RoundTo2Dp(this decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}