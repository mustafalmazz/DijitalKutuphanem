using Microsoft.AspNetCore.Mvc;
using BookManagementApp.Models;
using BookManagementApp.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.Controllers
{
    public class MessageController : Controller
    {
        private readonly MyDbContext _context;

        public MessageController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            int uid = userId.Value;

            // Engellenen / engelleyen kullanıcı id'leri
            var blockedOrBlockingIds = _context.Blocks
                .Where(b => b.BlockerId == uid || b.BlockedId == uid)
                .Select(b => b.BlockerId == uid ? b.BlockedId : b.BlockerId)
                .ToList();

            // Her sohbet partneri için EN SON mesajın Id'si. Gruplama ve MAX artık
            // veritabanında yapılıyor (eskiden kullanıcının TÜM mesajları belleğe çekilip
            // orada gruplanıyordu). Id kimlik sütunu insertion sırasıyla arttığından,
            // bir partnerle olan en büyük Id = en yeni mesaj.
            var lastMessageIds = _context.ChatMessages
                .Where(m => m.SenderId == uid || m.ReceiverId == uid)
                .GroupBy(m => m.SenderId == uid ? m.ReceiverId : m.SenderId)
                .Select(g => g.Max(m => m.Id))
                .ToList();

            // Yalnızca bu son mesajları taraf bilgileriyle çek (partner sayısı kadar satır).
            var conversations = _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => lastMessageIds.Contains(m.Id))
                .OrderByDescending(m => m.CreatedAt)
                .ToList()
                // Engelli partnerler küçük sonuç kümesi üzerinde elenir (partner başına tek satır).
                .Where(m => !blockedOrBlockingIds.Contains(m.SenderId == uid ? m.ReceiverId : m.SenderId))
                .ToList();

            ViewBag.CurrentUserId = userId;
            return View(conversations);
        }

        public IActionResult Chat(int userId)
        {
            var myId = HttpContext.Session.GetInt32("UserId");
            if (myId == null) return RedirectToAction("Login", "Account");

            // Kendine sohbet açılmaz (hub da göndermeyi reddeder)
            if (userId == myId.Value) return RedirectToAction("Index");

            var otherUser = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (otherUser == null) return NotFound();

            var isBlocked = _context.Blocks.Any(b =>
                (b.BlockerId == myId.Value && b.BlockedId == userId) ||
                (b.BlockerId == userId && b.BlockedId == myId.Value));

            if (isBlocked)
            {
                return RedirectToAction("Index"); // Redirect to messages if blocked
            }

            // Mesajlaşma kuralı: taraflardan biri diğerini takip ediyor olmalı
            // (takip ettiğin VEYA seni takip eden). Bu iki durum dışında sohbet açılmaz.
            bool related = _context.Follows.Any(f => f.IsAccepted &&
                ((f.FollowerId == myId.Value && f.FollowingId == userId) ||
                 (f.FollowerId == userId && f.FollowingId == myId.Value)));
            if (!related)
            {
                TempData["ErrorMessage"] = "Yalnızca takip ettiğin ya da seni takip eden kişilerle mesajlaşabilirsin.";
                return RedirectToAction("PublicProfile", "Profile", new { id = userId });
            }

            ViewBag.OtherUser = otherUser;
            ViewBag.CurrentUserId = myId;
            ViewBag.IsOnline = BookManagementApp.Hubs.ChatHub.IsUserOnline(userId.ToString());

            // Mesajları çek
            var messages = _context.ChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == userId) || (m.SenderId == userId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt)
                .ToList();

            // NOT: Okundu işaretleme artık burada yapılmıyor. İstemci bağlanınca hub'daki
            // MarkMessagesAsRead çağrılıyor; o hem DB'de işaretliyor HEM de gönderene
            // "MessagesRead" sinyali gönderiyor. Burada sessizce işaretlersek, hub okunmamış
            // mesaj bulamayıp gönderene bildirim atamıyor ve tikler anlık maviye dönmüyordu.

            return View(messages);
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
