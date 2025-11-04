namespace Hoard.Core.Messages.Valuations;

/// <summary>
/// Calculate valuations of portfolios at a given date
/// </summary>
/// <param name="AsOfDate">Date on which to calculate valuation</param>
/// <param name="PortfolioIds">Optional list of portfolio IDs to be valued</param>
public record CalculatePortfolioValuationsCommand(DateOnly AsOfDate);