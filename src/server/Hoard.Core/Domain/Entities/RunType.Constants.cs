namespace Hoard.Core.Domain.Entities;

public partial class RunType
{
    public const int CloseOfDay = 1;
    public const int RefreshPrices = 2;
    public const int BackfillPrices = 3;
    public const int BackfillHoldings = 4;
    public const int CalculateHoldings = 5;
    public const int CalculatePositions = 6;
    public const int BackfillValuations = 7;
    public const int CalculateValuations = 8;
    public const int CalculatePerformance = 9;
    public const int BackfillSnapshots = 10;
    public const int CalculateSnapshots = 11;
    public const int AnalyseInstruments = 12;
    public const int RefreshNews = 13;
    public const int RefreshQuotes = 14;
}
