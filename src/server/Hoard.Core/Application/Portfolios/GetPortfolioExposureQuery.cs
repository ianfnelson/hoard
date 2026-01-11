using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Portfolios;

public record GetPortfolioExposureQuery(int PortfolioId) : IQuery<PortfolioExposureDto?>;

public class GetPortfolioExposureHandler(HoardContext context, ILogger<GetPortfolioExposureHandler> logger)
    : IQueryHandler<GetPortfolioExposureQuery, PortfolioExposureDto?>
{
    private const decimal RebalanceReducePortfolioPercentage = 1.0M;
    private const decimal RebalanceAddPortfolioPercentage = -0.5M;
    
    public async Task<PortfolioExposureDto?> HandleAsync(
        GetPortfolioExposureQuery query,
        CancellationToken ct = default)
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
        
        var today = DateOnlyHelper.TodayLocal();

        // ------------------------------------------------------------------
        // 1. Load taxonomy (asset subclasses + asset classes)
        // ------------------------------------------------------------------
        var subclasses = await context.AssetSubclasses
            .AsNoTracking()
            .Select(sc => new
            {
                sc.Id,
                sc.Code,
                sc.Name,
                sc.AssetClassId,
                AssetClassCode = sc.AssetClass.Code,
                AssetClassName = sc.AssetClass.Name
            })
            .ToListAsync(ct);

        // ------------------------------------------------------------------
        // 2. Load actual values per subclass
        // ------------------------------------------------------------------
        var actuals = await context.HoldingValuations
            .AsNoTracking()
            .Where(hv => hv.Holding.Account.Portfolios.Any(p => p.Id == query.PortfolioId))
            .Where(hv => hv.Holding.AsOfDate == today)
            .GroupBy(hv => hv.Holding.Instrument.AssetSubclassId)
            .Select(g => new
            {
                AssetSubclassId = g.Key,
                ActualValue = g.Sum(x => (decimal?)x.Value) ?? 0m
            })
            .ToListAsync(ct);

        var actualBySubclassId = actuals
            .ToDictionary(x => x.AssetSubclassId, x => x.ActualValue);

        // ------------------------------------------------------------------
        // 3. Load target allocations per subclass
        // ------------------------------------------------------------------
        var targets = await context.TargetAllocations
            .AsNoTracking()
            .Where(ta => ta.PortfolioId == query.PortfolioId)
            .Select(ta => new
            {
                ta.AssetSubclassId,
                TargetPercentage = (decimal?)ta.Target ?? 0m
            })
            .ToListAsync(ct);

        var targetBySubclassId = targets
            .ToDictionary(x => x.AssetSubclassId, x => x.TargetPercentage);

        // ------------------------------------------------------------------
        // 4. Assemble raw rows (NO rounding here)
        // ------------------------------------------------------------------
        var rows = subclasses.Select(sc =>
        {
            var actualValue = actualBySubclassId.TryGetValue(sc.Id, out var av) ? av : 0m;
            var targetPct = targetBySubclassId.TryGetValue(sc.Id, out var tp) ? tp : 0m;

            return new
            {
                sc.Id,
                sc.Code,
                sc.Name,

                sc.AssetClassId,
                sc.AssetClassCode,
                sc.AssetClassName,

                ActualValue = actualValue,
                TargetPercentage = targetPct
            };
        }).ToList();

        var totalValue = rows.Sum(x => x.ActualValue);

        // ------------------------------------------------------------------
        // 5. Subclass exposure (full precision internally)
        // ------------------------------------------------------------------
        var subclassExposureRaw = rows.Select(x =>
        {
            var actualPct = totalValue == 0 ? 0m : x.ActualValue / totalValue * 100m;

            return new
            {
                x.AssetClassId,
                x.AssetClassCode,
                x.AssetClassName,

                AssetSubclassId = x.Id,
                AssetSubclassCode = x.Code,
                AssetSubclassName = x.Name,

                ActualValue = x.ActualValue,
                ActualPercentage = actualPct,
                
                TargetValue = totalValue * x.TargetPercentage / 100.0m,
                TargetPercentage = x.TargetPercentage,
                
                DeviationValue = totalValue * (actualPct - x.TargetPercentage) / 100.0m,
                DeviationPercentage = x.TargetPercentage == 0.0M ? 0.0M : 100.0M * (actualPct - x.TargetPercentage) / x.TargetPercentage
            };
        })
        .Where(x => x.ActualPercentage != 0m || x.TargetPercentage != 0m)
        .ToList();

        // ------------------------------------------------------------------
        // 6. Asset subclass DTOs (ROUND HERE – final boundary)
        // ------------------------------------------------------------------
        var subclassExposureDtos = subclassExposureRaw
            .Select(x => new AssetSubclassExposureDto
            {
                AssetClassId = x.AssetClassId,
                AssetClassCode = x.AssetClassCode,
                AssetClassName = x.AssetClassName,

                AssetSubclassId = x.AssetSubclassId,
                AssetSubclassCode = x.AssetSubclassCode,
                AssetSubclassName = x.AssetSubclassName,

                ActualValue = x.ActualValue.RoundTo2Dp(),
                ActualPercentage = x.ActualPercentage,

                TargetValue = x.TargetValue.RoundTo2Dp(),
                TargetPercentage = x.TargetPercentage,

                DeviationValue = x.DeviationValue.RoundTo2Dp(),
                DeviationPercentage = x.DeviationPercentage
            })
            .OrderByDescending(x => x.ActualPercentage)
            .ToList();

        // ------------------------------------------------------------------
        // 7. Asset class roll-up (from UNROUNDED subclass data)
        // ------------------------------------------------------------------
        var assetClassExposureDtos =
            subclassExposureRaw
                .GroupBy(x => new
                {
                    x.AssetClassId,
                    x.AssetClassCode,
                    x.AssetClassName
                })
                .Select(g =>
                {
                    var actualValue = g.Sum(x => x.ActualValue);
                    var actualPct = g.Sum(x => x.ActualPercentage);
                    
                    var targetValue = g.Sum(x => x.TargetValue);
                    var targetPct = g.Sum(x => x.TargetPercentage);
                    
                    var deviationValue = g.Sum(x => x.DeviationValue);
                    var deviationPct = targetPct == 0.0M ? 0.0M : 100.0M * (actualPct - targetPct) / targetPct;

                    return new AssetClassExposureDto
                    {
                        AssetClassId = g.Key.AssetClassId,
                        AssetClassCode = g.Key.AssetClassCode,
                        AssetClassName = g.Key.AssetClassName,

                        ActualValue = actualValue.RoundTo2Dp(),
                        ActualPercentage = actualPct,

                        TargetValue = targetValue.RoundTo2Dp(),
                        TargetPercentage = targetPct,
                        
                        DeviationValue = deviationValue.RoundTo2Dp(),
                        DeviationPercentage = deviationPct
                    };
                })
                .OrderByDescending(x => x.ActualPercentage)
                .ToList();

        var rebalanceActions = DeriveRebalanceActions(totalValue, subclassExposureDtos);
        
        // ------------------------------------------------------------------
        // 8. Final DTO
        // ------------------------------------------------------------------
        return new PortfolioExposureDto
        {
            PortfolioId = query.PortfolioId,
            AsOfDate = today,

            TotalValue = totalValue.RoundTo2Dp(),

            AssetClasses = assetClassExposureDtos,
            AssetSubclasses = subclassExposureDtos,
            RebalanceActions = rebalanceActions
        };
    }

    private static List<RebalanceActionDto> DeriveRebalanceActions(decimal totalValue, List<AssetSubclassExposureDto> subclassExposureDtos)
    {
        var actions = new List<RebalanceActionDto>();
        
        var reduceDeviance = RebalanceReducePortfolioPercentage * totalValue / 100.0M;
        var addDeviance = RebalanceAddPortfolioPercentage * totalValue / 100.0M;
        
        var cash = subclassExposureDtos
            .SingleOrDefault(x => x.AssetSubclassId == AssetSubclass.Cash)?.ActualValue ?? 0m;

        var subclassesToSell = subclassExposureDtos
            .Where(x => x.AssetSubclassId != AssetSubclass.Cash)
            .Where(x => x.DeviationValue >= reduceDeviance);

        foreach (var subclassToSell in subclassesToSell)
        {
            actions.Add(new RebalanceActionDto
            {
                AssetSubclassCode = subclassToSell.AssetSubclassCode,
                AssetSubclassId = subclassToSell.AssetSubclassId,
                AssetSubclassName = subclassToSell.AssetSubclassName,
                RebalanceAction = RebalanceActionType.Reduce,
                Amount = subclassToSell.DeviationValue
            });
            
            cash += subclassToSell.DeviationValue;
        }

        var subclassesToAdd = subclassExposureDtos
            .Where(x => x.AssetSubclassId != AssetSubclass.Cash)
            .Where(x => x.DeviationValue <= addDeviance)
            .OrderBy(x => x.DeviationValue);

        foreach (var subclassToAdd in subclassesToAdd)
        {
            var amount = Math.Min(-subclassToAdd.DeviationValue, cash);
            
            actions.Add(new RebalanceActionDto
            {
                AssetSubclassCode = subclassToAdd.AssetSubclassCode,
                AssetSubclassName = subclassToAdd.AssetSubclassName,
                AssetSubclassId = subclassToAdd.AssetClassId,
                RebalanceAction = RebalanceActionType.Add,
                Amount = amount
            });

            cash -= amount;

            if (cash < -addDeviance)
            {
                break;
            }
        }
        
        return actions;
    }
}