using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioInstrumentTypesHandlerTests
{
    [Fact]
    public async Task HandleAsync_NonExistentPortfolio_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPortfolioInstrumentTypesHandler(context, NullLogger<GetPortfolioInstrumentTypesHandler>.Instance, TimeProvider.System);

        var result = await handler.HandleAsync(new GetPortfolioInstrumentTypesQuery(999));

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_ExistingPortfolioNoHoldings_ReturnsEmptyList()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        await context.SaveChangesAsync();

        var handler = new GetPortfolioInstrumentTypesHandler(context, NullLogger<GetPortfolioInstrumentTypesHandler>.Instance, TimeProvider.System);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioInstrumentTypesQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.InstrumentTypes);
    }
}
