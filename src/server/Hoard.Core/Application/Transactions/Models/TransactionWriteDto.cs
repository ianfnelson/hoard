namespace Hoard.Core.Application.Transactions.Models;

public class TransactionWriteDto
{
    public int? AccountId { get; set; }
    public int? CategoryId { get; set; }
    public int? NonCashSubcategoryId { get; set; }
    public int? InstrumentId { get; set; }
    public DateOnly? Date { get; set; }
    public string? Notes { get; set; }
    public decimal? Units { get; set; }
    
    public string? ContractNoteReference { get; set; }
    
    public decimal? DealingChargeGbp { get; set; }
    public decimal? FxChargeGbp { get; set; }
    public decimal? StampDutyGbp { get; set; }
    public decimal? PtmLevyGbp { get; set; }
    
    public decimal? ValueGbp { get; set; }
}