using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioPerformanceQuery(int PortfolioId) : IQuery<PortfolioPerformanceDto?>;

public class GetPortfolioPerformanceHandler(HoardContext context, ILogger<GetPortfolioPerformanceHandler> logger)
    : IQueryHandler<GetPortfolioPerformanceQuery, PortfolioPerformanceDto?>
{
    public async Task<PortfolioPerformanceDto?> HandleAsync(GetPortfolioPerformanceQuery query, CancellationToken ct = default)
    {
        var dto = await context.PortfolioPerformances
            .AsNoTracking()
            .Where(ppc => ppc.PortfolioId == query.PortfolioId)
            .Select(ppc => new PortfolioPerformanceDto
            {
                Value = ppc.Value,
                CashValue = ppc.CashValue,
                PreviousValue = ppc.PreviousValue,
                ValueChange = ppc.ValueChange,
                UnrealisedGain = ppc.UnrealisedGain,
                RealisedGain = ppc.RealisedGain,
                Income = ppc.Income,
                Return1D = ppc.Return1D,
                Return1W = ppc.Return1W,
                Return1M = ppc.Return1M,
                Return3M = ppc.Return3M,
                Return6M = ppc.Return6M,
                Return1Y = ppc.Return1Y,
                Return3Y = ppc.Return3Y,
                Return5Y = ppc.Return5Y,
                Return10Y = ppc.Return10Y,
                ReturnYtd = ppc.ReturnYtd,
                ReturnAllTime = ppc.ReturnAllTime,
                AnnualisedReturn = ppc.AnnualisedReturn,
                UpdatedUtc = ppc.UpdatedUtc
            }).SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "Performance for Portfolio with ID {PortfolioId} not found", query.PortfolioId);
        }

        return dto;
    }
}