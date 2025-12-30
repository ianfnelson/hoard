namespace Hoard.Core.Application.Portfolios;

public sealed class PortfolioInstrumentTypesDto
{
    public int PortfolioId { get; init; }
    public DateOnly AsOfDate { get; init; }

    public decimal TotalValue { get; init; }
    
    public IReadOnlyList<PortfolioInstrumentTypeDto> InstrumentTypes { get; init; } = [];
}