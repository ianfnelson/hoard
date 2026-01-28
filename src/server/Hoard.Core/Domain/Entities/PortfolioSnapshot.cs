namespace Hoard.Core.Domain.Entities;

public class PortfolioSnapshot : Entity<int>
{
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    
    public int Year { get; set; }
    
    public decimal StartValue { get; set; }
    public decimal EndValue { get; set; }
    public decimal ValueChange { get; set; }
    public decimal AverageValue { get; set; }
    
    public decimal Return { get; set; }
    public decimal Churn { get; set; }
    public decimal Yield { get; set; }
    
    public decimal TotalBuys { get; set; }
    public decimal TotalSells { get; set; }
    public decimal TotalIncomeInterest { get; set; }
    public decimal TotalIncomeLoyaltyBonus { get; set; }
    public decimal TotalIncomeDividends { get; set; }
    public decimal TotalPromotion { get; set; }
    public decimal TotalFees { get; set; }
    public decimal TotalDealingCharge { get; set; }
    public decimal TotalStampDuty { get; set; }
    public decimal TotalPtmLevy { get; set; }
    public decimal TotalFxCharge { get; set; }
    public decimal TotalDepositPersonal { get; set; }
    public decimal TotalDepositEmployer { get; set; }
    public decimal TotalDepositIncomeTaxReclaim { get; set; }
    public decimal TotalDepositTransferIn { get; set; }
    public decimal TotalWithdrawals { get; set; }
    
    public int CountTrades { get; set; }
    
    public DateTime UpdatedUtc { get; set; }
}