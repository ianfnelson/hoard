namespace Hoard.Core.Domain.Entities;

public partial class TransactionType : Entity<int>
{
    public required string Name { get; set; }
}