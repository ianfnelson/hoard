namespace Hoard.Core.Domain;

public class TransactionLegType : Entity<int>
{
    public required string Code { get; set; }
    public required string Name { get; set; }
}