namespace Hoard.Core.Application.Portfolios;

public sealed class AssetSubclassExposureDto : ExposureDto
{
    public int AssetClassId { get; init; }
    public required string AssetClassName { get; init; }

    public int AssetSubclassId { get; init; }
    public required string AssetSubclassName { get; init; }
}