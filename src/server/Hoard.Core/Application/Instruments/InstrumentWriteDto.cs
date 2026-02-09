namespace Hoard.Core.Application.Instruments;

public class InstrumentWriteDto
{
    public string? Name { get; set; }
    public int? InstrumentTypeId { get; set; }
    public int? AssetSubclassId { get; set; }
    public string? CurrencyId { get; set; }
    public string? TickerDisplay { get; set; }
    public string? Isin { get; set; }
}