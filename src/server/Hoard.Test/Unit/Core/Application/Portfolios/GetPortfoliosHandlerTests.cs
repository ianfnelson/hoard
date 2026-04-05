using Hoard.Core.Application.Portfolios;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfoliosHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPortfoliosOrderedAlphabetically()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.AddRange(
            new Portfolio { Id = 1, Name = "Zebra Fund" },
            new Portfolio { Id = 2, Name = "Alpha Portfolio" },
            new Portfolio { Id = 3, Name = "Middle Trust" }
        );
        await context.SaveChangesAsync();

        var handler = new GetPortfoliosHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetPortfoliosQuery());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Alpha Portfolio", result[0].Name);
        Assert.Equal("Middle Trust", result[1].Name);
        Assert.Equal("Zebra Fund", result[2].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfoliosHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetPortfoliosQuery());

        // Assert
        Assert.Empty(result);
    }

}
