namespace Hoard.Core.Domain.Entities;

public class AccountType : Entity<int>
{
    public required string Name { get; set; }
}