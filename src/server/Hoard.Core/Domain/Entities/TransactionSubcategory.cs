namespace Hoard.Core.Domain.Entities;

public partial class TransactionSubcategory : Entity<int>
{
    public int TransactionCategoryId { get; set; }
    public TransactionCategory TransactionCategory { get; set; } = null!;
    
    public required string Name { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}