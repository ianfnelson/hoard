namespace Hoard.Core.Domain;

public class TransactionCategory : Entity<int>
{
    public required string Name { get; set; }
}