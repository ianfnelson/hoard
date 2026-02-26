using Hoard.Core.Application;
using Hoard.Core.Application.Analysis;
using Hoard.Messages.Analysis;
using Rebus.Handlers;

namespace Hoard.Bus.Analysis;

public class AnalyseInstrumentsCommandHandler(IMediator mediator) :
    IHandleMessages<AnalyseInstrumentBatchBusCommand>
{
    public async Task Handle(AnalyseInstrumentBatchBusCommand message)
    {
        var appCommand = new ProcessAnalyseInstrumentCommand(
            message.AnalysisRunId,
            message.PipelineMode,
            message.InstrumentId,
            message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }
}
