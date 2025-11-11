using Hoard.Messages.Quotes;

namespace Hoard.Core.Application.Quotes;

public record TriggerRefreshQuotesCommand(Guid CorrelationId) : ITriggerCommand
{
    public object ToBusCommand() => new RefreshQuotesBusCommand(CorrelationId);
}
