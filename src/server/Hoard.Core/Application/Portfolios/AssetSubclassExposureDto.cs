namespace Hoard.Core.Application.Portfolios;

public sealed class AssetSubclassExposureDto : ExposureDto
{
    public int AssetClassId { get; init; }
    public required string AssetClassName { get; init; }
    public required string AssetClassColour { get; init; }
}