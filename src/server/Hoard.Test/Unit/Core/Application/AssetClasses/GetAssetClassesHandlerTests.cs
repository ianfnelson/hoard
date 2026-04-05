using Hoard.Core.Application.AssetClasses;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.AssetClasses;

public class GetAssetClassesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAssetClassesWithSubclassesOrderedByName()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var equities = new AssetClass { Id = 1, Name = "Equities", Colour = "#0000FF" };
        equities.Subclasses.Add(new AssetSubclass { Id = 1, AssetClassId = 1, Name = "UK", Colour = "#0000AA" });
        equities.Subclasses.Add(new AssetSubclass { Id = 2, AssetClassId = 1, Name = "Global", Colour = "#0000BB" });
        var bonds = new AssetClass { Id = 2, Name = "Bonds", Colour = "#00FF00" };

        context.AssetClasses.AddRange(equities, bonds);
        await context.SaveChangesAsync();

        var handler = new GetAssetClassesHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetAssetClassesQuery());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Bonds", result[0].Name);
        Assert.Equal("Equities", result[1].Name);
        Assert.Equal(2, result[1].Subclasses.Count);
        Assert.Equal("Global", result[1].Subclasses[0].Name);
        Assert.Equal("UK", result[1].Subclasses[1].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetAssetClassesHandler(context);

        var result = await handler.HandleAsync(new GetAssetClassesQuery());

        Assert.Empty(result);
    }

}
