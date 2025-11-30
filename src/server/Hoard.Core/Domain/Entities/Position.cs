namespace Hoard.Core.Domain.Entities;

public class Position : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    
    public PositionPerformanceCumulative? Performance { get; set; }
    
    public DateOnly OpenDate { get; set; }
    public DateOnly? CloseDate { get; set; }
    
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}