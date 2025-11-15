namespace Hoard.Core.Domain;

public partial class InstrumentType : Entity<int>
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.Now;

    public bool IsCash { get; set; }
    public bool IsFxPair { get; set; }
}