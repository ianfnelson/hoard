namespace Hoard.Core.Application.Portfolios;

public sealed class PortfolioExposureDto
{
    public int PortfolioId { get; init; }
    public DateOnly AsOfDate { get; init; }

    public decimal TotalValue { get; init; }

    public IReadOnlyList<AssetClassExposureDto> AssetClasses { get; init; } = [];

    public IReadOnlyList<AssetSubclassExposureDto> AssetSubclasses { get; init; } = [];
    
    public IReadOnlyList<RebalanceActionDto> RebalanceActions { get; init; } = [];
}