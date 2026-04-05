using Hoard.Core.Application.Accounts;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Accounts;

public class GetAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingAccount_ReturnsDetailWithPortfolios()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
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
        await using var context = TestContextFactory.Create();
        var handler = new GetAccountHandler(context, NullLogger<GetAccountHandler>.Instance);

        var result = await handler.HandleAsync(new GetAccountQuery(999));

        Assert.Null(result);
    }

}
