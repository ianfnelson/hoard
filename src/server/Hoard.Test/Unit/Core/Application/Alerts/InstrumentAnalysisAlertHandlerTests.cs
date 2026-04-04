using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Test.Shared;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class InstrumentAnalysisAlertHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IAlertService> _alertService = new();
    private readonly InstrumentAnalysisAlertHandler _handler;

    public InstrumentAnalysisAlertHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new InstrumentAnalysisAlertHandler(_context, _alertService.Object);
    }

    private static DateOnly TodayIfWeekday()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        while (today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            today = today.AddDays(1);
        return today;
    }

    [Fact]
    public async Task HandleAsync_NotToday_DoesNotRaiseAlert()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, 1, yesterday, true, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Weekend_DoesNotRaiseAlert()
    {
        // Find next Saturday
        var date = DateOnly.FromDateTime(DateTime.Now);
        while (date.DayOfWeek != DayOfWeek.Saturday) date = date.AddDays(1);

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, 1, date, true, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_FiftyTwoWeekHigh_RaisesAlert()
    {
        var today = TodayIfWeekday();
        if (DateOnly.FromDateTime(DateTime.Now) != today) return;
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, today, true, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.FiftyTwoWeekHigh &&
            r.InstrumentId == instrument.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_FiftyTwoWeekLow_RaisesAlert()
    {
        var today = TodayIfWeekday();
        if (DateOnly.FromDateTime(DateTime.Now) != today) return;
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, today, false, true, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.FiftyTwoWeekLow), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AbnormalVolume_RaisesAlert()
    {
        var today = TodayIfWeekday();
        if (DateOnly.FromDateTime(DateTime.Now) != today) return;
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, today, false, false, true));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.AbnormalVolume), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AllFlagsTrue_RaisesThreeAlerts()
    {
        var today = TodayIfWeekday();
        if (DateOnly.FromDateTime(DateTime.Now) != today) return;
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, today, true, true, true));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_AllFlagsFalse_RaisesNoAlerts()
    {
        var today = TodayIfWeekday();
        if (DateOnly.FromDateTime(DateTime.Now) != today) return;
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, instrument.Id, today, false, false, false));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UnknownInstrument_DoesNotRaiseAlert()
    {
        var today = TodayIfWeekday();

        await _handler.HandleAsync(new InstrumentAnalysisAlertCommand(
            PipelineMode.CloseOfDay, 999, today, true, true, true));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
