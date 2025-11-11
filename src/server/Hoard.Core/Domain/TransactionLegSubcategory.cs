namespace Hoard.Core.Domain;

public partial class TransactionLegSubcategory : Entity<int>
{
    public int CategoryId { get; set; }
    public TransactionLegCategory Category { get; set; } = null!;
    public required string Name { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}