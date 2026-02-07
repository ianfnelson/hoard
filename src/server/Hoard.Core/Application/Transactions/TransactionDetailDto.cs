namespace Hoard.Core.Application.Transactions;

public class TransactionDetailDto
{
    public int Id { get; set; }
    
    public int AccountId { get; set; }
    public required string AccountName { get; set; }
    
    public int? InstrumentId { get; set; }
    public string? InstrumentName { get; set; }
    public string? InstrumentTicker { get; set; }
    
    public int TransactionTypeId { get; set; }
    public required string TransactionTypeName { get; set; }
    
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public string? ContractNoteReference { get; set; }
    
    public decimal? Units { get; set; }
    public decimal Value { get; set; }
    public decimal? Price { get; set; }
    
    public decimal? DealingCharge { get; set; }
    public decimal? StampDuty { get; set; }
    public decimal? PtmLevy { get; set; }
    public decimal? FxCharge { get; set; }
}