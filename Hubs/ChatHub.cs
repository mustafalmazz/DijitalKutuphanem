using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using BookManagementApp.Areas.Admin.Models; // For MyDbContext
using BookManagementApp.Models; // For ChatMessage
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BookManagementApp.Hubs
{
    // Hub yalnızca kimliği doğrulanmış kullanıcılara açık; aksi halde Context.UserIdentifier
    // null olur ve int.Parse patlardı.
    [Authorize]
    public class ChatHub : Hub
    {
        // Thread-safe dictionary: UserId -> thread-safe set of ConnectionIds
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> OnlineUsers = new();

        // İçerik için makul üst sınır (istemci maxlength=1000 ile uyumlu, sunucu tarafı da korur)
        private const int MaxContentLength = 1000;

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

                // Çevrimdışı -> çevrimiçi geçişi yalnızca ilk bağlantıda bildirilir
                // (aynı kullanıcının ek sekmeleri tekrar tekrar "online" yaymasın).
                bool wasOffline = connections.IsEmpty;
                connections.TryAdd(Context.ConnectionId, 0);

                if (wasOffline)
                {
                    await NotifyConversationPartnersAsync(userId, "UserOnline");
                }
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
                    // Yalnızca sohbet ettiği kişilerden çevrimiçi olanlara bildir
                    await NotifyConversationPartnersAsync(userId, "UserOffline");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTyping(string receiverIdStr)
        {
            var senderId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderId)) return;

            // Engellenen/gizli ilişkilerde "yazıyor" sinyali de sızmasın
            if (int.TryParse(senderId, out var sId) && int.TryParse(receiverIdStr, out var rId))
            {
                if (await CanMessageAsync(sId, rId) != null) return;
            }

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

            if (!int.TryParse(senderIdStr, out var senderId) || !int.TryParse(receiverIdStr, out var receiverId))
                return new { success = false, message = "Geçersiz kullanıcı." };

            // Engel + gizlilik kontrolü artık gönderme yolunun tek otoritesi.
            // (Sayfa açılışındaki kontrol tek başına yeterli değildi: açık kalan sekme
            //  ya da doğrudan hub çağrısı tüm kuralları atlıyordu.)
            var guard = await CanMessageAsync(senderId, receiverId);
            if (guard != null)
                return new { success = false, message = guard };

            content = content.Trim();
            if (content.Length > MaxContentLength)
                content = content.Substring(0, MaxContentLength);

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

                // Alıcı çevrimiçiyse gönderene "İletildi" sinyali (çift gri tik)
                await Clients.Caller.SendAsync("MessageDelivered", msg.Id);
            }

            return new { success = true, message = messageObj };
        }

        public async Task MarkMessagesAsRead(string senderIdStr)
        {
            var receiverIdStr = Context.UserIdentifier;
            if (string.IsNullOrEmpty(receiverIdStr)) return;
            if (!int.TryParse(receiverIdStr, out var receiverId) || !int.TryParse(senderIdStr, out var senderId)) return;

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

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

        /// <summary>
        /// Gönderenin alıcıya mesaj atmasına engel bir durum varsa Türkçe hata metni,
        /// yoksa null döner. Kural: kendine mesaj yok, engelli değil ve taraflardan biri
        /// diğerini takip ediyor olmalı (ben onu takip ediyorum VEYA o beni takip ediyor).
        /// Bu iki durum dışında (gizli/açık fark etmez) mesajlaşılamaz.
        /// </summary>
        private async Task<string?> CanMessageAsync(int senderId, int receiverId)
        {
            if (senderId == receiverId)
                return "Kendinize mesaj gönderemezsiniz.";

            bool blocked = await _context.Blocks.AnyAsync(b =>
                (b.BlockerId == senderId && b.BlockedId == receiverId) ||
                (b.BlockerId == receiverId && b.BlockedId == senderId));
            if (blocked)
                return "Bu kullanıcıyla mesajlaşamazsınız.";

            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == receiverId);
            if (receiver == null)
                return "Kullanıcı bulunamadı.";

            // İki yönden biri yeterli: takip ettiğin ya da seni takip eden biri.
            bool related = await _context.Follows.AnyAsync(f => f.IsAccepted &&
                ((f.FollowerId == senderId && f.FollowingId == receiverId) ||
                 (f.FollowerId == receiverId && f.FollowingId == senderId)));
            if (!related)
                return "Yalnızca takip ettiğin ya da seni takip eden kişilerle mesajlaşabilirsin.";

            return null;
        }

        /// <summary>
        /// Çevrimiçi/çevrimdışı durumunu yalnızca kullanıcının sohbet ettiği kişilerden
        /// o an bağlı olanlara bildirir (eskiden herkese global yayın yapılıyordu).
        /// </summary>
        private async Task NotifyConversationPartnersAsync(string userIdStr, string eventName)
        {
            if (!int.TryParse(userIdStr, out var uid)) return;

            var partnerIds = await _context.ChatMessages
                .Where(m => m.SenderId == uid || m.ReceiverId == uid)
                .Select(m => m.SenderId == uid ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            if (partnerIds.Count == 0) return;

            var targetConnections = new List<string>();
            foreach (var pid in partnerIds)
            {
                if (OnlineUsers.TryGetValue(pid.ToString(), out var conns))
                    targetConnections.AddRange(conns.Keys);
            }

            if (targetConnections.Count > 0)
                await Clients.Clients(targetConnections).SendAsync(eventName, userIdStr);
        }
    }
}
