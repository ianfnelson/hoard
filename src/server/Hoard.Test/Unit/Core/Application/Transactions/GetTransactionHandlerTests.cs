using Hoard.Core.Application.Transactions;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class GetTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingTransaction_ReturnsDetail()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        context.Accounts.Add(TestData.Account());
        context.Instruments.Add(TestData.Instrument());
        context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, InstrumentId = 1, TransactionTypeId = TransactionType.Buy,
            Date = new DateOnly(2024, 3, 15), Units = 100, Price = 10.50m, Value = -1050m
        });
        await context.SaveChangesAsync();

        var handler = new GetTransactionHandler(context, NullLogger<GetTransactionHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetTransactionQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Account", result.AccountName);
        Assert.Equal("TEST", result.InstrumentTicker);
        Assert.Equal(100m, result.Units);
        Assert.Equal(-1050m, result.Value);
    }

    [Fact]
    public async Task HandleAsync_NonExistentTransaction_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetTransactionHandler(context, NullLogger<GetTransactionHandler>.Instance);

        var result = await handler.HandleAsync(new GetTransactionQuery(999));

        Assert.Null(result);
    }

}
