namespace Hoard.Core.Domain.Entities;

public partial class AlertType
{
    public const int CloseOfDayPortfolioValuation = 1;
    public const int NewsArticle = 2;
    public const int FiftyTwoWeekHigh = 3;
    public const int FiftyTwoWeekLow = 4;
    public const int AbnormalVolume = 5;
    public const int QuoteMovement = 6;
    public const int PortfolioAllTimeHigh = 7;
    public const int PortfolioDrawdown10 = 8;
    public const int PortfolioDrawdown20 = 9;
    public const int PortfolioDrawdown30 = 10;
    public const int PortfolioDrawdown50 = 11;
}