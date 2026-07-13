using Microsoft.AspNetCore.Mvc;
using BookManagementApp.Areas.Admin.Models;
using BookManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BookManagementApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ProfileController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // --- 1. YILLIK HEDEF GÜNCELLEME ---
        [HttpPost]
        public IActionResult UpdateYearlyGoal([FromForm] int goal)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum kapalı. Lütfen tekrar giriş yapın." });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.YearlyReadingGoal = goal;
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Kullanıcı veritabanında bulunamadı." });
        }

        // --- 2. AYLIK HEDEF VE OKUNAN SAYFA GÜNCELLEME (GÜNCELLENDİ) ---
        // Artık hem hedefi hem de okunan sayfa sayısını manuel alıyor.
        [HttpPost]
        public IActionResult UpdateMonthlyGoal([FromForm] int goal, [FromForm] int readPages)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                // Hem hedefi hem de manuel girilen okunan sayfayı kaydediyoruz
                user.MonthlyReadingGoal = goal;
                user.MonthlyPagesRead = readPages;

                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Kullanıcı veritabanında bulunamadı." });
        }

        public IActionResult Index()
        {
            // 1. Oturum Kontrolü
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // 2. Kullanıcıyı Çek
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            // 3. Kullanıcının Kitaplarını Çek
            var userBooks = _context.Books
                                    .Include(b => b.Category)
                                    .Where(b => b.UserId == userId)
                                    .ToList();

            // 4. İstatistikler: Favori Kategori Hesaplama
            var favCategoryGroup = userBooks
                                    .GroupBy(b => b.Category != null ? b.Category.CategoryName : "Diğer")
                                    .OrderByDescending(g => g.Count())
                                    .FirstOrDefault();

            string favCategoryName = favCategoryGroup != null ? favCategoryGroup.Key : "Henüz Yok";

            var now = DateTime.Now;

            // =========================================================
            // YENİ EKLENEN KISIM: ÇALIŞMA (POMODORO) İSTATİSTİKLERİ
            // =========================================================

            // Kullanıcının tüm çalışma verilerini veritabanından çek (Listeye çevirki bellekte işlesin)
            var userSessions = _context.StudySessions
                                       .Where(s => s.UserId == userId)
                                       .ToList();

            // Bugün çalışılan toplam dakika (CreatedAt tarihinin sadece Gün/Ay/Yıl kısmına bakıyoruz)
            int todayStudyMins = userSessions
                .Where(s => s.CreatedAt.Date == now.Date)
                .Sum(s => s.DurationInMinutes);

            // Bu ay çalışılan toplam dakika (Aynı yıl ve aynı ayda olanları topla)
            int monthStudyMins = userSessions
                .Where(s => s.CreatedAt.Year == now.Year && s.CreatedAt.Month == now.Month)
                .Sum(s => s.DurationInMinutes);

            // Başarıyla tamamlanmış Pomodoro sayısı
            int completedPomodoros = userSessions
                .Count(s => s.SessionType == "Pomodoro" && s.IsCompleted);

            // =========================================================

            // 5. ViewModel'i Doldur
            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                Role = user.Role ?? "Üye",
                JoinDate = user.CreateDate,
                ProfileImageUrl = user.ProfileImageUrl,

                TotalBooks = userBooks.Count,
                TotalCategories = userBooks.Select(b => b.CategoryId).Distinct().Count(),
                TotalPagesRead = (int)userBooks.Sum(b => b.TotalPages ?? 0),
                TotalMoneySpent = userBooks.Sum(b => b.Price ?? 0),

                FavoriteCategory = favCategoryName,

                BooksReadThisYear = userBooks.Count(b => b.CreateDate.Year == now.Year),
                YearlyReadingGoal = user.YearlyReadingGoal,

                MonthlyReadingGoal = user.MonthlyReadingGoal,
                MonthlyPagesRead = user.MonthlyPagesRead,

                // --- YENİ EKLENEN DEĞERLERİ MODELE AKTARIYORUZ ---
                TodayStudyMinutes = todayStudyMins,
                ThisMonthStudyMinutes = monthStudyMins,
                TotalPomodoroCompleted = completedPomodoros,
                User = user
            };
            // --- ÇERÇEVE BİLGİLERİ ---
            var ownedFrames = _context.UserFrames
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.ProfileFrame)
                .Select(uf => uf.ProfileFrame)
                .ToList();

            ViewBag.OwnedFrames = ownedFrames;
            ViewBag.ActiveFrameImageUrl = user.ActiveFrameImageUrl;
            var ownedAvatarIds = _context.UserAvatars
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.ProfileAvatarId)
                .ToList();

            ViewBag.Avatars = _context.ProfileAvatars
                .Where(a => ownedAvatarIds.Contains(a.Id))
                .ToList();

            return View(model);
        }

        // --- AVATAR GÜNCELLEME ---
        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile file, string avatarUrl)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            try
            {
                // SENARYO 1: Dosya Yükleme (Cloudinary)
                if (file != null && file.Length > 0)
                {
                    string cloudName = _configuration["CloudinarySettings:CloudName"];
                    string apiKey = _configuration["CloudinarySettings:ApiKey"];
                    string apiSecret = _configuration["CloudinarySettings:ApiSecret"];

                    Account account = new Account(cloudName, apiKey, apiSecret);
                    Cloudinary cloudinary = new Cloudinary(account);

                    using (var stream = file.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(file.FileName, stream),
                            Transformation = new Transformation().Width(150).Height(150).Crop("fill").Gravity("face")
                        };

                        var uploadResult = await cloudinary.UploadAsync(uploadParams);

                        if (uploadResult.Error != null)
                        {
                            return Json(new { success = false, message = "Cloudinary hatası: " + uploadResult.Error.Message });
                        }

                        user.ProfileImageUrl = uploadResult.SecureUrl.AbsoluteUri;
                    }
                }
                // SENARYO 2: Hazır Avatar Seçimi
                else if (!string.IsNullOrEmpty(avatarUrl))
                {
                    user.ProfileImageUrl = avatarUrl;
                }
                else
                {
                    return Json(new { success = false, message = "Dosya veya avatar seçilmedi." });
                }

                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sunucu hatası: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveAvatar(string returnUrl)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.ProfileImageUrl = null;
                _context.SaveChanges();
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult PublicProfile(int id)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return RedirectToAction("Login", "Account");

            var user = _context.Users.Include(u => u.Books).FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            var isBlockedByMe = _context.Blocks.Any(b => b.BlockerId == loggedInUserId.Value && b.BlockedId == id);
            var isBlockingMe = _context.Blocks.Any(b => b.BlockerId == id && b.BlockedId == loggedInUserId.Value);

            var followersCount = _context.Follows.Count(f => f.FollowingId == id && f.IsAccepted);
            var followingCount = _context.Follows.Count(f => f.FollowerId == id && f.IsAccepted);
            var isFollowing = _context.Follows.Any(f => f.FollowerId == loggedInUserId && f.FollowingId == id && f.IsAccepted);
            var isPending = _context.Follows.Any(f => f.FollowerId == loggedInUserId && f.FollowingId == id && !f.IsAccepted);
            var bookCount = _context.Books.Count(b => b.UserId == id);

            // Toplam Odaklanma Süresi (Saat bazında)
            var totalFocusMins = _context.StudySessions.Where(s => s.UserId == id).Sum(s => s.DurationInMinutes);
            var totalFocusHours = totalFocusMins / 60;

            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;
            ViewBag.IsFollowing = isFollowing;
            ViewBag.IsPending = isPending;
            ViewBag.BookCount = bookCount;
            ViewBag.TotalFocusHours = totalFocusHours;
            ViewBag.LoggedInUserId = loggedInUserId;
            ViewBag.IsBlockedByMe = isBlockedByMe;
            ViewBag.IsBlockingMe = isBlockingMe;
            ViewBag.IsPrivate = user.IsPrivate;

            if (loggedInUserId == id)
            {
                user.PasswordHash = string.Empty;

                var ownedFrames = _context.UserFrames
                    .Where(uf => uf.UserId == id)
                    .Include(uf => uf.ProfileFrame)
                    .Select(uf => uf.ProfileFrame)
                    .ToList();

                ViewBag.OwnedFrames = ownedFrames;
                ViewBag.ActiveFrameImageUrl = user.ActiveFrameImageUrl;

                var ownedAvatarIds = _context.UserAvatars
                    .Where(ua => ua.UserId == id)
                    .Select(ua => ua.ProfileAvatarId)
                    .ToList();

                ViewBag.Avatars = _context.ProfileAvatars
                    .Where(a => ownedAvatarIds.Contains(a.Id))
                    .ToList();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfileInline(User model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || model.Id != userId) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            try
            {
                user.UserName = model.UserName;
                user.Bio = model.Bio;
                user.IsPrivate = model.IsPrivate;

                if (!string.IsNullOrEmpty(model.PasswordHash))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Güncelleme sırasında hata oluştu: " + ex.Message;
            }

            return RedirectToAction("PublicProfile", new { id = userId });
        }

        [HttpPost]
        public IActionResult ToggleFollow(int targetId)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            if (loggedInUserId == targetId) return Json(new { success = false, message = "Kendinizi takip edemezsiniz." });

            // DÜZELTME: Engel kontrolü eklendi — taraflardan biri diğerini engellediyse takip işlemi yapılamaz
            var isBlocked = _context.Blocks.Any(b =>
                (b.BlockerId == targetId && b.BlockedId == loggedInUserId.Value) ||
                (b.BlockerId == loggedInUserId.Value && b.BlockedId == targetId));
            if (isBlocked) return Json(new { success = false, message = "Bu işlem gerçekleştirilemiyor." });

            var existingFollow = _context.Follows.FirstOrDefault(f => f.FollowerId == loggedInUserId && f.FollowingId == targetId);
            var targetUser = _context.Users.FirstOrDefault(u => u.Id == targetId);

            if (targetUser == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            if (existingFollow != null)
            {
                // Takipten Çık veya İsteği Geri Çek
                _context.Follows.Remove(existingFollow);
                _context.SaveChanges();
                return Json(new { success = true, isFollowing = false, isPending = false });
            }
            else
            {
                // Takip Et veya İstek Gönder
                var newFollow = new Follow
                {
                    FollowerId = loggedInUserId.Value,
                    FollowingId = targetId,
                    CreatedAt = DateTime.Now,
                    IsAccepted = !targetUser.IsPrivate // Gizli hesapsa false, değilse true
                };
                _context.Follows.Add(newFollow);
                _context.SaveChanges();
                return Json(new { success = true, isFollowing = newFollow.IsAccepted, isPending = !newFollow.IsAccepted });
            }
        }

        // ============================================================
        //  YENİ: TAKİP İSTEKLERİ (Community sayfasındaki modal bunları çağırıyor)
        // ============================================================

        // Bekleyen (kabul edilmemiş) takip isteklerini listeler
        [HttpGet]
        public async Task<IActionResult> GetFollowRequests()
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            // Navigation property'ye bağımlı kalmamak için join kullanıldı
            var data = await (from f in _context.Follows
                              join u in _context.Users on f.FollowerId equals u.Id
                              where f.FollowingId == loggedInUserId.Value && !f.IsAccepted
                              orderby f.CreatedAt descending
                              select new
                              {
                                  followerId = f.FollowerId,
                                  userName = u.UserName,
                                  profileImage = u.ProfileImageUrl,
                                  activeFrame = u.ActiveFrameImageUrl
                              }).ToListAsync();

            return Json(new { success = true, data });
        }

        // Takip isteğini kabul eder
        [HttpPost]
        public async Task<IActionResult> AcceptFollowRequest(int followerId)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var request = await _context.Follows.FirstOrDefaultAsync(f =>
                f.FollowerId == followerId &&
                f.FollowingId == loggedInUserId.Value &&
                !f.IsAccepted);

            if (request == null) return Json(new { success = false, message = "İstek bulunamadı." });

            request.IsAccepted = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Takip isteğini reddeder (kaydı siler)
        [HttpPost]
        public async Task<IActionResult> RejectFollowRequest(int followerId)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var request = await _context.Follows.FirstOrDefaultAsync(f =>
                f.FollowerId == followerId &&
                f.FollowingId == loggedInUserId.Value &&
                !f.IsAccepted);

            if (request == null) return Json(new { success = false, message = "İstek bulunamadı." });

            _context.Follows.Remove(request);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            user.PasswordHash = string.Empty;

            var ownedFrames = _context.UserFrames
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.ProfileFrame)
                .Select(uf => uf.ProfileFrame)
                .ToList();

            ViewBag.OwnedFrames = ownedFrames;
            ViewBag.ActiveFrameImageUrl = user.ActiveFrameImageUrl;

            var ownedAvatarIds = _context.UserAvatars
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.ProfileAvatarId)
                .ToList();

            ViewBag.Avatars = _context.ProfileAvatars
                .Where(a => ownedAvatarIds.Contains(a.Id))
                .ToList();

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || model.Id != userId) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            try
            {
                user.UserName = model.UserName;
                user.Bio = model.Bio;

                if (!string.IsNullOrEmpty(model.PasswordHash))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Güncelleme sırasında hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Community(string q)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var blockedOrBlockingIds = _context.Blocks
                .Where(b => b.BlockerId == loggedInUserId.Value || b.BlockedId == loggedInUserId.Value)
                .Select(b => b.BlockerId == loggedInUserId.Value ? b.BlockedId : b.BlockerId)
                .ToList();

            // DÜZELTME: Kullanıcının kendisi artık sorguda dışlanıyor
            // (önceden view'da gizleniyordu; bu, listede sadece sen varken
            // "Sonuç Bulunamadı" yerine boş liste görünmesine yol açıyordu)
            var query = _context.Users
                .Include(u => u.Books)
                .Where(u =>
                !u.IsBanned &&
                u.Id != loggedInUserId.Value &&
                !blockedOrBlockingIds.Contains(u.Id)).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                // DÜZELTME: Büyük/küçük harf duyarsız arama (tüm veritabanı sağlayıcılarında tutarlı)
                var lowered = q.ToLower();
                query = query.Where(u => u.UserName.ToLower().Contains(lowered));
            }

            var users = await query
                .OrderByDescending(u => u.WisdomStones)
                .ToListAsync();

            // DÜZELTME: Hassas alan view'a gitmesin
            foreach (var u in users)
            {
                u.PasswordHash = string.Empty;
            }

            ViewBag.SearchQuery = q;
            ViewBag.CurrentUserId = loggedInUserId;

            var acceptedFollowings = await _context.Follows
                .Where(f => f.FollowerId == loggedInUserId.Value && f.IsAccepted)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var requestedFollowings = await _context.Follows
                .Where(f => f.FollowerId == loggedInUserId.Value && !f.IsAccepted)
                .Select(f => f.FollowingId)
                .ToListAsync();

            ViewBag.AcceptedFollowings = acceptedFollowings;
            ViewBag.RequestedFollowings = requestedFollowings;

            // DÜZELTME: "Takip İstekleri" butonundaki rozet için gelen istek sayısı
            // (önceden hiç set edilmiyordu, rozet her zaman gizli kalıyordu)
            ViewBag.IncomingRequestCount = await _context.Follows
                .CountAsync(f => f.FollowingId == loggedInUserId.Value && !f.IsAccepted);

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBlock(int userId)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Lütfen giriş yapın." });

            var existingBlock = await _context.Blocks
                .FirstOrDefaultAsync(b => b.BlockerId == loggedInUserId.Value && b.BlockedId == userId);

            if (existingBlock != null)
            {
                _context.Blocks.Remove(existingBlock);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isBlocked = false, message = "Engel kaldırıldı." });
            }
            else
            {
                var block = new BookManagementApp.Models.Block
                {
                    BlockerId = loggedInUserId.Value,
                    BlockedId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Blocks.Add(block);

                var follow1 = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == loggedInUserId.Value && f.FollowingId == userId);
                var follow2 = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == loggedInUserId.Value);
                if (follow1 != null) _context.Follows.Remove(follow1);
                if (follow2 != null) _context.Follows.Remove(follow2);

                await _context.SaveChangesAsync();
                return Json(new { success = true, isBlocked = true, message = "Kullanıcı engellendi." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReportUser(int reportedUserId, string reason, string details)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Lütfen giriş yapın." });

            if (string.IsNullOrWhiteSpace(reason)) return Json(new { success = false, message = "Lütfen bir sebep belirtin." });

            var report = new BookManagementApp.Models.Report
            {
                ReporterId = loggedInUserId.Value,
                ReportedUserId = reportedUserId,
                Reason = reason,
                Details = details,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Şikayetiniz başarıyla iletildi." });
        }

        // --- SOCIAL INTERACTIONS ---

        [HttpGet]
        public async Task<IActionResult> BookDetails(int id)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var book = await _context.Books
                .Include(b => b.User)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            var likesCount = await _context.BookLikes.CountAsync(bl => bl.BookId == id);
            var isLikedByMe = await _context.BookLikes.AnyAsync(bl => bl.BookId == id && bl.UserId == loggedInUserId);

            var comments = await _context.BookComments
                .Include(bc => bc.User)
                .Where(bc => bc.BookId == id)
                .OrderByDescending(bc => bc.CreatedAt)
                .ToListAsync();

            ViewBag.LikesCount = likesCount;
            ViewBag.IsLikedByMe = isLikedByMe;
            ViewBag.Comments = comments;
            ViewBag.CurrentUserId = loggedInUserId;

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLikeBook(int bookId)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var existingLike = await _context.BookLikes.FirstOrDefaultAsync(bl => bl.BookId == bookId && bl.UserId == loggedInUserId);

            if (existingLike != null)
            {
                _context.BookLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isLiked = false });
            }
            else
            {
                var newLike = new BookLike
                {
                    BookId = bookId,
                    UserId = loggedInUserId.Value
                };
                _context.BookLikes.Add(newLike);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isLiked = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int bookId, string text)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            if (string.IsNullOrWhiteSpace(text)) return Json(new { success = false, message = "Yorum boş olamaz." });

            var comment = new BookComment
            {
                BookId = bookId,
                UserId = loggedInUserId.Value,
                Text = text
            };

            _context.BookComments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var comment = await _context.BookComments.FirstOrDefaultAsync(bc => bc.Id == commentId);
            if (comment == null) return Json(new { success = false, message = "Yorum bulunamadı." });

            if (comment.UserId != loggedInUserId) return Json(new { success = false, message = "Yetkisiz işlem." });

            _context.BookComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}