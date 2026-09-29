using Microsoft.AspNetCore.SignalR;

namespace WhatDoYouMeme.Api.Hubs;

public class GameHub : Hub
{
    public async Task JoinGame(string joinCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, joinCode);
    }

    public async Task LeaveGame(string joinCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, joinCode);
    }
}