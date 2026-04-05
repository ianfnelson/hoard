using Hoard.Core.Application.Accounts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Test.Unit.Core.Application.Accounts;

public class GetAccountsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAllAccounts()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        context.Accounts.AddRange(
            new Account { Id = 1, Name = "ISA", Colour = "#FF0000", IsActive = true },
            new Account { Id = 2, Name = "SIPP", Colour = "#00FF00", IsActive = false }
        );
        await context.SaveChangesAsync();

        var handler = new GetAccountsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetAccountsQuery());

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_FiltersByIsActive()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        context.Accounts.AddRange(
            new Account { Id = 1, Name = "ISA", Colour = "#FF0000", IsActive = true },
            new Account { Id = 2, Name = "Old SIPP", Colour = "#00FF00", IsActive = false },
            new Account { Id = 3, Name = "GIA", Colour = "#0000FF", IsActive = true }
        );
        await context.SaveChangesAsync();

        var handler = new GetAccountsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetAccountsQuery { IsActive = true });

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.True(a.IsActive));
    }

    [Fact]
    public async Task HandleAsync_FiltersByPortfolioId()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var portfolio = new Portfolio { Id = 1, Name = "Main" };
        var account1 = new Account { Id = 1, Name = "ISA", Colour = "#FF0000" };
        var account2 = new Account { Id = 2, Name = "SIPP", Colour = "#00FF00" };
        account1.Portfolios.Add(portfolio);

        context.Portfolios.Add(portfolio);
        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        var handler = new GetAccountsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetAccountsQuery { PortfolioId = 1 });

        // Assert
        Assert.Single(result);
        Assert.Equal("ISA", result[0].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = CreateInMemoryContext();
        var handler = new GetAccountsHandler(context);

        var result = await handler.HandleAsync(new GetAccountsQuery());

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
