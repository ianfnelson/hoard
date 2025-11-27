using Hoard.Messages;
using Hoard.Messages.Quotes;

namespace Hoard.Core.Application.Quotes;

public record TriggerRefreshQuotesCommand(Guid CorrelationId, PipelineMode PipelineMode) : ITriggerCommand
{
    public object ToBusCommand() => new RefreshQuotesBusCommand(CorrelationId, PipelineMode);
}
