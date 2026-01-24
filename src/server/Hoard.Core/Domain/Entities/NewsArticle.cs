namespace Hoard.Core.Domain.Entities;

public class NewsArticle : Entity<int>
{
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    
    public DateTime PublishedUtc { get; set; }
    public DateTime RetrievedUtc { get; set; }
    public required string Source { get; set; }
    public required string SourceArticleId { get; set; }
    public required string Headline { get; set; }
    public required string BodyHtml { get; set; }
    public required string BodyText { get; set; }
    public required string Url { get; set; }
}