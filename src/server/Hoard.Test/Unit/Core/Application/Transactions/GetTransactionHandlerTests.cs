using Hoard.Core.Application.Transactions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class GetTransactionHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingTransaction_ReturnsDetail()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        SeedReferenceData(context);
        context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, InstrumentId = 1, TransactionTypeId = 1,
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
        await using var context = CreateInMemoryContext();
        var handler = new GetTransactionHandler(context, NullLogger<GetTransactionHandler>.Instance);

        var result = await handler.HandleAsync(new GetTransactionQuery(999));

        Assert.Null(result);
    }

    private static void SeedReferenceData(HoardContext context)
    {
        context.Accounts.Add(new Account { Id = 1, Name = "Test Account", Colour = "#FF0000" });
        context.Instruments.Add(new Instrument { Id = 1, Name = "Test Stock", Ticker = "TEST", CurrencyId = "GBP" });
        context.TransactionTypes.Add(new TransactionType { Id = 1, Name = "Buy" });
    }

    private static HoardContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HoardContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new HoardContext(options);
    }
}
