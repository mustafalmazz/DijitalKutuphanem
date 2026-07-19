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
        private readonly BookManagementApp.Services.AchievementService _achievementService;

        public ProfileController(MyDbContext context, IConfiguration configuration, BookManagementApp.Services.AchievementService achievementService)
        {
            _context = context;
            _configuration = configuration;
            _achievementService = achievementService;
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

        public async Task<IActionResult> Index()
        {
            // 1. Oturum Kontrolü
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Başarımları Kontrol Et
            await _achievementService.CheckAndAwardAchievementsAsync(userId.Value);

            // 2. Kullanıcıyı Çek
            var user = await _context.Users
                .Include(u => u.UserAchievements)
                .ThenInclude(ua => ua.Achievement)
                .FirstOrDefaultAsync(u => u.Id == userId);
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
                User = user,
                UserAchievements = user.UserAchievements,
                ActiveBannerId = user.ActiveBannerId
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

            var ownedBannerIds = _context.UserBanners
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.ProfileBannerId)
                .ToList();

            ViewBag.Banners = _context.ProfileBanners
                .Where(b => ownedBannerIds.Contains(b.Id))
                .ToList();

            // Başarımlar partial'ı (_ProfileAchievements) için gerekli veriler
            ViewBag.AllAchievements = await _context.Achievements.ToListAsync();
            ViewBag.AchievementStats = await _achievementService.GetUserStatsAsync(userId.Value);
            ViewBag.LoggedInUserId = userId.Value;

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
        public async Task<IActionResult> PublicProfile(int id)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return RedirectToAction("Login", "Account");

            await _achievementService.CheckAndAwardAchievementsAsync(id);

            var user = await _context.Users
                .Include(u => u.Books)
                .Include(u => u.UserAchievements)
                .ThenInclude(ua => ua.Achievement)
                .Include(u => u.ActiveBanner)
                .FirstOrDefaultAsync(u => u.Id == id);
                
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

            // Başarımlar sekmesi için: tüm başarımlar + kategori bazlı kullanıcı metrikleri
            ViewBag.AllAchievements = await _context.Achievements.ToListAsync();
            ViewBag.AchievementStats = await _achievementService.GetUserStatsAsync(id);

            // Kitaplık kartlarındaki hızlı beğeni (kalp) için: beğeni sayıları + benim beğendiklerim
            var bookIds = user.Books?.Select(b => b.Id).ToList() ?? new List<int>();
            ViewBag.BookLikeCounts = await _context.BookLikes
                .Where(bl => bookIds.Contains(bl.BookId))
                .GroupBy(bl => bl.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BookId, x => x.Count);
            ViewBag.MyLikedBookIds = (await _context.BookLikes
                .Where(bl => bl.UserId == loggedInUserId.Value && bookIds.Contains(bl.BookId))
                .Select(bl => bl.BookId)
                .ToListAsync()).ToHashSet();

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

                var ownedBannerIds = _context.UserBanners
                    .Where(ub => ub.UserId == id)
                    .Select(ub => ub.ProfileBannerId)
                    .ToList();

                ViewBag.Banners = _context.ProfileBanners
                    .Where(b => ownedBannerIds.Contains(b.Id))
                    .ToList();
            }

            return View(user);
        }

        // --- TAKİPÇİ / TAKİP EDİLEN LİSTESİ (MODAL İÇİN JSON) ---
        [HttpGet]
        public async Task<IActionResult> FollowList(int id, string type)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");
            if (loggedInUserId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var target = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (target == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            // Gizlilik: kendi profili ya da gizli olmayan hesap ya da takipçisi olunan gizli hesap görülebilir
            bool isSelf = loggedInUserId.Value == id;
            bool isFollowingTarget = _context.Follows.Any(f => f.FollowerId == loggedInUserId.Value && f.FollowingId == id && f.IsAccepted);
            if (!isSelf && target.IsPrivate && !isFollowingTarget)
            {
                return Json(new { success = false, message = "Bu hesabın listesi gizli." });
            }

            // Engelleme iki yönlü uygulanır: görüntüleyenin engellediği VE görüntüleyeni engelleyen
            // kullanıcılar listeden çıkarılır. Modal başlığındaki sayı, listenin gerçek uzunluğuyla
            // (aşağıdaki count) senkronlandığı için bu filtreleme sayı/liste uyumsuzluğu yaratmaz.
            var hiddenByBlock = _context.Blocks
                .Where(b => b.BlockerId == loggedInUserId.Value || b.BlockedId == loggedInUserId.Value)
                .Select(b => b.BlockerId == loggedInUserId.Value ? b.BlockedId : b.BlockerId)
                .ToHashSet();

            // Navigation property'ye bağımlı kalmadan explicit join kullanılır (kod tabanındaki mevcut desenle uyumlu).
            // type: "following" => bu kullanıcının takip ettikleri, aksi halde => takipçileri
            List<FollowListItem> rawList;
            if (type == "following")
            {
                rawList = await (from f in _context.Follows
                                 join u in _context.Users on f.FollowingId equals u.Id
                                 where f.FollowerId == id && f.IsAccepted
                                 orderby f.CreatedAt descending
                                 select new FollowListItem
                                 {
                                     Id = u.Id,
                                     UserName = u.UserName,
                                     ProfileImageUrl = u.ProfileImageUrl,
                                     ActiveFrameImageUrl = u.ActiveFrameImageUrl,
                                     Bio = u.Bio
                                 }).ToListAsync();
            }
            else
            {
                rawList = await (from f in _context.Follows
                                 join u in _context.Users on f.FollowerId equals u.Id
                                 where f.FollowingId == id && f.IsAccepted
                                 orderby f.CreatedAt descending
                                 select new FollowListItem
                                 {
                                     Id = u.Id,
                                     UserName = u.UserName,
                                     ProfileImageUrl = u.ProfileImageUrl,
                                     ActiveFrameImageUrl = u.ActiveFrameImageUrl,
                                     Bio = u.Bio
                                 }).ToListAsync();
            }

            // Takip / bekleyen istek kümeleri — her satırdaki buton durumu için
            var myFollowingIds = _context.Follows
                .Where(f => f.FollowerId == loggedInUserId.Value && f.IsAccepted)
                .Select(f => f.FollowingId)
                .ToHashSet();
            var myPendingIds = _context.Follows
                .Where(f => f.FollowerId == loggedInUserId.Value && !f.IsAccepted)
                .Select(f => f.FollowingId)
                .ToHashSet();

            var users = rawList
                .Where(u => !hiddenByBlock.Contains(u.Id))
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.UserName,
                    profileImageUrl = u.ProfileImageUrl,
                    frameImageUrl = u.ActiveFrameImageUrl,
                    bio = u.Bio,
                    initial = string.IsNullOrEmpty(u.UserName) ? "?" : u.UserName.Substring(0, 1).ToUpper(),
                    isSelf = u.Id == loggedInUserId.Value,
                    isFollowing = myFollowingIds.Contains(u.Id),
                    isPending = myPendingIds.Contains(u.Id)
                })
                .ToList();

            // count = listenin gerçek uzunluğu; modal başlığı bununla senkronlanır
            return Json(new { success = true, count = users.Count, users });
        }

        // --- BAŞARIM ÖDÜLÜNÜ MANUEL TOPLA ---
        [HttpPost]
        public async Task<IActionResult> ClaimAchievement(int achievementId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var result = await _achievementService.ClaimAchievementAsync(userId.Value, achievementId);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                reward = result.RewardStones,
                newTotal = result.NewTotal
            });
        }

        // --- PROFİLDE GÖSTERİLECEK ETİKETİ SEÇ ---
        // achievementId = 0 gönderilirse takılı etiket kaldırılır.
        [HttpPost]
        public async Task<IActionResult> EquipTitle(int achievementId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum kapalı." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            if (achievementId == 0)
            {
                user.ActiveTitleAchievementId = null;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Etiket kaldırıldı.", equippedId = 0 });
            }

            // Sunucu tarafı doğrulama: yalnızca gerçekten kazanılmış bir başarım takılabilir.
            bool owned = await _context.UserAchievements
                .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

            if (!owned)
            {
                return Json(new { success = false, message = "Bu etikete henüz sahip değilsin." });
            }

            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == achievementId);
            if (achievement == null) return Json(new { success = false, message = "Başarım bulunamadı." });

            user.ActiveTitleAchievementId = achievementId;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"\"{achievement.Name}\" artık profilinde görünüyor.",
                equippedId = achievementId,
                name = achievement.Name,
                iconClass = achievement.IconClass,
                colorHex = achievement.ColorHex
            });
        }

        // FollowList sorgusu için basit taşıyıcı (projeksiyon tipi)
        private class FollowListItem
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string ProfileImageUrl { get; set; }
            public string ActiveFrameImageUrl { get; set; }
            public string Bio { get; set; }
        }

        [HttpPost]
        public IActionResult EditProfileInline(User model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || model.Id != userId) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            var newInlineName = (model.UserName ?? "").Trim();
            if (newInlineName.Length < 3 || newInlineName.Length > 30)
            {
                TempData["ErrorMessage"] = "Kullanıcı adı 3 ile 30 karakter arasında olmalı.";
                return RedirectToAction("PublicProfile", new { id = userId });
            }

            if (_context.Users.Any(u => u.UserName == newInlineName && u.Id != userId))
            {
                TempData["ErrorMessage"] = "Bu kullanıcı adı zaten kullanılıyor.";
                return RedirectToAction("PublicProfile", new { id = userId });
            }

            try
            {
                user.UserName = newInlineName;
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
        // ================== HESAP AYARLARI ==================

        [HttpGet]
        public IActionResult Settings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            // Google ile kayıt olanların şifresi olmayabilir; görünüm buna göre şekillenir
            ViewBag.HasPassword = !string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash.StartsWith("$2");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUsername(string newUserName)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            newUserName = (newUserName ?? "").Trim();

            if (newUserName.Length < 3 || newUserName.Length > 30)
            {
                TempData["SettingsError"] = "Kullanıcı adı 3 ile 30 karakter arasında olmalı.";
                return RedirectToAction(nameof(Settings));
            }

            if (newUserName == user.UserName)
            {
                TempData["SettingsError"] = "Yeni kullanıcı adı mevcut adınızla aynı.";
                return RedirectToAction(nameof(Settings));
            }

            var taken = _context.Users.Any(u => u.UserName == newUserName && u.Id != userId);
            if (taken)
            {
                TempData["SettingsError"] = "Bu kullanıcı adı zaten kullanılıyor.";
                return RedirectToAction(nameof(Settings));
            }

            user.UserName = newUserName;
            _context.SaveChanges();

            TempData["SettingsSuccess"] = "Kullanıcı adınız güncellendi.";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePassword(string? currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            var hasPassword = !string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash.StartsWith("$2");

            // Mevcut şifresi olan kullanıcıdan önce onu doğrulamasını iste
            if (hasPassword)
            {
                if (string.IsNullOrEmpty(currentPassword) ||
                    !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    TempData["SettingsError"] = "Mevcut şifreniz hatalı.";
                    return RedirectToAction(nameof(Settings));
                }
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                TempData["SettingsError"] = "Yeni şifre en az 6 karakter olmalı.";
                return RedirectToAction(nameof(Settings));
            }

            if (newPassword != confirmPassword)
            {
                TempData["SettingsError"] = "Yeni şifreler birbiriyle uyuşmuyor.";
                return RedirectToAction(nameof(Settings));
            }

            if (hasPassword && BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                TempData["SettingsError"] = "Yeni şifre eski şifrenizle aynı olamaz.";
                return RedirectToAction(nameof(Settings));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.SaveChanges();

            TempData["SettingsSuccess"] = "Şifreniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePrivacy(bool isPrivate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            user.IsPrivate = isPrivate;
            _context.SaveChanges();

            TempData["SettingsSuccess"] = isPrivate
                ? "Hesabınız gizli hesaba çevrildi. Kitaplığınızı ve istatistiklerinizi sadece takipçileriniz görebilir."
                : "Hesabınız herkese açık hale getirildi.";
            return RedirectToAction(nameof(Settings));
        }

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

            var ownedBannerIds = _context.UserBanners
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.ProfileBannerId)
                .ToList();

            ViewBag.Banners = _context.ProfileBanners
                .Where(b => ownedBannerIds.Contains(b.Id))
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

            var newName = (model.UserName ?? "").Trim();
            if (newName.Length < 3 || newName.Length > 30)
            {
                TempData["ErrorMessage"] = "Kullanıcı adı 3 ile 30 karakter arasında olmalı.";
                return RedirectToAction("Edit");
            }

            if (_context.Users.Any(u => u.UserName == newName && u.Id != userId))
            {
                TempData["ErrorMessage"] = "Bu kullanıcı adı zaten kullanılıyor.";
                return RedirectToAction("Edit");
            }

            try
            {
                user.UserName = newName;
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
                .Include(u => u.ActiveTitleAchievement)
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
            var myId = HttpContext.Session.GetInt32("UserId");
            if (myId == null) return Json(new { success = false, message = "Oturum kapal." });

            var existing = await _context.Blocks.FirstOrDefaultAsync(b => b.BlockerId == myId.Value && b.BlockedId == userId);
            if (existing != null)
            {
                _context.Blocks.Remove(existing);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isBlocked = false, message = "Engel kaldrld." });
            }
            else
            {
                var b = new BookManagementApp.Models.Block { BlockerId = myId.Value, BlockedId = userId, CreatedAt = DateTime.Now };
                _context.Blocks.Add(b);

                // Eer takipleme varsa sil
                var f1 = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == myId.Value && f.FollowingId == userId);
                if (f1 != null) _context.Follows.Remove(f1);
                var f2 = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == myId.Value);
                if (f2 != null) _context.Follows.Remove(f2);

                await _context.SaveChangesAsync();
                return Json(new { success = true, isBlocked = true, message = "Kullanc engellendi." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EquipBannerProfile(int bannerId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum kapal." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Json(new { success = false, message = "Kullanc bulunamad." });

            // bannerId 0 ise kaldır
            if (bannerId == 0)
            {
                user.ActiveBannerId = null;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Banner kaldrld." });
            }

            // Sahiplik kontrolü
            bool owned = await _context.UserBanners.AnyAsync(ub => ub.UserId == userId && ub.ProfileBannerId == bannerId);
            if (!owned) return Json(new { success = false, message = "Bu arka plana sahip deilsiniz." });

            user.ActiveBannerId = bannerId;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
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
                int likeCount = await _context.BookLikes.CountAsync(bl => bl.BookId == bookId);
                return Json(new { success = true, isLiked = false, likeCount });
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
                int likeCount = await _context.BookLikes.CountAsync(bl => bl.BookId == bookId);
                return Json(new { success = true, isLiked = true, likeCount });
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