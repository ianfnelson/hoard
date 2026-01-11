namespace Hoard.Core.Domain.Entities;

public partial class TransactionType
{
    public const int Buy = 100;
    public const int Sell = 200;
    public const int CorporateAction = 300;
    public const int DepositPersonal = 401;
    public const int DepositEmployer = 402;
    public const int DepositTransfer = 403;
    public const int DepositIncomeTaxReclaim = 404;
    public const int Withdrawal = 500;
    public const int Fee = 600;
    public const int IncomeInterest = 701;
    public const int IncomeLoyaltyBonus = 702;
    public const int IncomePromotion = 703;
    public const int IncomeDividend = 704;
}

internal static class TransactionTypeSets
{
    internal static readonly int[] Deposit =
    [
        TransactionType.DepositPersonal, 
        TransactionType.DepositEmployer, 
        TransactionType.DepositTransfer,
        TransactionType.DepositIncomeTaxReclaim
    ];
    
    internal static readonly int[] Income =
    [
        TransactionType.IncomeInterest, 
        TransactionType.IncomeLoyaltyBonus, 
        TransactionType.IncomePromotion, 
        TransactionType.IncomeDividend
    ];
}