namespace Hoard.Core.Domain.Entities;

public partial class InstrumentType : Entity<int>
{
    public required string Name { get; set; }
}