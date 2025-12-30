namespace Hoard.Core.Application.Portfolios;

public class PortfolioDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    public PortfolioPerformanceDto? Performance { get; set; }
}