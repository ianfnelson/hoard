using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Valuations;

public class GetInstrumentsForHoldingValuationsHandlerTests
{
    [Fact]
    public async Task HandleAsync_NoInstrumentId_ReturnsDistinctInstrumentIdsFromHoldings()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Accounts.Add(TestData.Account());
        context.Instruments.AddRange(TestData.Instrument(1), TestData.Instrument(2, name: "Other", ticker: "OTH"));

        var today = new DateOnly(2024, 6, 15);
        context.Holdings.AddRange(
            new Holding { Id = 1, AccountId = 1, InstrumentId = 1, AsOfDate = today, Units = 100, UpdatedUtc = DateTime.UtcNow },
            new Holding { Id = 2, AccountId = 1, InstrumentId = 2, AsOfDate = today, Units = 50, UpdatedUtc = DateTime.UtcNow },
            new Holding { Id = 3, AccountId = 1, InstrumentId = 1, AsOfDate = today.AddDays(-1), Units = 90, UpdatedUtc = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForHoldingValuationsHandler(context, NullLogger<GetInstrumentsForHoldingValuationsHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsForHoldingValuationsQuery(today, null));

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_SpecificInstrumentId_ReturnsSingleId()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Instruments.Add(TestData.Instrument());
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForHoldingValuationsHandler(context, NullLogger<GetInstrumentsForHoldingValuationsHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsForHoldingValuationsQuery(new DateOnly(2024, 1, 1), 1));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrumentId_ReturnsEmpty()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetInstrumentsForHoldingValuationsHandler(context, NullLogger<GetInstrumentsForHoldingValuationsHandler>.Instance);

        var result = await handler.HandleAsync(new GetInstrumentsForHoldingValuationsQuery(new DateOnly(2024, 1, 1), 999));

        Assert.Empty(result);
    }
}
