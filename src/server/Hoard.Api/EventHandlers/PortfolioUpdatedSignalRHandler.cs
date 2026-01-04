using Hoard.Api.Hubs;
using Hoard.Messages.Performance;
using Microsoft.AspNetCore.SignalR;
using Rebus.Handlers;

namespace Hoard.Api.EventHandlers;

public sealed class PortfolioUpdatedSignalRHandler(IHubContext<PortfolioHub> hub)
    : IHandleMessages<PortfolioPerformanceCalculatedEvent>
{
    public async Task Handle(PortfolioPerformanceCalculatedEvent message)
    {
        var group = PortfolioHub.GroupName(message.PortfolioId);
        
        await hub.Clients.Group(group).SendAsync("PortfolioUpdated",
            new { portfolioId = message.PortfolioId, });
    }
}