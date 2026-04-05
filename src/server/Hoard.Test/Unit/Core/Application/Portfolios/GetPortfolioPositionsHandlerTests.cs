using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioPositionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPositionsForPortfolio()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        context.Instruments.Add(TestData.Instrument());
        context.Positions.Add(new Position
        {
            Id = 1, PortfolioId = 1, InstrumentId = 1,
            OpenDate = new DateOnly(2024, 1, 1)
        });
        await context.SaveChangesAsync();

        var handler = new GetPortfolioPositionsHandler(context, NullLogger<GetPortfolioHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioPositionsQuery(1, null));

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Positions);
        Assert.Equal("Test Stock", result.Positions[0].InstrumentName);
    }

    [Fact]
    public async Task HandleAsync_FiltersByOpenPositions()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        context.Instruments.Add(TestData.Instrument());
        context.Instruments.Add(new Instrument { Id = 2, Name = "Closed Stock", Ticker = "CLS", CurrencyId = "GBP" });
        context.Positions.AddRange(
            new Position { Id = 1, PortfolioId = 1, InstrumentId = 1, OpenDate = new DateOnly(2024, 1, 1) },
            new Position { Id = 2, PortfolioId = 1, InstrumentId = 2, OpenDate = new DateOnly(2024, 1, 1), CloseDate = new DateOnly(2024, 6, 1) }
        );
        await context.SaveChangesAsync();

        var handler = new GetPortfolioPositionsHandler(context, NullLogger<GetPortfolioHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioPositionsQuery(1, IsOpen: true));

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Positions);
        Assert.Equal("Test Stock", result.Positions[0].InstrumentName);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPortfolio_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfolioPositionsHandler(context, NullLogger<GetPortfolioHandler>.Instance);

        var result = await handler.HandleAsync(new GetPortfolioPositionsQuery(999, null));

        Assert.Null(result);
    }
}
