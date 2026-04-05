using Hoard.Core.Application.Transactions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class GetTransactionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPagedResults()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        SeedReferenceData(context);
        for (var i = 1; i <= 10; i++)
        {
            context.Transactions.Add(new Transaction
            {
                Id = i, AccountId = 1, InstrumentId = 1, TransactionTypeId = 1,
                Date = new DateOnly(2024, 1, i), Value = -100m * i
            });
        }
        await context.SaveChangesAsync();

        var handler = new GetTransactionsHandler(context);
        var query = new GetTransactionsQuery { PageNumber = 1, PageSize = 3 };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task HandleAsync_FiltersByDateRange()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        SeedReferenceData(context);
        context.Transactions.AddRange(
            new Transaction { Id = 1, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 1), Value = -100m },
            new Transaction { Id = 2, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 15), Value = -200m },
            new Transaction { Id = 3, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 2, 1), Value = -300m }
        );
        await context.SaveChangesAsync();

        var handler = new GetTransactionsHandler(context);
        var query = new GetTransactionsQuery
        {
            FromDate = new DateOnly(2024, 1, 10),
            ToDate = new DateOnly(2024, 1, 20)
        };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(new DateOnly(2024, 1, 15), result.Items[0].Date);
    }

    [Fact]
    public async Task HandleAsync_FiltersByAccountId()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        SeedReferenceData(context);
        context.Accounts.Add(new Account { Id = 2, Name = "Other Account", Colour = "#00FF00" });
        context.Transactions.AddRange(
            new Transaction { Id = 1, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 1), Value = -100m },
            new Transaction { Id = 2, AccountId = 2, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 1), Value = -200m }
        );
        await context.SaveChangesAsync();

        var handler = new GetTransactionsHandler(context);
        var query = new GetTransactionsQuery { AccountId = 1 };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Items[0].AccountId);
    }

    [Fact]
    public async Task HandleAsync_FiltersByInstrumentId()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        SeedReferenceData(context);
        context.Instruments.Add(new Instrument { Id = 2, Name = "Other Stock", Ticker = "OTH", CurrencyId = "GBP" });
        context.Transactions.AddRange(
            new Transaction { Id = 1, AccountId = 1, InstrumentId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 1), Value = -100m },
            new Transaction { Id = 2, AccountId = 1, InstrumentId = 2, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 1), Value = -200m }
        );
        await context.SaveChangesAsync();

        var handler = new GetTransactionsHandler(context);
        var query = new GetTransactionsQuery { InstrumentId = 1 };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_SearchByContractNoteReference()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        SeedReferenceData(context);
        context.Transactions.AddRange(
            new Transaction { Id = 1, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 1), Value = -100m, ContractNoteReference = "CN-001" },
            new Transaction { Id = 2, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 2), Value = -200m, ContractNoteReference = "CN-002" }
        );
        await context.SaveChangesAsync();

        var handler = new GetTransactionsHandler(context);
        var query = new GetTransactionsQuery { Search = "CN-001" };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_DefaultSortByDateDescending()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        SeedReferenceData(context);
        context.Transactions.AddRange(
            new Transaction { Id = 1, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 1), Value = -100m },
            new Transaction { Id = 2, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 3), Value = -300m },
            new Transaction { Id = 3, AccountId = 1, TransactionTypeId = 1, Date = new DateOnly(2024, 1, 2), Value = -200m }
        );
        await context.SaveChangesAsync();

        var handler = new GetTransactionsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetTransactionsQuery());

        // Assert
        Assert.Equal(new DateOnly(2024, 1, 3), result.Items[0].Date);
        Assert.Equal(new DateOnly(2024, 1, 2), result.Items[1].Date);
        Assert.Equal(new DateOnly(2024, 1, 1), result.Items[2].Date);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyPagedResult()
    {
        await using var context = CreateInMemoryContext();
        var handler = new GetTransactionsHandler(context);

        var result = await handler.HandleAsync(new GetTransactionsQuery());

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
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
