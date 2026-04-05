using Hoard.Core.Application.Currencies;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Test.Unit.Core.Application.Currencies;

public class GetCurrenciesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsCurrenciesOrderedByName()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        context.Currencies.AddRange(
            new Currency { Id = "USD", Name = "US Dollar" },
            new Currency { Id = "GBP", Name = "British Pound" },
            new Currency { Id = "EUR", Name = "Euro" }
        );
        await context.SaveChangesAsync();

        var handler = new GetCurrenciesHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetCurrenciesQuery());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("British Pound", result[0].Name);
        Assert.Equal("Euro", result[1].Name);
        Assert.Equal("US Dollar", result[2].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = CreateInMemoryContext();
        var handler = new GetCurrenciesHandler(context);

        var result = await handler.HandleAsync(new GetCurrenciesQuery());

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
