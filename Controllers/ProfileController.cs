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



            var followersCount = _context.Follows.Count(f => f.FollowingId == id);
            var followingCount = _context.Follows.Count(f => f.FollowerId == id);
            var isFollowing = _context.Follows.Any(f => f.FollowerId == loggedInUserId && f.FollowingId == id);
            var bookCount = _context.Books.Count(b => b.UserId == id);

            // Toplam Odaklanma Süresi (Saat bazında)
            var totalFocusMins = _context.StudySessions.Where(s => s.UserId == id).Sum(s => s.DurationInMinutes);
            var totalFocusHours = totalFocusMins / 60;

            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;
            ViewBag.IsFollowing = isFollowing;
            ViewBag.BookCount = bookCount;
            ViewBag.TotalFocusHours = totalFocusHours;
            ViewBag.LoggedInUserId = loggedInUserId;

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

            var existingFollow = _context.Follows.FirstOrDefault(f => f.FollowerId == loggedInUserId && f.FollowingId == targetId);

            if (existingFollow != null)
            {
                // Takipten Çık
                _context.Follows.Remove(existingFollow);
                _context.SaveChanges();
                return Json(new { success = true, isFollowing = false });
            }
            else
            {
                // Takip Et
                var newFollow = new Follow
                {
                    FollowerId = loggedInUserId.Value,
                    FollowingId = targetId,
                    CreatedAt = DateTime.Now
                };
                _context.Follows.Add(newFollow);
                _context.SaveChanges();
                return Json(new { success = true, isFollowing = true });
            }
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

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(u => u.UserName.Contains(q));
            }

            var users = await query
                .OrderByDescending(u => u.WisdomStones)
                .ToListAsync();

            ViewBag.SearchQuery = q;
            ViewBag.CurrentUserId = loggedInUserId;

            var followings = await _context.Follows
                .Where(f => f.FollowerId == loggedInUserId.Value)
                .Select(f => f.FollowingId)
                .ToListAsync();

            ViewBag.Followings = followings;

            return View(users);
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