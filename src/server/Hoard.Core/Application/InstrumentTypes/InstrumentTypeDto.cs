namespace Hoard.Core.Application.InstrumentTypes;

public class InstrumentTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsCash { get; set; }
    public bool IsFxPair { get; set; }
}