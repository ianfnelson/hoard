using Hoard.Core.Application.AlertTypes;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.AlertTypes;

public class GetAlertTypesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAlertTypesOrderedByName()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.AlertTypes.AddRange(
            new AlertType { Id = 1, Name = "Price Movement" },
            new AlertType { Id = 2, Name = "Close Of Day" },
            new AlertType { Id = 3, Name = "News" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAlertTypesHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetAlertTypesQuery());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Close Of Day", result[0].Name);
        Assert.Equal("News", result[1].Name);
        Assert.Equal("Price Movement", result[2].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetAlertTypesHandler(context);

        var result = await handler.HandleAsync(new GetAlertTypesQuery());

        Assert.Empty(result);
    }

}
