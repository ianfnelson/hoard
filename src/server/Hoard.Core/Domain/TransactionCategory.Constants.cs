namespace Hoard.Core.Domain;

public partial class TransactionCategory
{
    public const int Buy = 1;
    public const int Sell = 2;
    public const int Income = 3;
    public const int Fee = 4;
    public const int Deposit = 5;
    public const int Withdrawal = 6;
    public const int CorporateAction = 7;
}