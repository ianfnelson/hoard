namespace Hoard.Core;

public static class HoardUrls
{
    private const string BaseUrl = "http://hoard.iannelson.uk";

    public static string PortfolioOverview(int portfolioId) =>
        $"{BaseUrl}/portfolios/{portfolioId}/overview";

    public static string Instrument(int instrumentId) =>
        $"{BaseUrl}/instruments/{instrumentId}";
}
