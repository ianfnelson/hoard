namespace Hoard.Core.Application.AssetClasses;

public class AssetClassDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public required List<AssetSubclassDto> Subclasses { get; set; }
}