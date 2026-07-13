using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using BookManagementApp.Areas.Admin.Models; // For MyDbContext
using BookManagementApp.Models; // For ChatMessage
using System;
using System.Threading.Tasks;
using System.Linq;

namespace BookManagementApp.Hubs
{
    public class ChatHub : Hub
    {
        // Thread-safe dictionary: UserId -> thread-safe set of ConnectionIds
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> OnlineUsers = new();

        public static bool IsUserOnline(string userId)
        {
            return OnlineUsers.TryGetValue(userId, out var connections) && !connections.IsEmpty;
        }

        private readonly MyDbContext _context;

        public ChatHub(MyDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                var connections = OnlineUsers.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
                
                // Add connection
                connections.TryAdd(Context.ConnectionId, 0);
                
                // Notify friends or globally that this user is online
                await Clients.Others.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId) && OnlineUsers.TryGetValue(userId, out var connections))
            {
                connections.TryRemove(Context.ConnectionId, out _);
                if (connections.IsEmpty)
                {
                    OnlineUsers.TryRemove(userId, out _);
                    // Notify others that user is offline
                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTyping(string receiverIdStr)
        {
            var senderId = Context.UserIdentifier;
            if (OnlineUsers.TryGetValue(receiverIdStr, out var connections))
            {
                var connectionIds = connections.Keys.ToList();
                await Clients.Clients(connectionIds).SendAsync("ReceiveTyping", senderId);
            }
        }

        public async Task<object> SendMessage(string receiverIdStr, string content)
        {
            var senderIdStr = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderIdStr) || string.IsNullOrWhiteSpace(content)) 
                return new { success = false, message = "Geçersiz mesaj." };

            int senderId = int.Parse(senderIdStr);
            int receiverId = int.Parse(receiverIdStr);

            var msg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            var messageObj = new
            {
                id = msg.Id,
                senderId = msg.SenderId,
                content = msg.Content,
                createdAt = msg.CreatedAt.ToString("HH:mm")
            };

            if (OnlineUsers.TryGetValue(receiverIdStr, out var connections))
            {
                var connectionIds = connections.Keys.ToList();
                await Clients.Clients(connectionIds).SendAsync("ReceiveMessage", messageObj);
                
                // Eğer kullanıcı aktifse "İletildi" sinyali gönder (Çift Gri Tik)
                await Clients.Caller.SendAsync("MessageDelivered", msg.Id);
            }

            return new { success = true, message = messageObj };
        }

        public async Task MarkMessagesAsRead(string senderIdStr)
        {
            var receiverIdStr = Context.UserIdentifier;
            if (string.IsNullOrEmpty(receiverIdStr)) return;

            int receiverId = int.Parse(receiverIdStr);
            int senderId = int.Parse(senderIdStr);

            var unreadMessages = _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();

                if (OnlineUsers.TryGetValue(senderIdStr, out var senderConnections))
                {
                    var connectionIds = senderConnections.Keys.ToList();
                    await Clients.Clients(connectionIds).SendAsync("MessagesRead", receiverIdStr);
                }
            }
        }
    }
}
