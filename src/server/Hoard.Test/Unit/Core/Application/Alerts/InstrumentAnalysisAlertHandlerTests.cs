using Hoard.Core;
using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Test.Shared;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class InstrumentAnalysisAlertHandlerTests : IDisposable
{
    private static readonly DateOnly Wednesday = new(2026, 4, 8);
    private static readonly DateOnly Saturday = new(2026, 4, 11);

    private readonly HoardContext _context;
    private readonly Mock<IAlertService> _alertService = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly InstrumentAnalysisAlertHandler _handler;

    public InstrumentAnalysisAlertHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new InstrumentAnalysisAlertHandler(_context, _alertService.Object, _timeProvider);
    }

    private void SetUkDate(DateOnly date)
    {
        var ukNoon = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukNoon, DateOnlyHelper.UkTimeZone);
        _timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));
    }

    [Fact]
    public async Task HandleAsync_NotToday_DoesNotRaiseAlert()
    {
        SetUkDate(Wednesday);

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, 1, Wednesday.AddDays(-1), true, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Weekend_DoesNotRaiseAlert()
    {
        SetUkDate(Saturday);

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, 1, Saturday, true, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_FiftyTwoWeekHigh_RaisesAlert()
    {
        SetUkDate(Wednesday);
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, Wednesday, true, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.FiftyTwoWeekHigh &&
            r.InstrumentId == instrument.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_FiftyTwoWeekLow_RaisesAlert()
    {
        SetUkDate(Wednesday);
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, Wednesday, false, true, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.FiftyTwoWeekLow), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AbnormalVolume_RaisesAlert()
    {
        SetUkDate(Wednesday);
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, Wednesday, false, false, true));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.AbnormalVolume), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AllFlagsTrue_RaisesThreeAlerts()
    {
        SetUkDate(Wednesday);
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, Wednesday, true, true, true));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_AllFlagsFalse_RaisesNoAlerts()
    {
        SetUkDate(Wednesday);
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, Wednesday, false, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UnknownInstrument_DoesNotRaiseAlert()
    {
        SetUkDate(Wednesday);

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, 999, Wednesday, true, true, true));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
