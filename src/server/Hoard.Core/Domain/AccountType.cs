namespace Hoard.Core.Domain;

public class AccountType : Entity<int>
{
    public required string Name { get; set; }
}