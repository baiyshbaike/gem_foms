using Microsoft.AspNetCore.SignalR;

namespace Dialysis.Server.Hubs;

public class VerificationHub : Hub
{
    public async Task JoinVerificationGroup(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    }
}