using BookManagementApp.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.Controllers
{
    [Authorize]
    public class ExcerptController : Controller
    {
        private const string AdminRole = "SuperAdmin";
        private const long MaxImageBytes = 8 * 1024 * 1024; // 8 MB
        private const int PageSize = 12;

        public static readonly string[] AllowedEmojis = { "❤️", "👍", "📖", "🔥", "😂", "😢" };

        private readonly MyDbContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly IHubContext<BookManagementApp.Hubs.ChatHub> _hubContext;

        public ExcerptController(MyDbContext context, Cloudinary cloudinary,
            IHubContext<BookManagementApp.Hubs.ChatHub> hubContext)
        {
            _context = context;
            _cloudinary = cloudinary;
            _hubContext = hubContext;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        private static DateTime FeedCutoff => DateTime.Now.AddHours(-24);

        // Kitap Akışı: son 24 saatin kesitleri, PageSize'lık sayfalar halinde.
        // Devamı LoadMore ile sonsuz kaydırma olarak yüklenir.
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var page = await BuildFeedPageAsync(userId.Value, 0);

            // Kitap seçici modalı için: isimle arama ve yazar gösterimi
            ViewBag.MyBookOptions = await _context.Books
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.Name)
                .Select(b => new ExcerptBookOption(b.Id, b.Name, b.Author))
                .ToListAsync();

            ViewBag.HasMore = page.Excerpts.Count == PageSize;
            return View(page);
        }

        // Sonsuz kaydırma: sıradaki sayfanın kartlarını HTML olarak döndürür
        public async Task<IActionResult> LoadMore(int skip)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var page = await BuildFeedPageAsync(userId.Value, Math.Max(0, skip));

            Response.Headers["X-Has-More"] = page.Excerpts.Count == PageSize ? "1" : "0";
            return PartialView("_ExcerptCards", page);
        }

        private async Task<ExcerptFeedPageViewModel> BuildFeedPageAsync(int userId, int skip)
        {
            var excerpts = await _context.BookExcerpts
                .Include(e => e.User)
                .Where(e => e.CreatedAt >= FeedCutoff)
                .OrderByDescending(e => e.CreatedAt)
                .ThenByDescending(e => e.Id)
                .Skip(skip)
                .Take(PageSize)
                .ToListAsync();

            var excerptIds = excerpts.Select(e => e.Id).ToList();
            var reactions = await _context.ExcerptReactions
                .Where(r => excerptIds.Contains(r.ExcerptId))
                .ToListAsync();

            return new ExcerptFeedPageViewModel
            {
                Excerpts = excerpts,
                ReactionCounts = reactions
                    .GroupBy(r => r.ExcerptId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(r => r.Emoji).ToDictionary(e => e.Key, e => e.Count())),
                MyReactions = reactions
                    .Where(r => r.UserId == userId)
                    .ToDictionary(r => r.ExcerptId, r => r.Emoji),
                CurrentUserId = userId,
                IsAdmin = User.IsInRole(AdminRole)
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId, string? customTitle, string? customAuthor, string? caption, IFormFile? imageFile)
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            // Mobilde yer kaplamasın diye not 100 karakterle sınırlı
            if (caption != null && caption.Trim().Length > 100)
            {
                TempData["Error"] = "Not en fazla 100 karakter olabilir.";
                return RedirectToAction(nameof(Index));
            }

            // Kitap bilgisi iki yoldan gelebilir:
            //  1) Kendi rafındaki bir kitap (bookId > 0) — bilgiler kitaptan kopyalanır.
            //  2) "Diğer" — kitap rafta yoksa kullanıcı adı elle yazar (customTitle).
            string bookTitle;
            string? bookAuthor;
            if (bookId > 0)
            {
                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.UserId == userId);
                if (book == null)
                {
                    TempData["Error"] = "Lütfen kendi kitaplarınızdan birini seçin.";
                    return RedirectToAction(nameof(Index));
                }
                bookTitle = book.Name;
                bookAuthor = book.Author;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(customTitle))
                {
                    TempData["Error"] = "Lütfen bir kitap seçin ya da kitap adını yazın.";
                    return RedirectToAction(nameof(Index));
                }

                bookTitle = customTitle.Trim();
                if (bookTitle.Length > 150) bookTitle = bookTitle.Substring(0, 150);

                bookAuthor = string.IsNullOrWhiteSpace(customAuthor) ? null : customAuthor.Trim();
                if (bookAuthor != null && bookAuthor.Length > 200) bookAuthor = bookAuthor.Substring(0, 200);
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Kesit fotoğrafı zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            if (imageFile.Length > MaxImageBytes)
            {
                TempData["Error"] = "Fotoğraf en fazla 8 MB olabilir.";
                return RedirectToAction(nameof(Index));
            }

            if (imageFile.ContentType == null || !imageFile.ContentType.StartsWith("image/"))
            {
                TempData["Error"] = "Sadece resim dosyası yükleyebilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            string imageUrl, publicId;
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                    Folder = "excerpts",
                    Transformation = new Transformation().Width(1200).Crop("limit")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                    throw new Exception(uploadResult.Error.Message);

                imageUrl = uploadResult.SecureUrl.ToString();
                publicId = uploadResult.PublicId;
            }
            catch (Exception)
            {
                TempData["Error"] = "Fotoğraf yüklenirken hata oluştu, tekrar deneyin.";
                return RedirectToAction(nameof(Index));
            }

            var excerpt = new BookExcerpt
            {
                UserId = userId.Value,
                BookTitle = bookTitle,
                BookAuthor = bookAuthor,
                Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
                ImageUrl = imageUrl,
                ImagePublicId = publicId
            };

            _context.BookExcerpts.Add(excerpt);
            await _context.SaveChangesAsync();

            // Akışı açık tutan diğer kullanıcılara "yeni kesit var" bildirimi
            await _hubContext.Clients.All.SendAsync("ReceiveExcerpt", excerpt.Id);

            TempData["Success"] = "Kesitiniz paylaşıldı. 24 saat boyunca akışta kalacak.";
            return RedirectToAction(nameof(Index));
        }

        // Emoji tepkisi (AJAX). Aynı emojiye tekrar basmak tepkiyi kaldırır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> React(int id, string emoji)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            if (!AllowedEmojis.Contains(emoji)) return BadRequest();

            var excerpt = await _context.BookExcerpts
                .FirstOrDefaultAsync(e => e.Id == id && e.CreatedAt >= FeedCutoff);
            if (excerpt == null) return NotFound();

            var existing = await _context.ExcerptReactions
                .FirstOrDefaultAsync(r => r.ExcerptId == id && r.UserId == userId);

            string? myEmoji;
            if (existing == null)
            {
                _context.ExcerptReactions.Add(new ExcerptReaction
                {
                    ExcerptId = id,
                    UserId = userId.Value,
                    Emoji = emoji
                });
                myEmoji = emoji;
            }
            else if (existing.Emoji == emoji)
            {
                _context.ExcerptReactions.Remove(existing);
                myEmoji = null;
            }
            else
            {
                existing.Emoji = emoji;
                myEmoji = emoji;
            }

            await _context.SaveChangesAsync();

            var counts = await _context.ExcerptReactions
                .Where(r => r.ExcerptId == id)
                .GroupBy(r => r.Emoji)
                .Select(g => new { Emoji = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Emoji, g => g.Count);

            await _hubContext.Clients.All.SendAsync("ReceiveExcerptReaction", id, counts);

            return Json(new { counts, myEmoji });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int id, string? reason)
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var excerpt = await _context.BookExcerpts.FirstOrDefaultAsync(e => e.Id == id);
            if (excerpt == null)
            {
                TempData["Error"] = "Kesit bulunamadı (silinmiş olabilir).";
                return RedirectToAction(nameof(Index));
            }

            if (excerpt.UserId == userId)
            {
                TempData["Error"] = "Kendi kesitinizi şikayet edemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var alreadyReported = await _context.ExcerptReports
                .AnyAsync(r => r.ExcerptId == id && r.ReporterId == userId);
            if (!alreadyReported)
            {
                _context.ExcerptReports.Add(new ExcerptReport
                {
                    ExcerptId = id,
                    ReporterId = userId.Value,
                    ReportedUserId = excerpt.UserId,
                    ImageUrl = excerpt.ImageUrl,
                    Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
                });
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Şikayetiniz alındı, incelenecek.";
            return RedirectToAction(nameof(Index));
        }

        // Kendi kesitini (veya admin herkesinkini) siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var excerpt = await _context.BookExcerpts.FirstOrDefaultAsync(e => e.Id == id);
            if (excerpt == null) return NotFound();

            if (excerpt.UserId != userId && !User.IsInRole(AdminRole)) return Forbid();

            await DestroyImageAsync(excerpt.ImagePublicId);
            _context.BookExcerpts.Remove(excerpt);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kesit silindi.";
            return RedirectToAction(nameof(Index));
        }

        internal async Task DestroyImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return;
            try
            {
                await _cloudinary.DestroyAsync(new DeletionParams(publicId) { Invalidate = true });
            }
            catch (Exception)
            {
                // Cloudinary silme hatası kesit silmeyi engellemesin
            }
        }
    }
}
