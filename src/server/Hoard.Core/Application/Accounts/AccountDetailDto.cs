using Hoard.Core.Application.Portfolios;

namespace Hoard.Core.Application.Accounts;

public class AccountDetailDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    public required List<PortfolioSummaryDto> Portfolios { get; set; }
}