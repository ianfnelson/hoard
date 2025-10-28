namespace Hoard.Core.Domain;

public class TransactionType : Entity<int>
{
    public required string Code { get; set; }
    public required string Name { get; set; }
}