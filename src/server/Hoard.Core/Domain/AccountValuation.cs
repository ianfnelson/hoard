namespace Hoard.Core.Domain;

public class AccountValuation : Valuation
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
}