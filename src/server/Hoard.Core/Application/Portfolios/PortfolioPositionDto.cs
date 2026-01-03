namespace Hoard.Core.Application.Portfolios;

public class PortfolioPositionDto
{
    public int InstrumentId { get; set; }
    public required string InstrumentName { get; set; }
    public required string InstrumentTicker { get; set; }
    
    public DateOnly OpenDate { get; set; }
    public DateOnly? CloseDate { get; set; }
    
    public PositionPerformanceDto? Performance { get; set; }
    
    public decimal PortfolioPercentage { get; set; }
}