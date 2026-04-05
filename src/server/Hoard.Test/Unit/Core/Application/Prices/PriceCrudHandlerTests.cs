using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages.Prices;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Prices;

public class CreatePriceCommandHandlerTests : IDisposable
{
    private readonly HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task HandleAsync_CreatesPriceAndPublishesPriceChangedEvent()
    {
        // Arrange
        _context.Instruments.Add(TestData.Instrument());
        await _context.SaveChangesAsync();

        var dto = new PriceWriteDto();
        var price = new Price
        {
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15),
            Close = 105m, AdjustedClose = 105m, Source = "Manual"
        };
        _mapper.Setup(m => m.Map<PriceWriteDto, Price>(dto)).Returns(price);

        var handler = new CreatePriceCommandHandler(_bus.Object, _mapper.Object, _context);

        // Act
        var id = await handler.HandleAsync(new CreatePriceCommand(dto));

        // Assert
        var saved = await _context.Prices.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(105m, saved.Close);
        _bus.Verify(b => b.Publish(It.Is<PriceChangedEvent>(e =>
            e.InstrumentId == 1 && e.AsOfDate == new DateOnly(2024, 1, 15)), null), Times.Once);
    }

    public void Dispose() => _context.Dispose();
}

public class UpdatePriceCommandHandlerTests : IDisposable
{
    private readonly HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task HandleAsync_UpdatesPriceAndPublishesEvent()
    {
        // Arrange
        _context.Instruments.Add(TestData.Instrument());
        _context.Prices.Add(TestData.Price());
        await _context.SaveChangesAsync();

        var dto = new PriceWriteDto();
        _mapper.Setup(m => m.Map<PriceWriteDto, Price>(dto, It.IsAny<Price>()));

        var handler = new UpdatePriceCommandHandler(_bus.Object, _mapper.Object, _context);
        var price = await _context.Prices.FirstAsync();

        // Act
        await handler.HandleAsync(new UpdatePriceCommand(price.Id, dto));

        // Assert
        _bus.Verify(b => b.Publish(It.IsAny<PriceChangedEvent>(), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPrice_Throws()
    {
        var handler = new UpdatePriceCommandHandler(_bus.Object, _mapper.Object, _context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.HandleAsync(new UpdatePriceCommand(999, new PriceWriteDto())));
    }

    public void Dispose() => _context.Dispose();
}

public class DeletePriceCommandHandlerTests : IDisposable
{
    private readonly HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IBus> _bus = new();

    [Fact]
    public async Task HandleAsync_RemovesPriceAndPublishesEvent()
    {
        // Arrange
        _context.Instruments.Add(TestData.Instrument());
        _context.Prices.Add(TestData.Price());
        await _context.SaveChangesAsync();

        var handler = new DeletePriceCommandHandler(_bus.Object, _context);
        var price = await _context.Prices.FirstAsync();

        // Act
        await handler.HandleAsync(new DeletePriceCommand(price.Id));

        // Assert
        Assert.Equal(0, await _context.Prices.CountAsync());
        _bus.Verify(b => b.Publish(It.Is<PriceChangedEvent>(e =>
            e.InstrumentId == 1), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPrice_Throws()
    {
        var handler = new DeletePriceCommandHandler(_bus.Object, _context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.HandleAsync(new DeletePriceCommand(999)));
    }

    public void Dispose() => _context.Dispose();
}

public class GetPriceQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingPrice_ReturnsDetail()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Instruments.Add(TestData.Instrument());
        context.Prices.Add(TestData.Price());
        await context.SaveChangesAsync();

        var handler = new GetPriceQueryHandler(context, NullLogger<GetPriceQueryHandler>.Instance);
        var price = await context.Prices.FirstAsync();

        // Act
        var result = await handler.HandleAsync(new GetPriceQuery(price.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(99m, result.Close);
        Assert.Equal("Test Stock", result.InstrumentName);
    }

    [Fact]
    public async Task HandleAsync_NonExistentPrice_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPriceQueryHandler(context, NullLogger<GetPriceQueryHandler>.Instance);

        var result = await handler.HandleAsync(new GetPriceQuery(999), CancellationToken.None);

        Assert.Null(result);
    }
}

public class GetInstrumentsForRefreshHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithSpecificInstrumentId_ReturnsOnlyThatInstrument()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        SeedInstrumentWithPriceFeed(context, 1, "Stock A");
        SeedInstrumentWithPriceFeed(context, 2, "Stock B");
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForRefreshHandler(context);

        // Act
        var result = await handler.HandleAsync(
            new GetInstrumentsForRefreshQuery(InstrumentId: 1));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    [Fact]
    public async Task HandleAsync_OnlyReturnsInstrumentsWithPriceDataFeed()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        SeedInstrumentWithPriceFeed(context, 1, "Stock A");
        // Instrument without price data feed
        context.Instruments.Add(new Instrument
        {
            Id = 2, Name = "Stock B", Ticker = "SB", CurrencyId = "GBP"
        });
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsForRefreshHandler(context);

        // Act
        var result = await handler.HandleAsync(
            new GetInstrumentsForRefreshQuery(null, Messages.PipelineMode.Backfill));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0]);
    }

    private static void SeedInstrumentWithPriceFeed(HoardContext context, int id, string name)
    {
        var instrument = new Instrument { Id = id, Name = name, Ticker = $"T{id}", CurrencyId = "GBP" };
        instrument.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = id,
            DataFeedId = DataFeed.Prices,
            Configuration = new DataFeedConfiguration { Ticker = $"T{id}" }
        });
        context.Instruments.Add(instrument);
    }
}
