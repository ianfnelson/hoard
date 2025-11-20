namespace Hoard.Core.Domain;

public class Transaction : Entity<int>
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public int? InstrumentId { get; set; }
    public Instrument? Instrument { get; set; }
    
    public int CategoryId { get; set; }
    public TransactionCategory Category { get; set; } = null!;
    
    public int? SubcategoryId { get; set; }
    public TransactionSubcategory? Subcategory { get; set; }
    
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
    
    public Cashflow? ToCashflow()
    {
        return CategoryId switch
        {
            TransactionCategory.Buy           => Create(),
            TransactionCategory.Sell          => Create(),
            TransactionCategory.Income        => InstrumentId == null ? null : Create(),
            TransactionCategory.Deposit       => Create(),
            TransactionCategory.Withdrawal    => Create(),
            TransactionCategory.CorporateAction 
                => Value != 0 ? Create() : null,
            _                                 => null
        };

        Cashflow Create() => new()
        {
            AccountId     = AccountId,
            TransactionId = Id,
            InstrumentId  = InstrumentId,
            Date          = Date,
            Value         = Value
        };
    }
}