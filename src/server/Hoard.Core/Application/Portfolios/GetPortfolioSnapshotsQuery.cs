using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioSnapshotsQuery(int PortfolioId) : IQuery<PortfolioSnapshotsDto?>;

public class GetPortfolioSnapshotsHandler(HoardContext context, ILogger<GetPortfolioSnapshotsHandler> logger)
    : IQueryHandler<GetPortfolioSnapshotsQuery, PortfolioSnapshotsDto?>
{
    public async Task<PortfolioSnapshotsDto?> HandleAsync(GetPortfolioSnapshotsQuery query, CancellationToken ct = default)
    {
        var exists = await context.Portfolios
            .AnyAsync(x => x.Id == query.PortfolioId, ct);

        if (!exists)
        {
            logger.LogWarning(
                "Portfolio with id {PortfolioId} not found",
                query.PortfolioId);
            return null;
        }
        
        var dtos = await context.PortfolioSnapshots
            .AsNoTracking()
            .Where(p => p.PortfolioId == query.PortfolioId)
            .Select(p => new PortfolioSnapshotDto
            {
                Year = p.Year,
                StartValue = p.StartValue,
                EndValue = p.EndValue,
                ValueChange = p.ValueChange,
                AverageValue = p.AverageValue,
                Return = p.Return,
                Churn = p.Churn,
                Yield = p.Yield,
                TotalBuys = p.TotalBuys,
                TotalSells = p.TotalSells,
                TotalIncomeDividends = p.TotalIncomeDividends,
                TotalIncomeInterest = p.TotalIncomeInterest,
                TotalIncomeLoyaltyBonus = p.TotalIncomeLoyaltyBonus,
                TotalPromotion = p.TotalPromotion,
                TotalFees = p.TotalFees,
                TotalDealingCharge = p.TotalDealingCharge,
                TotalStampDuty = p.TotalStampDuty,
                TotalPtmLevy = p.TotalPtmLevy,
                TotalFxCharge = p.TotalFxCharge,
                TotalDepositPersonal = p.TotalDepositPersonal,
                TotalDepositEmployer = p.TotalDepositEmployer,
                TotalDepositIncomeTaxReclaim = p.TotalDepositIncomeTaxReclaim,
                TotalDepositTransferIn = p.TotalDepositTransferIn,
                TotalWithdrawals = p.TotalWithdrawals,
                CountTrades = p.CountTrades,
                UpdatedUtc = p.UpdatedUtc
                
            }).ToListAsync(ct);

        return new PortfolioSnapshotsDto
        {
            PortfolioId = query.PortfolioId,
            Snapshots = dtos
        };
    }
}