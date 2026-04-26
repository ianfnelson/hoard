using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioValuationsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsValuationItemsForPortfolio()
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

        var today = new DateOnly(2024, 6, 15);
        context.Holdings.Add(new Holding
        {
            Id = 1, AccountId = 1, InstrumentId = 1, AsOfDate = today,
            Units = 100, UpdatedUtc = DateTime.UtcNow,
            Valuation = new HoldingValuation { Id = 1, Value = 5000m, UpdatedUtc = DateTime.UtcNow }
        });
        await context.SaveChangesAsync();

        var handler = new GetPortfolioValuationsHandler(context, NullLogger<GetPortfolioValuationsHandler>.Instance, TimeProvider.System);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioValuationsQuery(1, null, today));

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Test Stock", result.Items[0].InstrumentName);
        Assert.Equal(5000m, result.Items[0].ToValue);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPortfolio_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfolioValuationsHandler(context, NullLogger<GetPortfolioValuationsHandler>.Instance, TimeProvider.System);

        var result = await handler.HandleAsync(new GetPortfolioValuationsQuery(999, null, null));

        Assert.Null(result);
    }
}
