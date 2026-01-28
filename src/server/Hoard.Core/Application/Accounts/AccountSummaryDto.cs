namespace Hoard.Core.Application.Accounts;

public class AccountSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
}