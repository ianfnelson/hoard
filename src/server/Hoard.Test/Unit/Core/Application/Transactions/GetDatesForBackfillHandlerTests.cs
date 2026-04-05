using Hoard.Core.Application.Transactions;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class GetDatesForBackfillHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExplicitDateRange_ReturnsAllDatesInRange()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var handler = new GetDatesForBackfillHandler(context);
        var query = new GetDatesForBackfillQuery(
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 1, 3));

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new DateOnly(2024, 1, 1), result[0]);
        Assert.Equal(new DateOnly(2024, 1, 2), result[1]);
        Assert.Equal(new DateOnly(2024, 1, 3), result[2]);
    }

    [Fact]
    public async Task HandleAsync_WithSingleDate_ReturnsSingleDate()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var handler = new GetDatesForBackfillHandler(context);
        var query = new GetDatesForBackfillQuery(
            new DateOnly(2024, 6, 15),
            new DateOnly(2024, 6, 15));

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal(new DateOnly(2024, 6, 15), result[0]);
    }

    [Fact]
    public async Task HandleAsync_NoStartDate_UsesEarliestTransactionDate()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Accounts.Add(TestData.Account());
        context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        context.Transactions.AddRange(
            TestData.BuyTransaction(date: new DateOnly(2024, 3, 10)),
            TestData.BuyTransaction(date: new DateOnly(2024, 3, 5))
        );
        await context.SaveChangesAsync();

        var handler = new GetDatesForBackfillHandler(context);
        var query = new GetDatesForBackfillQuery(null, new DateOnly(2024, 3, 7));

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(new DateOnly(2024, 3, 5), result[0]);
        Assert.Equal(3, result.Count);
    }

}
