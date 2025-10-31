namespace Hoard.Core.Domain;

public class TransactionLeg : Entity<int>
{
    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;
    
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public int TransactionLegTypeId { get; set; }
    public TransactionLegCategory TransactionLegCategory { get; set; } = null!;
    
    public decimal Value { get; set; }
    public decimal Units { get; set; }
    public string? Notes { get; set; }
}