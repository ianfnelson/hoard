using Hoard.Core.Application.RunLog;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using RunType = Hoard.Core.Domain.Entities.RunType;

namespace Hoard.Test.Unit.Core.Application.RunLog;

public class GetRunTypesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsRunTypesOrderedByName()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        context.RunTypes.AddRange(
            new RunType { Id = 1, Name = "Refresh Quotes" },
            new RunType { Id = 2, Name = "Close Of Day" },
            new RunType { Id = 3, Name = "Refresh Prices" }
        );
        await context.SaveChangesAsync();

        var handler = new GetRunTypesHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetRunTypesQuery());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Close Of Day", result[0].Name);
        Assert.Equal("Refresh Prices", result[1].Name);
        Assert.Equal("Refresh Quotes", result[2].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = CreateInMemoryContext();
        var handler = new GetRunTypesHandler(context);

        var result = await handler.HandleAsync(new GetRunTypesQuery());

        Assert.Empty(result);
    }

    private static HoardContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HoardContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new HoardContext(options);
    }
}
