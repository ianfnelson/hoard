namespace Hoard.Core.Domain;

public class TransactionLeg : Entity<int>
{
    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;
    
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public int CategoryId { get; set; }
    public TransactionLegCategory Category { get; set; } = null!;
    
    public int? SubcategoryId { get; set; }
    public TransactionLegSubcategory Subcategory { get; set; } = null!;
    
    public decimal ValueGbp { get; set; }
    public decimal Units { get; set; }
    public string? Notes { get; set; }
}