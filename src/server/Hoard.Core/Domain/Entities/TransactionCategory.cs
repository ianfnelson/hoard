namespace Hoard.Core.Domain.Entities;

public partial class TransactionCategory : Entity<int>
{
    public required string Name { get; set; }
}