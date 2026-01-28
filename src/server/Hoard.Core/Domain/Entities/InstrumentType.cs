namespace Hoard.Core.Domain.Entities;

public partial class InstrumentType : Entity<int>
{
    public required string Name { get; set; }
    public bool IsCash { get; set; }
    public bool IsFxPair { get; set; }
}