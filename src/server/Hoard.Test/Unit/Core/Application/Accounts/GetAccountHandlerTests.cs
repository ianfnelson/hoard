using Hoard.Core.Application.Accounts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Accounts;

public class GetAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingAccount_ReturnsDetailWithPortfolios()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var portfolio = new Portfolio { Id = 1, Name = "Main Portfolio" };
        var account = new Account { Id = 1, Name = "ISA", Colour = "#FF0000", IsActive = true };
        account.Portfolios.Add(portfolio);

        context.Portfolios.Add(portfolio);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        var handler = new GetAccountHandler(context, NullLogger<GetAccountHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetAccountQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ISA", result.Name);
        Assert.Equal("#FF0000", result.Colour);
        Assert.Single(result.Portfolios);
        Assert.Equal("Main Portfolio", result.Portfolios[0].Name);
    }

    [Fact]
    public async Task HandleAsync_NonExistentAccount_ReturnsNull()
    {
        await using var context = CreateInMemoryContext();
        var handler = new GetAccountHandler(context, NullLogger<GetAccountHandler>.Instance);

        var result = await handler.HandleAsync(new GetAccountQuery(999));

        Assert.Null(result);
    }

    private static HoardContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HoardContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new HoardContext(options);
    }
}
