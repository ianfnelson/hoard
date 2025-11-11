namespace Hoard.Core.Domain;

public partial class TransactionCategory
{
    public const int Buy = 1;
    public const int Sell = 2;
    public const int Dividend = 3;
    public const int Interest = 4;
    public const int Fee = 5;
    public const int Deposit = 6;
    public const int Withdrawal = 7;
}