namespace Hoard.Core.Domain;

public class Account : Entity<int>
{
    public required string Name { get; set; }
    
    public int AccountTypeId { get; set; }
    public AccountType AccountType { get; set; } = null!;
    
    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();

    public bool IsActive { get; set; } = true;
}