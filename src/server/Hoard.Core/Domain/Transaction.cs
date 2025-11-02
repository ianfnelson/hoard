namespace Hoard.Core.Domain;

public class Transaction : Entity<int>
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public int CategoryId { get; set; }
    public TransactionCategory Category { get; set; } = null!;
    
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    public ICollection<TransactionLeg> Legs { get; set; } = new List<TransactionLeg>();
}