namespace Hoard.Core.Extensions;

internal static class DecimalExtensions
{
    internal static decimal RoundForDisplay(this decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}