namespace Hoard.Core.Domain.Entities;

public partial class InstrumentType : Entity<int>
{
    public required string Name { get; set; }

    public required string Colour { get; set; }
}