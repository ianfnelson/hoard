namespace Hoard.Core.Domain;

public class TransactionLegSubcategory : Entity<int>
{
    public int CategoryId { get; set; }
    public TransactionLegCategory Category { get; set; } = null!;
    public required string Name { get; set; }
}