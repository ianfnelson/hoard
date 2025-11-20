namespace Hoard.Core.Domain;

public class Cashflow : Entity<int>
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public int? InstrumentId { get; set; }
    public Instrument? Instrument { get; set; }
    
    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;
    
    public DateOnly Date { get; set; }
    
    public decimal Value { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}