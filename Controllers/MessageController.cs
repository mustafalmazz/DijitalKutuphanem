using Microsoft.AspNetCore.Mvc;
using BookManagementApp.Models;
using BookManagementApp.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.SignalR;

namespace BookManagementApp.Controllers
{
    public class MessageController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IHubContext<BookManagementApp.Hubs.ChatHub> _hubContext;

        public MessageController(MyDbContext context, IHubContext<BookManagementApp.Hubs.ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Blocked or blocking IDs
            var blockedOrBlockingIds = _context.Blocks
                .Where(b => b.BlockerId == userId.Value || b.BlockedId == userId.Value)
                .Select(b => b.BlockerId == userId.Value ? b.BlockedId : b.BlockerId)
                .ToList();

            // Kullanıcının daha önce mesajlaştığı kişileri bul (En son mesaj atanlar üstte)
            var conversations = _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToList()
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Where(g => !blockedOrBlockingIds.Contains(g.Key))
                .Select(g => g.First())
                .ToList();

            ViewBag.CurrentUserId = userId;
            return View(conversations);
        }

        public IActionResult Chat(int userId)
        {
            var myId = HttpContext.Session.GetInt32("UserId");
            if (myId == null) return RedirectToAction("Login", "Account");

            var otherUser = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (otherUser == null) return NotFound();

            var isBlocked = _context.Blocks.Any(b => 
                (b.BlockerId == myId.Value && b.BlockedId == userId) || 
                (b.BlockerId == userId && b.BlockedId == myId.Value));

            if (isBlocked)
            {
                return RedirectToAction("Index"); // Redirect to messages if blocked
            }

            if (otherUser.IsPrivate && otherUser.Id != myId.Value)
            {
                var isFollowing = _context.Follows.Any(f => f.FollowerId == myId.Value && f.FollowingId == userId && f.IsAccepted);
                if (!isFollowing)
                {
                    TempData["ErrorMessage"] = "Bu hesap gizli olduğu için mesaj gönderemezsiniz. Lütfen önce takip edin.";
                    return RedirectToAction("PublicProfile", "Profile", new { id = userId });
                }
            }

            ViewBag.OtherUser = otherUser;
            ViewBag.CurrentUserId = myId;
            ViewBag.IsOnline = BookManagementApp.Hubs.ChatHub.IsUserOnline(userId.ToString());

            // Mesajları çek
            var messages = _context.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == userId) || (m.SenderId == userId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            // Okunmamışları okundu yap
            var unreadMessages = messages.Where(m => m.ReceiverId == myId && !m.IsRead).ToList();
            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                _context.SaveChanges();
            }

            return View(messages);
        }

        [HttpPost]
        public IActionResult SendMessage(int receiverId, string content)
        {
            var senderId = HttpContext.Session.GetInt32("UserId");
            if (senderId == null) return Json(new { success = false, message = "Oturum kapalı." });
            if (string.IsNullOrWhiteSpace(content)) return Json(new { success = false, message = "Boş mesaj gönderilemez." });

            var otherUser = _context.Users.FirstOrDefault(u => u.Id == receiverId);
            if (otherUser != null && otherUser.IsPrivate && otherUser.Id != senderId.Value)
            {
                var isFollowing = _context.Follows.Any(f => f.FollowerId == senderId.Value && f.FollowingId == receiverId && f.IsAccepted);
                if (!isFollowing)
                {
                    return Json(new { success = false, message = "Bu hesap gizli olduğu için mesaj gönderemezsiniz." });
                }
            }

            var msg = new ChatMessage
            {
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                Content = content,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.ChatMessages.Add(msg);
            _context.SaveChanges();

            return Json(new { success = true, messageId = msg.Id, content = msg.Content, createdAt = msg.CreatedAt.ToString("HH:mm") });
        }

        [HttpGet]
        public IActionResult GetNewMessages(int receiverId, int lastMessageId)
        {
            var myId = HttpContext.Session.GetInt32("UserId");
            if (myId == null) return Json(new { success = false });

            var newMessages = _context.ChatMessages
                .Where(m => m.Id > lastMessageId && ((m.SenderId == myId && m.ReceiverId == receiverId) || (m.SenderId == receiverId && m.ReceiverId == myId)))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new {
                    id = m.Id,
                    senderId = m.SenderId,
                    content = m.Content,
                    createdAt = m.CreatedAt.ToString("HH:mm")
                })
                .ToList();

            // Bize gelenleri okundu olarak işaretle
            var unreadIds = newMessages.Where(m => m.senderId == receiverId).Select(m => m.id).ToList();
            if (unreadIds.Any())
            {
                var unreadMsgs = _context.ChatMessages.Where(m => unreadIds.Contains(m.Id)).ToList();
                foreach(var u in unreadMsgs) u.IsRead = true;
                _context.SaveChanges();
            }

            return Json(new { success = true, messages = newMessages });
        }

        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            var myId = HttpContext.Session.GetInt32("UserId");
            if (myId == null) return Json(0);

            var count = _context.ChatMessages.Count(m => m.ReceiverId == myId && !m.IsRead);
            return Json(count);
        }
    }
}
