namespace Hoard.Core.Application.Portfolios;

public class PortfolioInstrumentTypeDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public decimal Value { get; init; }
    public decimal Percentage { get; set; }
}