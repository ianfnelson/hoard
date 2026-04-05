using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioIdsHandlerTests
{
    [Fact]
    public async Task HandleAsync_NoPortfolioId_ReturnsAllActiveIds()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.AddRange(
            new Portfolio { Id = 1, Name = "Active 1", IsActive = true },
            new Portfolio { Id = 2, Name = "Active 2", IsActive = true },
            new Portfolio { Id = 3, Name = "Inactive", IsActive = false }
        );
        await context.SaveChangesAsync();

        var handler = new GetPortfolioIdsHandler(context, NullLogger<GetPortfolioIdsHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioIdsQuery());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public async Task HandleAsync_ActiveOnlyFalse_ReturnsAllIds()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.AddRange(
            new Portfolio { Id = 1, Name = "Active", IsActive = true },
            new Portfolio { Id = 2, Name = "Inactive", IsActive = false }
        );
        await context.SaveChangesAsync();

        var handler = new GetPortfolioIdsHandler(context, NullLogger<GetPortfolioIdsHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioIdsQuery(ActiveOnly: false));

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_SpecificPortfolioId_ReturnsSingleId()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(new Portfolio { Id = 1, Name = "Main" });
        await context.SaveChangesAsync();

        var handler = new GetPortfolioIdsHandler(context, NullLogger<GetPortfolioIdsHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioIdsQuery(PortfolioId: 1));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPortfolioId_ReturnsEmpty()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfolioIdsHandler(context, NullLogger<GetPortfolioIdsHandler>.Instance);

        var result = await handler.HandleAsync(new GetPortfolioIdsQuery(PortfolioId: 999));

        Assert.Empty(result);
    }
}
