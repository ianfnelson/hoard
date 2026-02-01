using Microsoft.AspNetCore.SignalR;

namespace Hoard.Api.Hubs;

public sealed class InstrumentHub : Hub
{
    internal const string HubPath = "/hubs/instrument";
    
    public Task SubscribeToInstrument(int instrumentId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(instrumentId));
    
    public Task UnsubscribeFromInstrument(int instrumentId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(instrumentId));

    internal static string GroupName(int instrumentId) => $"portfolio:{instrumentId}";
}