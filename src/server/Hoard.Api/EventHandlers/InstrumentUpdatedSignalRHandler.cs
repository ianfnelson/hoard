using Hoard.Api.Hubs;
using Hoard.Messages.Performance;
using Hoard.Messages.Quotes;
using Microsoft.AspNetCore.SignalR;
using Rebus.Handlers;

namespace Hoard.Api.EventHandlers;

public sealed class InstrumentUpdatedSignalRHandler(IHubContext<InstrumentHub> hub)
    : IHandleMessages<QuoteChangedEvent>
{
    public async Task Handle(QuoteChangedEvent message)
    {
        var group = InstrumentHub.GroupName(message.InstrumentId);
        
        await hub.Clients.Group(group).SendAsync("InstrumentUpdated",
            new { instrumentId = message.InstrumentId });
    }
}