using Hoard.Core.Application.Portfolios;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioValuationSeriesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSingleSeriesWithTotalByDefault()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var account = TestData.Account();
        var portfolio = TestData.Portfolio();
        account.Portfolios.Add(portfolio);
        var instrument = TestData.Instrument();

        context.Portfolios.Add(portfolio);
        context.Accounts.Add(account);
        context.Instruments.Add(instrument);
        TestData.SeedReferenceData(context);

        context.Holdings.AddRange(
            CreateHoldingWithValuation(1, 1, 1, new DateOnly(2024, 1, 1), 1000m),
            CreateHoldingWithValuation(2, 1, 1, new DateOnly(2024, 1, 2), 1100m)
        );
        await context.SaveChangesAsync();

        var handler = new GetPortfolioValuationSeriesHandler(context, NullLogger<GetPortfolioValuationSeriesHandler>.Instance, TimeProvider.System);
        var query = new GetPortfolioValuationSeriesQuery(1)
        {
            From = new DateOnly(2024, 1, 1),
            To = new DateOnly(2024, 1, 2)
        };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Series);
        Assert.Equal("Total", result.Series[0].Name);
        Assert.Equal(2, result.Series[0].Data.Count);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPortfolio_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfolioValuationSeriesHandler(context, NullLogger<GetPortfolioValuationSeriesHandler>.Instance, TimeProvider.System);

        var result = await handler.HandleAsync(new GetPortfolioValuationSeriesQuery(999));

        Assert.Null(result);
    }

    private static Holding CreateHoldingWithValuation(int id, int accountId, int instrumentId, DateOnly date, decimal value)
        => new()
        {
            Id = id, AccountId = accountId, InstrumentId = instrumentId,
            AsOfDate = date, Units = 100, UpdatedUtc = DateTime.UtcNow,
            Valuation = new HoldingValuation { Id = id, Value = value, UpdatedUtc = DateTime.UtcNow }
        };
}
