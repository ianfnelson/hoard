namespace Hoard.Core.Application.Alerts;

public class AlertSummaryDto
{
    public int Id { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateOnly TriggeredDate { get; set; }

    public int AlertTypeId { get; set; }
    public required string AlertTypeName { get; set; }

    public int? InstrumentId { get; set; }
    public string? InstrumentName { get; set; }

    public int? PortfolioId { get; set; }
    public string? PortfolioName { get; set; }

    public required string Title { get; set; }
}
