using Hoard.Core.Services;

namespace Hoard.Core.Domain.Entities;

public class Price : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public DateOnly AsOfDate { get; set; }
    public DateTime RetrievedUtc { get; set; }
    public required string Source { get; set; }
    public bool IsLocked { get; set; }
    
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal Close { get; set; }
    public long? Volume { get; set; }
    public decimal AdjustedClose { get; set; }

    public void UpdateFrom(PriceDto dto)
    {
        AdjustedClose = dto.AdjustedClose;
        Close = dto.Close;
        High = dto.High;
        Low = dto.Low;
        Open = dto.Open;
        Volume = dto.Volume;
    }
}