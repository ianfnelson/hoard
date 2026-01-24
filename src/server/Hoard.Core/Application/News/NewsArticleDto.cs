namespace Hoard.Core.Application.News;

public class NewsArticleDto
{
    public int Id { get; set; }
    
    public DateTime PublishedUtc { get; set; }
    public DateTime RetrievedUtc { get; set; }
    
    public required string Headline { get; set; }
    public required string BodyHtml { get; set; }
    public required string Url { get; set; }
    
    public int InstrumentId { get; set; }
    public required string InstrumentName { get; set; }
    public required string InstrumentTicker { get; set; }
}