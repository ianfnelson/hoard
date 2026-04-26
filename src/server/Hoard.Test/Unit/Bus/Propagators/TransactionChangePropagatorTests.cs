using Hoard.Bus.Transactions;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Messages;
using Hoard.Messages.Transactions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class TransactionChangePropagatorTests
{
    private static readonly DateOnly Today = new(2024, 6, 15);

    private readonly Mock<IMediator> _mediator = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly TransactionChangePropagator _propagator;

    public TransactionChangePropagatorTests()
    {
        var ukNoon = new DateTime(Today.Year, Today.Month, Today.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukNoon, DateOnlyHelper.UkTimeZone);
        _timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));

        _propagator = new TransactionChangePropagator(_mediator.Object, _timeProvider);
    }

    [Fact]
    public async Task Handle_TransactionCreated_DispatchesHoldingsForEachDayFromDateToToday()
    {
        var date = Today.AddDays(-2);

        await _propagator.Handle(new TransactionCreatedEvent(PipelineMode.DaytimeReactive, 1, date));

        // Should dispatch for date, date+1, and today (3 days)
        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingsCommand>(c => c.PipelineMode == PipelineMode.DaytimeReactive),
            default), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_TransactionDeleted_DispatchesHoldings()
    {
        await _propagator.Handle(new TransactionDeletedEvent(PipelineMode.DaytimeReactive, 1, Today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingsCommand>(c => c.AsOfDate == Today),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_TransactionUpdated_DispatchesHoldings()
    {
        await _propagator.Handle(new TransactionUpdatedEvent(PipelineMode.DaytimeReactive, 1, Today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingsCommand>(c => c.AsOfDate == Today),
            default), Times.Once);
    }
}
