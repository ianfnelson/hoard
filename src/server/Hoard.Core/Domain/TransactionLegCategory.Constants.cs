namespace Hoard.Core.Domain;

public partial class TransactionLegCategory
{
    public const int Cash = 1;
    public const int Principal = 2;
    public const int Income = 3;
    public const int Fee = 4;
    public const int Tax = 5;
    public const int External = 6;
}