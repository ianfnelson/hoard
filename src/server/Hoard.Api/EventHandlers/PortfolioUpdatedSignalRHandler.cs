using Hoard.Api.Hubs;
using Hoard.Core.Application;
using Hoard.Core.Application.Portfolios;
using Hoard.Messages.News;
using Hoard.Messages.Performance;
using Microsoft.AspNetCore.SignalR;
using Rebus.Handlers;

namespace Hoard.Api.EventHandlers;

public sealed class PortfolioUpdatedSignalRHandler(
    IHubContext<PortfolioHub> hub,
    IMediator mediator)
    : IHandleMessages<PortfolioPerformanceCalculatedEvent>,
      IHandleMessages<NewsArticleCreatedEvent>
{
    public async Task Handle(PortfolioPerformanceCalculatedEvent message)
    {
        var group = PortfolioHub.GroupName(message.PortfolioId);

        await hub.Clients.Group(group).SendAsync("PortfolioUpdated",
            new { portfolioId = message.PortfolioId });
    }

    public async Task Handle(NewsArticleCreatedEvent message)
    {
        var portfolioIds = await mediator
            .QueryAsync<GetPortfolioIdsForInstrumentQuery, IReadOnlyList<int>>(
                new GetPortfolioIdsForInstrumentQuery(message.InstrumentId));

        foreach (var portfolioId in portfolioIds)
        {
            var group = PortfolioHub.GroupName(portfolioId);

            await hub.Clients.Group(group).SendAsync("PortfolioUpdated",
                new { portfolioId });
        }
    }
}