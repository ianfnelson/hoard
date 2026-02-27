using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Portfolios;

public interface IRebalanceService
{
    List<RebalanceActionDto> DeriveRebalanceActions(decimal totalValue, List<AssetSubclassExposureDto> subclassExposureDtos);
}

public class RebalanceService : IRebalanceService
{
    private const decimal RebalanceReducePortfolioPercentage = 1.0M;
    private const decimal RebalanceAddPortfolioPercentage = -0.5M;

    public List<RebalanceActionDto> DeriveRebalanceActions(decimal totalValue, List<AssetSubclassExposureDto> subclassExposureDtos)
    {
        var actions = new List<RebalanceActionDto>();

        var reduceDeviance = RebalanceReducePortfolioPercentage * totalValue / 100.0M;
        var addDeviance = RebalanceAddPortfolioPercentage * totalValue / 100.0M;

        var cash = subclassExposureDtos
            .SingleOrDefault(x => x.Id == AssetSubclass.Cash)?.Value ?? 0m;

        var subclassesToSell = subclassExposureDtos
            .Where(x => x.Id != AssetSubclass.Cash)
            .Where(x => x.DeviationValue >= reduceDeviance);

        foreach (var subclassToSell in subclassesToSell)
        {
            actions.Add(new RebalanceActionDto
            {
                AssetSubclassId = subclassToSell.Id,
                AssetSubclassName = subclassToSell.Name,
                Colour = subclassToSell.Colour,
                RebalanceAction = RebalanceActionType.Reduce,
                Amount = subclassToSell.DeviationValue
            });

            cash += subclassToSell.DeviationValue;
        }

        var subclassesToAdd = subclassExposureDtos
            .Where(x => x.Id != AssetSubclass.Cash)
            .Where(x => x.DeviationValue <= addDeviance)
            .OrderBy(x => x.DeviationValue);

        foreach (var subclassToAdd in subclassesToAdd)
        {
            var amount = Math.Min(-subclassToAdd.DeviationValue, cash);

            if (amount < -addDeviance)
            {
                break;
            }

            actions.Add(new RebalanceActionDto
            {
                AssetSubclassName = subclassToAdd.Name,
                AssetSubclassId = subclassToAdd.AssetClassId,
                Colour = subclassToAdd.Colour,
                RebalanceAction = RebalanceActionType.Add,
                Amount = amount
            });

            cash -= amount;
        }

        return actions;
    }
}
