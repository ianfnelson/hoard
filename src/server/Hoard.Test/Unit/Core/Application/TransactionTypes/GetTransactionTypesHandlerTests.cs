using Hoard.Core.Application.TransactionTypes;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.TransactionTypes;

public class GetTransactionTypesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsTransactionTypesOrderedByName()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.TransactionTypes.AddRange(
            new TransactionType { Id = 1, Name = "Sell" },
            new TransactionType { Id = 2, Name = "Buy" },
            new TransactionType { Id = 3, Name = "Dividend" }
        );
        await context.SaveChangesAsync();

        var handler = new GetTransactionTypesHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetTransactionTypesQuery());

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Buy", result[0].Name);
        Assert.Equal("Dividend", result[1].Name);
        Assert.Equal("Sell", result[2].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetTransactionTypesHandler(context);

        var result = await handler.HandleAsync(new GetTransactionTypesQuery());

        Assert.Empty(result);
    }

}
