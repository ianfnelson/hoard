namespace Hoard.Core.Application.AssetSubclasses;

public class AssetSubclassDto
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    
    public int AssetClassId { get; set; }
    public required string AssetClassCode { get; set; }
    public required string AssetClassName { get; set; }
}