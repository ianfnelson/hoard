using Hoard.Bus.Transactions;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Messages;
using Hoard.Messages.Transactions;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class TransactionChangePropagatorTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly TransactionChangePropagator _propagator;

    public TransactionChangePropagatorTests()
    {
        _propagator = new TransactionChangePropagator(_mediator.Object);
    }

    [Fact]
    public async Task Handle_TransactionCreated_DispatchesHoldingsForEachDayFromDateToToday()
    {
        var date = DateOnlyHelper.TodayLocal().AddDays(-2);

        await _propagator.Handle(new TransactionCreatedEvent(PipelineMode.DaytimeReactive, 1, date));

        // Should dispatch for date, date+1, and today (3 days)
        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingsCommand>(c => c.PipelineMode == PipelineMode.DaytimeReactive),
            default), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_TransactionDeleted_DispatchesHoldings()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new TransactionDeletedEvent(PipelineMode.DaytimeReactive, 1, today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingsCommand>(c => c.AsOfDate == today),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_TransactionUpdated_DispatchesHoldings()
    {
        var today = DateOnlyHelper.TodayLocal();

        await _propagator.Handle(new TransactionUpdatedEvent(PipelineMode.DaytimeReactive, 1, today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingsCommand>(c => c.AsOfDate == today),
            default), Times.Once);
    }
}
