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
    public const int IncomeDividend = 704;
    public const int Promotion = 800;
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
        TransactionType.IncomeDividend
    ];

    internal static readonly int[] NegativeUnits =
    [
        TransactionType.Sell
    ];
    
    internal static readonly int[] NegativeValue =
    [
        TransactionType.Buy,
        TransactionType.Withdrawal,
        TransactionType.Fee
    ];

    internal static readonly int[] PositionCashflows =
    [
        TransactionType.Buy,
        TransactionType.Sell,
        TransactionType.CorporateAction,
        TransactionType.IncomeDividend,
        TransactionType.IncomeLoyaltyBonus
    ];
    
    internal static readonly int[] PortfolioCashflows =
    [
        ..Deposit,
        TransactionType.Withdrawal
    ];
}