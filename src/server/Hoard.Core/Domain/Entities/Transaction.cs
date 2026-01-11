namespace Hoard.Core.Domain.Entities;

public class Transaction : Entity<int>
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public int? InstrumentId { get; set; }
    public Instrument? Instrument { get; set; }
    
    public int TransactionTypeId { get; set; }
    public TransactionType TransactionType { get; set; } = null!;
    
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    
    public string? ContractNoteReference { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    public decimal? Units { get; set; }
    public decimal Value { get; set; }
    
    public decimal? DealingCharge { get; set; }
    public decimal? StampDuty { get; set; }
    public decimal? PtmLevy { get; set; }
    public decimal? FxCharge { get; set; }
}