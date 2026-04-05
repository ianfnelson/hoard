using Hoard.Core.Application.InstrumentTypes;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Test.Unit.Core.Application.InstrumentTypes;

public class GetInstrumentTypesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsInstrumentTypesOrderedByName()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        context.InstrumentTypes.AddRange(
            new InstrumentType { Id = 1, Name = "Stock", Colour = "#FF0000" },
            new InstrumentType { Id = 2, Name = "ETF", Colour = "#00FF00" },
            new InstrumentType { Id = 3, Name = "Fund", Colour = "#0000FF" }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentTypesHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentTypesQuery());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("ETF", result[0].Name);
        Assert.Equal("Fund", result[1].Name);
        Assert.Equal("Stock", result[2].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = CreateInMemoryContext();
        var handler = new GetInstrumentTypesHandler(context);

        var result = await handler.HandleAsync(new GetInstrumentTypesQuery());

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
