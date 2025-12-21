namespace Hoard.Core.Application.Portfolios;

public class PortfolioSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
}