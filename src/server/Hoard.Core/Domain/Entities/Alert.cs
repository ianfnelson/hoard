namespace Hoard.Core.Domain.Entities;

public class Alert : Entity<int>
{
    public int AlertTypeId { get; set; }
    public AlertType AlertType { get; set; } = null!;
    
    public DateOnly TriggeredDate { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    public int? InstrumentId { get; set; }
    public Instrument? Instrument { get; set; }
    
    public int? PortfolioId { get; set; }
    public Portfolio? Portfolio { get; set; }
    
    public required string SuppressionKey { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string? Url { get; set; }
    public string? UrlTitle { get; set; }
}