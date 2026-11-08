using Hoard.Core.Application.Performance;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Performance;

public class GetInstrumentsForPerformanceHandlerTests
{
    [Fact]
    public async Task HandleAsync_NoInstrumentId_ReturnsDistinctInstrumentIdsFromPositions()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        context.Instruments.AddRange(TestData.Instrument(1), TestData.Instrument(2, name: "Other", ticker: "OTH"));
        context.Positions.AddRange(
            new Position { Id = 1, PortfolioId = 1, InstrumentId = 1, OpenDate = new DateOnly(2024, 1, 1) },
            new Position { Id = 2, PortfolioId = 1, InstrumentId = 2, OpenDate = new DateOnly(2024, 1, 1) },
            new Position { Id = 3, PortfolioId = 1, InstrumentId = 1, OpenDate = new DateOnly(2024, 6, 1) }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForPerformanceHandler(context, NullLogger<GetInstrumentsForPerformanceHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsForPerformanceQuery(null));

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_SpecificInstrumentId_ReturnsSingleId()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        context.Instruments.Add(TestData.Instrument());
        context.Positions.Add(new Position { Id = 1, PortfolioId = 1, InstrumentId = 1, OpenDate = new DateOnly(2024, 1, 1) });
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForPerformanceHandler(context, NullLogger<GetInstrumentsForPerformanceHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsForPerformanceQuery(1));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrumentId_ReturnsEmpty()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetInstrumentsForPerformanceHandler(context, NullLogger<GetInstrumentsForPerformanceHandler>.Instance);

        var result = await handler.HandleAsync(new GetInstrumentsForPerformanceQuery(999));

        Assert.Empty(result);
    }
}
