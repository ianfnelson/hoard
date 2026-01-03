namespace Hoard.Core.Application.Portfolios;

public abstract class ExposureDto
{
    public decimal ActualValue { get; init; }
    public decimal ActualPercentage { get; init; }

    public decimal TargetValue { get; init; }
    public decimal TargetPercentage { get; init; }

    public decimal DeviationValue { get; init; }
    public decimal DeviationPercentage { get; init; }
}