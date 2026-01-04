using Microsoft.AspNetCore.SignalR;

namespace Hoard.Api.Hubs;

public sealed class PortfolioHub : Hub
{
    internal const string HubPath = "/hubs/portfolio";
    
    public Task SubscribeToPortfolio(int portfolioId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(portfolioId));
    
    public Task UnsubscribeFromPortfolio(int portfolioId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(portfolioId));

    internal static string GroupName(int portfolioId) => $"portfolio:{portfolioId}";
}