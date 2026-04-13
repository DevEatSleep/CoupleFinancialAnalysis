using CoupleChat.Models;
using Microsoft.AspNetCore.SignalR;

namespace CoupleChat.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string sender, string content, string avatarUrl)
    {
        var message = new Message
        {
            Sender = sender,
            Content = content,
            AvatarUrl = avatarUrl,
            CreatedAt = DateTime.UtcNow
        };

        // Broadcast to all connected clients
        await Clients.All.SendAsync("ReceiveMessage", message.Sender, message.Content, message.AvatarUrl, message.CreatedAt);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}
