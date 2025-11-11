namespace Hoard.Core.Domain;

public partial class TransactionLegCategory : Entity<int>
{
    public required string Name { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}