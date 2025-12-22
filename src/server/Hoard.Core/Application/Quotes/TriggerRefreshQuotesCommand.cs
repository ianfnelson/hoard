using Hoard.Messages.Quotes;

namespace Hoard.Core.Application.Quotes;

public sealed record TriggerRefreshQuotesCommand : ITriggerCommand
{
    public object ToBusCommand() => new RefreshQuotesBusCommand();
}
