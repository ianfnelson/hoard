using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioIdsForInstrumentHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPortfolioIdsWithActiveHoldings()
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
        context.Holdings.Add(new Holding
        {
            AccountId = 1, InstrumentId = 1, AsOfDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Units = 100, UpdatedUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new GetPortfolioIdsForInstrumentHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioIdsForInstrumentQuery(1));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_ZeroUnitsHolding_ReturnsEmpty()
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
        context.Holdings.Add(new Holding
        {
            AccountId = 1, InstrumentId = 1, AsOfDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Units = 0, UpdatedUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new GetPortfolioIdsForInstrumentHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioIdsForInstrumentQuery(1));

        // Assert
        Assert.Empty(result);
    }
}
