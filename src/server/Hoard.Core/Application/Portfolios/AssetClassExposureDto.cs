namespace Hoard.Core.Application.Portfolios;

public sealed class AssetClassExposureDto : ExposureDto
{
    public int AssetClassId { get; init; }
    public string AssetClassCode { get; init; } = null!;
    public string AssetClassName { get; init; } = null!;
}

public abstract class ExposureDto
{
    public decimal ActualValue { get; init; }
    public decimal ActualPercentage { get; init; }

    public decimal TargetValue { get; init; }
    public decimal TargetPercentage { get; init; }

    public decimal DeviationValue { get; init; }
    public decimal DeviationPercentage { get; init; }
}