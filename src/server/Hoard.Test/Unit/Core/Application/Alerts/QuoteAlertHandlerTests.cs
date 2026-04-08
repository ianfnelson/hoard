using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class QuoteAlertHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IAlertService> _alertService = new();
    private readonly FakeTimeProvider _timeProvider = new();

    public QuoteAlertHandlerTests()
    {
        _context = TestContextFactory.Create();
    }

    private QuoteAlertHandler CreateHandler() =>
        new(_context, _alertService.Object, _timeProvider);

    private void SetUkTime(int year, int month, int day, int hour, int minute)
    {
        // FakeTimeProvider works in UTC. Convert the desired UK local time to UTC
        // so the handler's UK timezone conversion produces the expected time.
        var ukDateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukDateTime, Hoard.Core.DateOnlyHelper.UkTimeZone);
        _timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));
    }

    private async Task SeedInstrumentWithVolatility(int instrumentId = 1, decimal volatility = 1.0m)
    {
        _context.Instruments.Add(TestData.Instrument(id: instrumentId));
        _context.InstrumentAnalyses.Add(new InstrumentAnalysis
        {
            InstrumentId = instrumentId,
            AsOfDate = new DateOnly(2026, 4, 8),
            FiftyTwoWeekVolatility = volatility,
            UpdatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_BeforeMarketHours_DoesNotRaise()
    {
        SetUkTime(2026, 4, 8, 8, 18); // Wednesday 08:18 — before 09:00
        await SeedInstrumentWithVolatility();

        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 20.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AfterMarketHours_DoesNotRaise()
    {
        SetUkTime(2026, 4, 8, 16, 30); // Wednesday 16:30 — at/after cutoff
        await SeedInstrumentWithVolatility();

        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 20.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AtMarketOpen_Raises()
    {
        SetUkTime(2026, 4, 8, 9, 0); // Wednesday 09:00 — exactly at open
        await SeedInstrumentWithVolatility(volatility: 1.0m);

        // 20% move exceeds 2.5 * 1.0% = 2.5% threshold
        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 20.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Weekend_DoesNotRaise()
    {
        SetUkTime(2026, 4, 11, 10, 0); // Saturday 10:00
        await SeedInstrumentWithVolatility();

        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 20.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoInstrument_DoesNotRaise()
    {
        SetUkTime(2026, 4, 8, 10, 0); // Wednesday, during market hours

        await CreateHandler().HandleAsync(new QuoteAlertCommand(999, 10.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoAnalysis_DoesNotRaise()
    {
        SetUkTime(2026, 4, 8, 10, 0);
        _context.Instruments.Add(TestData.Instrument());
        await _context.SaveChangesAsync();

        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 10.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ZeroVolatility_DoesNotRaise()
    {
        SetUkTime(2026, 4, 8, 10, 0);
        await SeedInstrumentWithVolatility(volatility: 0m);

        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 5.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ChangeWithinThreshold_DoesNotRaise()
    {
        SetUkTime(2026, 4, 8, 10, 0);
        await SeedInstrumentWithVolatility(volatility: 5.0m);

        // 10% change, threshold is 2.5 * 5% = 12.5% — within threshold
        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 10.0m));

        _alertService.Verify(
            a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ChangeExceedsThreshold_RaisesWithCorrectDirection()
    {
        SetUkTime(2026, 4, 8, 10, 0);
        await SeedInstrumentWithVolatility(volatility: 2.0m);

        // 15% move exceeds 2.5 * 2% = 5% threshold
        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, 15.0m));

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r =>
                r.Title == "TEST up 15.00%" &&
                r.AlertTypeId == AlertType.QuoteMovement),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NegativeChange_RaisesDown()
    {
        SetUkTime(2026, 4, 8, 10, 0);
        await SeedInstrumentWithVolatility(volatility: 2.0m);

        await CreateHandler().HandleAsync(new QuoteAlertCommand(1, -15.0m));

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r =>
                r.Title == "TEST down 15.00%"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
