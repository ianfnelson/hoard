using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioSnapshotsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSnapshotsForPortfolio()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        context.PortfolioSnapshots.AddRange(
            new PortfolioSnapshot { Id = 1, PortfolioId = 1, Year = 2023, StartValue = 10000, EndValue = 12000, ValueChange = 2000, AverageValue = 11000, Return = 0.2m, Churn = 0.1m, Yield = 0.03m, UpdatedUtc = DateTime.UtcNow },
            new PortfolioSnapshot { Id = 2, PortfolioId = 1, Year = 2024, StartValue = 12000, EndValue = 15000, ValueChange = 3000, AverageValue = 13500, Return = 0.25m, Churn = 0.05m, Yield = 0.04m, UpdatedUtc = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetPortfolioSnapshotsHandler(context, NullLogger<GetPortfolioSnapshotsHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioSnapshotsQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Snapshots.Count);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPortfolio_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfolioSnapshotsHandler(context, NullLogger<GetPortfolioSnapshotsHandler>.Instance);

        var result = await handler.HandleAsync(new GetPortfolioSnapshotsQuery(999));

        Assert.Null(result);
    }
}
