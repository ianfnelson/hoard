namespace Hoard.Core.Application.Transactions;

public class TransactionSummaryDto
{
    public int Id { get; set; }
    
    public int AccountId { get; set; }
    public required string AccountName { get; set; }
    
    public int? InstrumentId { get; set; }
    public required string InstrumentName { get; set; }
    public required string InstrumentTicker { get; set; }
    
    public int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    
    public DateOnly Date { get; set; }
    public required string ContractNoteReference { get; set; }
    
    public decimal? Units { get; set; }
    public decimal Value { get; set; }
}