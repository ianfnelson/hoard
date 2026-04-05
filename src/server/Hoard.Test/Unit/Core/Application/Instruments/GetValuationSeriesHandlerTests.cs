using Hoard.Core.Application.Instruments;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class GetValuationSeriesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSeriesGroupedByAccount()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var account1 = new Account { Id = 1, Name = "ISA", Colour = "#FF0000" };
        var account2 = new Account { Id = 2, Name = "SIPP", Colour = "#00FF00" };
        context.Accounts.AddRange(account1, account2);
        context.Instruments.Add(TestData.Instrument());

        context.Holdings.AddRange(
            CreateHoldingWithValuation(1, 1, new DateOnly(2024, 1, 1), 1000m),
            CreateHoldingWithValuation(2, 1, new DateOnly(2024, 1, 2), 1100m),
            CreateHoldingWithValuation(3, 2, new DateOnly(2024, 1, 1), 500m)
        );
        await context.SaveChangesAsync();

        var handler = new GetValuationSeriesHandler(context, NullLogger<GetValuationSeriesHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetValuationSeriesQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Series.Count);
    }

    [Fact]
    public async Task HandleAsync_FiltersDateRange()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Accounts.Add(new Account { Id = 1, Name = "ISA", Colour = "#FF0000" });
        context.Instruments.Add(TestData.Instrument());
        context.Holdings.AddRange(
            CreateHoldingWithValuation(1, 1, new DateOnly(2024, 1, 1), 1000m),
            CreateHoldingWithValuation(2, 1, new DateOnly(2024, 1, 15), 1100m),
            CreateHoldingWithValuation(3, 1, new DateOnly(2024, 2, 1), 1200m)
        );
        await context.SaveChangesAsync();

        var handler = new GetValuationSeriesHandler(context, NullLogger<GetValuationSeriesHandler>.Instance);
        var query = new GetValuationSeriesQuery(1)
        {
            From = new DateOnly(2024, 1, 10),
            To = new DateOnly(2024, 1, 20)
        };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Series);
        Assert.Single(result.Series[0].Data);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrument_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetValuationSeriesHandler(context, NullLogger<GetValuationSeriesHandler>.Instance);

        var result = await handler.HandleAsync(new GetValuationSeriesQuery(999));

        Assert.Null(result);
    }

    private static Holding CreateHoldingWithValuation(int id, int accountId, DateOnly date, decimal value)
        => new()
        {
            Id = id, AccountId = accountId, InstrumentId = 1,
            AsOfDate = date, Units = 100, UpdatedUtc = DateTime.UtcNow,
            Valuation = new HoldingValuation { Id = id, Value = value, UpdatedUtc = DateTime.UtcNow }
        };
}
