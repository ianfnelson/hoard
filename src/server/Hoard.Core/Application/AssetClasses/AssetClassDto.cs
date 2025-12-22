namespace Hoard.Core.Application.AssetClasses;

public class AssetClassDto
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}