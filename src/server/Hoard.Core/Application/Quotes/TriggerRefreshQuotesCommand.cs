using Hoard.Core.Domain.Entities;
using Hoard.Messages.Quotes;

namespace Hoard.Core.Application.Quotes;

public sealed record TriggerRefreshQuotesCommand : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.RefreshQuotes;
    public object ToBusCommand() => new RefreshQuotesBusCommand(RunId);
}
