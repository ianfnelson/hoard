namespace Hoard.Core.Domain;

public class InstrumentType : Entity<int>
{
    public required string Code { get; set; }
    public required string Name { get; set; }

    public bool IsCash { get; set; }
    public bool IsExternal { get; set; }
    public bool IsFxPair { get; set; }
}