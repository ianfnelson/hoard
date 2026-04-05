using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingPortfolio_ReturnsDetail()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(new Portfolio { Id = 1, Name = "Main Portfolio", IsActive = true });
        await context.SaveChangesAsync();

        var handler = new GetPortfolioHandler(context, NullLogger<GetPortfolioHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Main Portfolio", result.Name);
        Assert.True(result.IsActive);
        Assert.Null(result.Performance);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPortfolio_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfolioHandler(context, NullLogger<GetPortfolioHandler>.Instance);

        var result = await handler.HandleAsync(new GetPortfolioQuery(999));

        Assert.Null(result);
    }
}
