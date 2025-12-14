namespace Hoard.Core.Application.Portfolios.Models;

public class PortfolioDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
}