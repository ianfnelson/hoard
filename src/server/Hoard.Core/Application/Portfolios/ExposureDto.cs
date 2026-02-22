namespace Hoard.Core.Application.Portfolios;

public class ExposureDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Colour { get; init; }

    public decimal Value { get; init; }
    public decimal Percentage { get; init; }

    public decimal TargetValue { get; init; }
    public decimal TargetPercentage { get; init; }

    public decimal DeviationValue { get; init; }
    public decimal DeviationPercentage { get; init; }
}