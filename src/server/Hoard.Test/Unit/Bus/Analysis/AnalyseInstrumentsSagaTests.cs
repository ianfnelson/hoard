using Hoard.Bus.Analysis;
using Hoard.Core.Application;
using Hoard.Core.Application.Analysis;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Analysis;

public class AnalyseInstrumentsSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly AnalyseInstrumentsSaga _saga;

    public AnalyseInstrumentsSagaTests()
    {
        _saga = new AnalyseInstrumentsSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<AnalyseInstrumentsSaga>>())
        {
            Data = new AiSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_WithInstruments_DispatchesAnalysisAndTracksPending()
    {
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);
        var instrumentIds = new List<int> { 1, 2, 3 };

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForAnalysisQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForAnalysisQuery>(), default))
            .ReturnsAsync(instrumentIds);

        await _saga.Handle(new StartAnalyseInstrumentsSagaCommand(runId, PipelineMode.CloseOfDay, asOfDate));

        Assert.Equal(runId, _saga.Data.AnalysisRunId);
        Assert.Equal(PipelineMode.CloseOfDay, _saga.Data.PipelineMode);
        Assert.Equal(3, _saga.Data.PendingInstruments.Count);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchAnalyseInstrumentsCommand>(), default), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<InstrumentsAnalysedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoInstruments_CompletesImmediately()
    {
        var runId = Guid.NewGuid();

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForAnalysisQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForAnalysisQuery>(), default))
            .ReturnsAsync(new List<int>());

        await _saga.Handle(new StartAnalyseInstrumentsSagaCommand(runId, PipelineMode.CloseOfDay, new DateOnly(2024, 6, 15)));

        _bus.Verify(b => b.Publish(It.Is<InstrumentsAnalysedEvent>(e =>
            e.AnalysisRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task Handle_InstrumentAnalysed_RemovesPendingInstrument()
    {
        var runId = Guid.NewGuid();
        _saga.Data.AnalysisRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.CloseOfDay;
        _saga.Data.PendingInstruments = new HashSet<int> { 1, 2 };

        await _saga.Handle(new InstrumentAnalysedEvent(runId, PipelineMode.CloseOfDay, 1, new DateOnly(2024, 6, 15), false, false, false));

        Assert.Single(_saga.Data.PendingInstruments);
        Assert.Contains(2, _saga.Data.PendingInstruments);
        _bus.Verify(b => b.Publish(It.IsAny<InstrumentsAnalysedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastInstrumentAnalysed_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.AnalysisRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.CloseOfDay;
        _saga.Data.PendingInstruments = new HashSet<int> { 1 };

        await _saga.Handle(new InstrumentAnalysedEvent(runId, PipelineMode.CloseOfDay, 1, new DateOnly(2024, 6, 15), false, false, false));

        Assert.Empty(_saga.Data.PendingInstruments);
        _bus.Verify(b => b.Publish(It.Is<InstrumentsAnalysedEvent>(e =>
            e.AnalysisRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }
}
