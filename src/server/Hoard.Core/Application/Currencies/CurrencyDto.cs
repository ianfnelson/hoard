namespace Hoard.Core.Application.Currencies;

public class CurrencyDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedUtc { get; set; }
}