namespace Hoard.Core.Application.Portfolios;

public sealed class AssetClassExposureDto : ExposureDto
{
    public int AssetClassId { get; init; }
    public string AssetClassName { get; init; } = null!;
}