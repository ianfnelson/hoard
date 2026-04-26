using Hoard.Core.Application.Snapshots;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Snapshots;

public class GetYearsForBackfillHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExplicitYearRange_ReturnsAllYearsInRange()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var handler = new GetYearsForBackfillHandler(context, TimeProvider.System);

        // Act
        var result = await handler.HandleAsync(new GetYearsForBackfillQuery(2020, 2024));

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(2020, result[0]);
        Assert.Equal(2024, result[4]);
    }

    [Fact]
    public async Task HandleAsync_NoStartYear_UsesEarliestTransactionYear()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Accounts.Add(TestData.Account());
        context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        context.Transactions.Add(TestData.BuyTransaction(date: new DateOnly(2018, 6, 15)));
        await context.SaveChangesAsync();

        var handler = new GetYearsForBackfillHandler(context, TimeProvider.System);

        // Act
        var result = await handler.HandleAsync(new GetYearsForBackfillQuery(null, 2020));

        // Assert
        Assert.Equal(2018, result[0]);
        Assert.Equal(3, result.Count);
    }
}
