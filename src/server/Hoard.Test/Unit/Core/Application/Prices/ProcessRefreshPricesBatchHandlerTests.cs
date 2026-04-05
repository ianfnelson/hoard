using System.Reflection;
using Hoard.Core.Application.Prices;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Prices;

// The handler's Include(InstrumentType) combined with ToJson() owned Configuration
// causes InMemory provider to drop the Configuration. We test HandleAsync guard
// paths (no DataFeedConfiguration needed) and business logic via reflection.

public class ProcessRefreshPricesBatchHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IPriceService> _priceService = new();
    private readonly ProcessRefreshPricesBatchHandler _handler;

    public ProcessRefreshPricesBatchHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new ProcessRefreshPricesBatchHandler(
            _bus.Object,
            _context,
            _priceService.Object,
            NullLogger<ProcessRefreshPricesBatchHandler>.Instance);
    }

    #region HandleAsync guard paths

    [Fact]
    public async Task HandleAsync_UnknownInstrument_ReturnsWithoutFetching()
    {
        await _handler.HandleAsync(new ProcessRefreshPricesBatchCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 999,
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));

        _priceService.Verify(s => s.GetPricesAsync(
            It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        _bus.Verify(b => b.Publish(It.IsAny<PriceRefreshedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoPriceFeed_ReturnsWithoutFetching()
    {
        var instrument = TestData.Instrument(id: 10);
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new ProcessRefreshPricesBatchCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, instrument.Id,
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));

        _priceService.Verify(s => s.GetPricesAsync(
            It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        // Unlike the quotes handler, the prices handler has no finally block — no events on early exit
        _bus.Verify(b => b.Publish(It.IsAny<PriceRefreshedEvent>(), null), Times.Never);
        _bus.Verify(b => b.Publish(It.IsAny<PriceChangedEvent>(), null), Times.Never);
    }

    #endregion

    #region UpsertPrices

    private async Task<List<DateOnly>> InvokeUpsertPrices(int instrumentId, IReadOnlyList<PriceDto> priceDtos, DateTime now)
    {
        var method = typeof(ProcessRefreshPricesBatchHandler)
            .GetMethod("UpsertPrices", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method 'UpsertPrices' not found — was it renamed?");
        return await (Task<List<DateOnly>>)method.Invoke(_handler, [instrumentId, priceDtos, now, CancellationToken.None])!;
    }

    [Fact]
    public async Task UpsertPrices_NewPrices_InsertsAndReturnsChangedDates()
    {
        var date1 = new DateOnly(2024, 1, 15);
        var date2 = new DateOnly(2024, 1, 16);
        var now = DateTime.UtcNow;

        var priceDtos = new List<PriceDto>
        {
            new() { Date = date1, Open = 100, High = 105, Low = 95, Close = 102, Volume = 1000, AdjustedClose = 102, Source = "Yahoo" },
            new() { Date = date2, Open = 103, High = 108, Low = 98, Close = 106, Volume = 1200, AdjustedClose = 106, Source = "Yahoo" }
        };

        var changed = await InvokeUpsertPrices(1, priceDtos, now);
        // UpsertPrices only adds to change tracker; SaveChanges verifies DB state
        await _context.SaveChangesAsync();

        Assert.Equal(2, changed.Count);
        Assert.Contains(date1, changed);
        Assert.Contains(date2, changed);

        var prices = await _context.Prices.ToListAsync();
        Assert.Equal(2, prices.Count);
        Assert.Equal(102m, prices.Single(p => p.AsOfDate == date1).Close);
        Assert.Equal(106m, prices.Single(p => p.AsOfDate == date2).Close);
    }

    [Fact]
    public async Task UpsertPrices_ExistingPriceWithNotableChange_UpdatesAndReturnsDate()
    {
        var date = new DateOnly(2024, 1, 15);
        _context.Prices.Add(new Price
        {
            InstrumentId = 1, AsOfDate = date, Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var changed = await InvokeUpsertPrices(1, new List<PriceDto>
        {
            new() { Date = date, Open = 98m, High = 102m, Low = 97m, Close = 105m, Volume = 1000, AdjustedClose = 105m, Source = "Yahoo" }
        }, now);

        Assert.Single(changed);
        var price = await _context.Prices.SingleAsync();
        Assert.Equal(105m, price.Close);
        Assert.Equal(105m, price.AdjustedClose);
        Assert.Equal(now, price.RetrievedUtc);
    }

    [Fact]
    public async Task UpsertPrices_ExistingPriceNoChange_ReturnsEmpty()
    {
        var date = new DateOnly(2024, 1, 15);
        _context.Prices.Add(new Price
        {
            InstrumentId = 1, AsOfDate = date, Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var changed = await InvokeUpsertPrices(1, new List<PriceDto>
        {
            new() { Date = date, Open = 98m, High = 102m, Low = 97m, Close = 100m, Volume = 1000, AdjustedClose = 100m, Source = "Yahoo" }
        }, DateTime.UtcNow);

        Assert.Empty(changed);
    }

    [Fact]
    public async Task UpsertPrices_LockedPrice_DoesNotUpdate()
    {
        var date = new DateOnly(2024, 1, 15);
        _context.Prices.Add(new Price
        {
            InstrumentId = 1, AsOfDate = date, Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo", IsLocked = true, RetrievedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var changed = await InvokeUpsertPrices(1, new List<PriceDto>
        {
            new() { Date = date, Open = 200m, High = 210m, Low = 190m, Close = 205m, Volume = 5000, AdjustedClose = 205m, Source = "Yahoo" }
        }, DateTime.UtcNow);

        Assert.Empty(changed);
        var price = await _context.Prices.SingleAsync();
        Assert.Equal(100m, price.Close);
    }

    [Fact]
    public async Task UpsertPrices_SpuriousPrice_DoesNotUpdate()
    {
        var date = new DateOnly(2024, 1, 15);
        _context.Prices.Add(new Price
        {
            InstrumentId = 1, AsOfDate = date, Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var changed = await InvokeUpsertPrices(1, new List<PriceDto>
        {
            new() { Date = date, Open = 198m, High = 210m, Low = 190m, Close = 200m, Volume = 5000, AdjustedClose = 200m, Source = "Yahoo" }
        }, DateTime.UtcNow);

        Assert.Empty(changed);
        var price = await _context.Prices.SingleAsync();
        Assert.Equal(100m, price.Close);
    }

    [Fact]
    public async Task UpsertPrices_DuplicateDates_DeduplicatesKeepingFirst()
    {
        var date = new DateOnly(2024, 1, 15);

        var changed = await InvokeUpsertPrices(1, new List<PriceDto>
        {
            new() { Date = date, Open = 100m, High = 105m, Low = 95m, Close = 102m, Volume = 1000, AdjustedClose = 102m, Source = "Yahoo" },
            new() { Date = date, Open = 200m, High = 210m, Low = 190m, Close = 999m, Volume = 2000, AdjustedClose = 999m, Source = "Yahoo" }
        }, DateTime.UtcNow);
        await _context.SaveChangesAsync();

        var price = await _context.Prices.SingleAsync();
        Assert.Equal(102m, price.Close);
        Assert.Single(changed);
    }

    [Fact]
    public async Task UpsertPrices_OnlyAdjustedCloseChanges_UpdatesButDoesNotReturnAsChanged()
    {
        var date = new DateOnly(2024, 1, 15);
        _context.Prices.Add(new Price
        {
            InstrumentId = 1, AsOfDate = date, Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var changed = await InvokeUpsertPrices(1, new List<PriceDto>
        {
            new() { Date = date, Open = 98m, High = 102m, Low = 97m, Close = 100m, Volume = 1000, AdjustedClose = 95m, Source = "Yahoo" }
        }, DateTime.UtcNow);

        Assert.Empty(changed);
        var price = await _context.Prices.SingleAsync();
        Assert.Equal(95m, price.AdjustedClose);
    }

    [Fact]
    public async Task UpsertPrices_OnlyVolumeChanges_UpdatesButDoesNotReturnAsChanged()
    {
        var date = new DateOnly(2024, 1, 15);
        _context.Prices.Add(new Price
        {
            InstrumentId = 1, AsOfDate = date, Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var changed = await InvokeUpsertPrices(1, new List<PriceDto>
        {
            new() { Date = date, Open = 98m, High = 102m, Low = 97m, Close = 100m, Volume = 5000, AdjustedClose = 100m, Source = "Yahoo" }
        }, DateTime.UtcNow);

        Assert.Empty(changed);
        var price = await _context.Prices.SingleAsync();
        Assert.Equal(5000, price.Volume);
    }

    #endregion

    #region IsSpuriousPrice

    private bool InvokeIsSpuriousPrice(Price price, PriceDto dto, int instrumentId)
    {
        var method = typeof(ProcessRefreshPricesBatchHandler)
            .GetMethod("IsSpuriousPrice", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method 'IsSpuriousPrice' not found — was it renamed?");
        return (bool)method.Invoke(_handler, [price, dto, instrumentId])!;
    }

    [Fact]
    public void IsSpuriousPrice_SmallChange_ReturnsFalse()
    {
        var price = new Price { Close = 100m, InstrumentId = 1, AsOfDate = DateOnly.MinValue, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow };
        var dto = new PriceDto { Close = 115m, Source = "Yahoo", Date = DateOnly.MinValue };

        Assert.False(InvokeIsSpuriousPrice(price, dto, 1));
    }

    [Fact]
    public void IsSpuriousPrice_LargeChange_ReturnsTrue()
    {
        var price = new Price { Close = 100m, InstrumentId = 1, AsOfDate = DateOnly.MinValue, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow };
        var dto = new PriceDto { Close = 125m, Source = "Yahoo", Date = DateOnly.MinValue };

        Assert.True(InvokeIsSpuriousPrice(price, dto, 1));
    }

    [Fact]
    public void IsSpuriousPrice_ExistingCloseIsZero_ReturnsFalse()
    {
        var price = new Price { Close = 0m, InstrumentId = 1, AsOfDate = DateOnly.MinValue, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow };
        var dto = new PriceDto { Close = 100m, Source = "Yahoo", Date = DateOnly.MinValue };

        Assert.False(InvokeIsSpuriousPrice(price, dto, 1));
    }

    [Fact]
    public void IsSpuriousPrice_LargeNegativeChange_ReturnsTrue()
    {
        var price = new Price { Close = 100m, InstrumentId = 1, AsOfDate = DateOnly.MinValue, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow };
        var dto = new PriceDto { Close = 70m, Source = "Yahoo", Date = DateOnly.MinValue };

        Assert.True(InvokeIsSpuriousPrice(price, dto, 1));
    }

    [Fact]
    public void IsSpuriousPrice_ExactlyAtThreshold_ReturnsFalse()
    {
        var price = new Price { Close = 100m, InstrumentId = 1, AsOfDate = DateOnly.MinValue, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow };
        var dto = new PriceDto { Close = 120m, Source = "Yahoo", Date = DateOnly.MinValue };

        Assert.False(InvokeIsSpuriousPrice(price, dto, 1));
    }

    [Fact]
    public void IsSpuriousPrice_JustAboveThreshold_ReturnsTrue()
    {
        var price = new Price { Close = 100m, InstrumentId = 1, AsOfDate = DateOnly.MinValue, Source = "Yahoo", RetrievedUtc = DateTime.UtcNow };
        var dto = new PriceDto { Close = 121m, Source = "Yahoo", Date = DateOnly.MinValue };

        Assert.True(InvokeIsSpuriousPrice(price, dto, 1));
    }

    #endregion

    #region TryUpdateExistingPrice

    private bool InvokeTryUpdateExistingPrice(Price price, PriceDto dto, DateTime now, int instrumentId)
    {
        var method = typeof(ProcessRefreshPricesBatchHandler)
            .GetMethod("TryUpdateExistingPrice", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method 'TryUpdateExistingPrice' not found — was it renamed?");
        return (bool)method.Invoke(_handler, [price, dto, now, instrumentId])!;
    }

    [Fact]
    public void TryUpdateExistingPrice_NotableChange_UpdatesFieldsAndReturnsTrue()
    {
        var price = new Price
        {
            Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Close = 105m, Open = 99m, High = 106m, Low = 96m,
            Volume = 1500, AdjustedClose = 105m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15)
        };
        var now = DateTime.UtcNow;

        var result = InvokeTryUpdateExistingPrice(price, dto, now, 1);

        Assert.True(result);
        Assert.Equal(105m, price.Close);
        Assert.Equal(99m, price.Open);
        Assert.Equal(1500, price.Volume);
        Assert.Equal(now, price.RetrievedUtc);
    }

    [Fact]
    public void TryUpdateExistingPrice_NoFieldsChanged_ReturnsFalse()
    {
        var price = new Price
        {
            Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15)
        };

        Assert.False(InvokeTryUpdateExistingPrice(price, dto, DateTime.UtcNow, 1));
    }

    [Fact]
    public void TryUpdateExistingPrice_Spurious_DoesNotUpdateAndReturnsFalse()
    {
        var price = new Price
        {
            Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Close = 200m, Open = 198m, High = 210m, Low = 190m,
            Volume = 5000, AdjustedClose = 200m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15)
        };

        var result = InvokeTryUpdateExistingPrice(price, dto, DateTime.UtcNow, 1);

        Assert.False(result);
        Assert.Equal(100m, price.Close);
    }

    [Fact]
    public void TryUpdateExistingPrice_OnlyNonCloseFieldsChanged_UpdatesButReturnsFalse()
    {
        var price = new Price
        {
            Close = 100m, Open = 98m, High = 102m, Low = 97m,
            Volume = 1000, AdjustedClose = 100m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Close = 100m, Open = 99m, High = 103m, Low = 96m,
            Volume = 2000, AdjustedClose = 100m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15)
        };

        var result = InvokeTryUpdateExistingPrice(price, dto, DateTime.UtcNow, 1);

        Assert.False(result);
        Assert.Equal(99m, price.Open);
        Assert.Equal(2000, price.Volume);
    }

    #endregion

    #region Static helper tests

    private static bool InvokeHasAnyFieldChanged(Price price, PriceDto dto)
    {
        var method = typeof(ProcessRefreshPricesBatchHandler)
            .GetMethod("HasAnyFieldChanged", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method 'HasAnyFieldChanged' not found — was it renamed?");
        return (bool)method.Invoke(null, [price, dto])!;
    }

    private static bool InvokeHasNotableChange(Price price, PriceDto dto)
    {
        var method = typeof(ProcessRefreshPricesBatchHandler)
            .GetMethod("HasNotableChange", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method 'HasNotableChange' not found — was it renamed?");
        return (bool)method.Invoke(null, [price, dto])!;
    }

    [Fact]
    public void HasAnyFieldChanged_AllFieldsSame_ReturnsFalse()
    {
        var price = new Price
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            Date = new DateOnly(2024, 1, 15)
        };

        Assert.False(InvokeHasAnyFieldChanged(price, dto));
    }

    [Fact]
    public void HasAnyFieldChanged_CloseDiffers_ReturnsTrue()
    {
        var price = new Price
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Open = 100m, High = 105m, Low = 95m, Close = 103m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            Date = new DateOnly(2024, 1, 15)
        };

        Assert.True(InvokeHasAnyFieldChanged(price, dto));
    }

    [Fact]
    public void HasAnyFieldChanged_VolumeDiffers_ReturnsTrue()
    {
        var price = new Price
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 2000, AdjustedClose = 102m, Source = "Yahoo",
            Date = new DateOnly(2024, 1, 15)
        };

        Assert.True(InvokeHasAnyFieldChanged(price, dto));
    }

    [Fact]
    public void HasNotableChange_CloseChanged_ReturnsTrue()
    {
        var price = new Price
        {
            Close = 100m, InstrumentId = 1,
            AsOfDate = new DateOnly(2024, 1, 15), Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto { Close = 105m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15) };

        Assert.True(InvokeHasNotableChange(price, dto));
    }

    [Fact]
    public void HasNotableChange_CloseUnchanged_ReturnsFalse()
    {
        var price = new Price
        {
            Close = 100m, InstrumentId = 1,
            AsOfDate = new DateOnly(2024, 1, 15), Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto { Close = 100m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15) };

        Assert.False(InvokeHasNotableChange(price, dto));
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
