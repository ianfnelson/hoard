using Hoard.Core.Infrastructure.Integrations;

namespace Hoard.Core.Application.Portfolios;

public record ExportPortfolioValuationsPdfQuery(int PortfolioId, DateOnly? FromDate, DateOnly? ToDate)
    : IQuery<PortfolioValuationsPdfResult?>;

public record PortfolioValuationsPdfResult(Stream Stream, string ContentType, string FileName);

public class ExportPortfolioValuationsPdfHandler(
    IMediator mediator,
    IPortfolioValuationsPdfService pdfService)
    : IQueryHandler<ExportPortfolioValuationsPdfQuery, PortfolioValuationsPdfResult?>
{
    public async Task<PortfolioValuationsPdfResult?> HandleAsync(
        ExportPortfolioValuationsPdfQuery query,
        CancellationToken ct = default)
    {
        var portfolio = await mediator
            .QueryAsync<GetPortfolioQuery, PortfolioDetailDto?>(new GetPortfolioQuery(query.PortfolioId), ct);

        if (portfolio == null)
            return null;

        var valuations = await mediator
            .QueryAsync<GetPortfolioValuationsQuery, PortfolioValuationsDto?>(
                new GetPortfolioValuationsQuery(query.PortfolioId, query.FromDate, query.ToDate), ct);

        if (valuations == null)
            return null;

        var stream = pdfService.Generate(portfolio.Name, valuations);

        var fileName = valuations.FromDate.HasValue
            ? $"{portfolio.Name} {valuations.FromDate.Value:yyyy-MM-dd} to {valuations.ToDate:yyyy-MM-dd}.pdf"
            : $"{portfolio.Name} {valuations.ToDate:yyyy-MM-dd}.pdf";

        return new PortfolioValuationsPdfResult(stream, "application/pdf", fileName);
    }
}
