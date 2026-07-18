using BookManagementApp.Models;
using BookManagementApp.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BookManagementApp.Controllers
{
    [Authorize]
    public class StoreController : Controller
    {
        private readonly MyDbContext _context;

        public StoreController(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Mağazanın dört sayfası da (Index, Avatars, Frames, Packs, Specials) aynı veriyi
        /// gösterdiği için yükleme tek yerde toplandı. Oturum yoksa null döner.
        /// </summary>
        private async Task<StoreViewModel?> LoadStoreAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var viewModel = new StoreViewModel
            {
                Frames = await _context.ProfileFrames
                    .OrderBy(f => f.PriceInStones)
                    .ThenBy(f => f.RequiredBookCount)
                    .ToListAsync(),
                Avatars = await _context.ProfileAvatars
                    .Where(a => a.PackCategory == null)
                    .OrderBy(a => a.PriceInStones)
                    .ThenBy(a => a.RequiredBookCount)
                    .ToListAsync(),
                PackAvatars = await _context.ProfileAvatars
                    .Where(a => a.PackCategory != null)
                    .OrderBy(a => a.Id)
                    .ToListAsync(),
                Packages = await _context.StorePackages
                    .OrderBy(p => p.Id)
                    .ToListAsync()
            };

            var ownedFrameIds = await _context.UserFrames
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.ProfileFrameId)
                .ToListAsync();

            var ownedAvatarIds = await _context.UserAvatars
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.ProfileAvatarId)
                .ToListAsync();

            var userBookCount = await _context.Books.CountAsync(b => b.UserId == userId);

            ViewBag.User = user;
            ViewBag.OwnedFrameIds = ownedFrameIds;
            ViewBag.OwnedAvatarIds = ownedAvatarIds;
            ViewBag.UserBookCount = userBookCount;

            return viewModel;
        }

        // Mağaza girişi: dört dükkana yönlendiren kartlar.
        public async Task<IActionResult> Index()
        {
            var viewModel = await LoadStoreAsync();
            if (viewModel == null) return RedirectToAction("Login", "Account");
            return View(viewModel);
        }

        public async Task<IActionResult> Avatars()
        {
            var viewModel = await LoadStoreAsync();
            if (viewModel == null) return RedirectToAction("Login", "Account");
            return View(viewModel);
        }

        public async Task<IActionResult> Frames()
        {
            var viewModel = await LoadStoreAsync();
            if (viewModel == null) return RedirectToAction("Login", "Account");
            return View(viewModel);
        }

        public async Task<IActionResult> Packs()
        {
            var viewModel = await LoadStoreAsync();
            if (viewModel == null) return RedirectToAction("Login", "Account");
            return View(viewModel);
        }

        public async Task<IActionResult> Specials()
        {
            var viewModel = await LoadStoreAsync();
            if (viewModel == null) return RedirectToAction("Login", "Account");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BuyPack(int packageId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Oturum süresi doldu." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            var package = await _context.StorePackages.FindAsync(packageId);
            if (package == null) return Json(new { success = false, message = "Paket bulunamadı." });

            if (user.WisdomStones < package.PriceInStones)
                return Json(new { success = false, message = $"Yeterli Bilgelik Taşınız yok! En az {package.PriceInStones} taşa ihtiyacınız var." });

            // Paketteki tüm avatarlar
            var packItems = await _context.ProfileAvatars
                .Where(a => a.PackCategory == package.CategoryCode)
                .ToListAsync();

            if (!packItems.Any())
                return Json(new { success = false, message = "Bu pakette henüz hiç avatar yok!" });

            // Kullanıcının sahip olduğu avatarlar
            var ownedAvatarIds = await _context.UserAvatars
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.ProfileAvatarId)
                .ToListAsync();

            // Sadece sahip OLMADIĞI avatarlar
            var unownedItems = packItems.Where(a => !ownedAvatarIds.Contains(a.Id)).ToList();

            if (!unownedItems.Any())
                return Json(new { success = false, message = "Tebrikler! Bu paketteki TÜM avatarlara zaten sahipsiniz. Başka çekiliş yapamazsınız." });

            // Rastgele seçim
            var random = new Random();
            var drawnAvatar = unownedItems[random.Next(unownedItems.Count)];

            // Satın almayı gerçekleştir
            user.WisdomStones -= package.PriceInStones;
            _context.UserAvatars.Add(new UserAvatar { UserId = userId.Value, ProfileAvatarId = drawnAvatar.Id });
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                newBalance = user.WisdomStones,
                avatarId = drawnAvatar.Id,
                avatarName = drawnAvatar.Name,
                avatarImage = drawnAvatar.ImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyFrame(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var frame = await _context.ProfileFrames.FindAsync(id);
            if (frame == null) return RedirectToAction("Frames");

            bool alreadyOwned = await _context.UserFrames.AnyAsync(uf => uf.UserId == userId && uf.ProfileFrameId == id);
            if (alreadyOwned)
            {
                TempData["ErrorMessage"] = "Bu çerçeveye zaten sahipsiniz!";
                return RedirectToAction("Frames");
            }

            if (frame.RequiredBookCount > 0)
            {
                var userBookCount = await _context.Books.CountAsync(b => b.UserId == userId);
                if (userBookCount < frame.RequiredBookCount)
                {
                    TempData["ErrorMessage"] = $"Bu çerçeve için en az {frame.RequiredBookCount} kitap eklemiş olmalısınız!";
                    return RedirectToAction("Frames");
                }
            }

            if (user.WisdomStones < frame.PriceInStones)
            {
                TempData["ErrorMessage"] = "Yeterli Bilgelik Taşınız yok!";
                return RedirectToAction("Frames");
            }

            user.WisdomStones -= frame.PriceInStones;
            _context.UserFrames.Add(new UserFrame { UserId = userId.Value, ProfileFrameId = id, PurchasedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{frame.Name}\" başarıyla satın alındı!";
            return RedirectToAction("Frames");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyAvatar(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var avatar = await _context.ProfileAvatars.FindAsync(id);
            if (avatar == null) return RedirectToAction("Avatars");

            bool alreadyOwned = await _context.UserAvatars.AnyAsync(ua => ua.UserId == userId && ua.ProfileAvatarId == id);
            if (alreadyOwned)
            {
                TempData["ErrorMessage"] = "Bu avatara zaten sahipsiniz!";
                return RedirectToAction("Avatars");
            }

            if (avatar.RequiredBookCount > 0)
            {
                var userBookCount = await _context.Books.CountAsync(b => b.UserId == userId);
                if (userBookCount < avatar.RequiredBookCount)
                {
                    TempData["ErrorMessage"] = $"Bu avatar için en az {avatar.RequiredBookCount} kitap eklemiş olmalısınız!";
                    return RedirectToAction("Avatars");
                }
            }

            if (user.WisdomStones < avatar.PriceInStones)
            {
                TempData["ErrorMessage"] = "Yeterli Bilgelik Taşınız yok!";
                return RedirectToAction("Avatars");
            }

            user.WisdomStones -= avatar.PriceInStones;
            _context.UserAvatars.Add(new UserAvatar { UserId = userId.Value, ProfileAvatarId = id });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{avatar.Name}\" başarıyla satın alındı!";
            return RedirectToAction("Avatars");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EquipFrame(int id, string? returnUrl)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            if (id == 0)
            {
                user.ActiveFrameImageUrl = null;
                TempData["SuccessMessage"] = "Çerçeve kaldırıldı.";
                await _context.SaveChangesAsync();
                return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Frames") : Redirect(returnUrl);
            }

            var frame = await _context.ProfileFrames.FindAsync(id);
            bool owned = await _context.UserFrames.AnyAsync(uf => uf.UserId == userId && uf.ProfileFrameId == id);
            if (frame == null || !owned) return RedirectToAction("Frames");

            if (user.ActiveFrameImageUrl == frame.ImageUrl) user.ActiveFrameImageUrl = null;
            else user.ActiveFrameImageUrl = frame.ImageUrl;

            await _context.SaveChangesAsync();
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Frames") : Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EquipAvatar(int id, string? returnUrl)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var avatar = await _context.ProfileAvatars.FindAsync(id);
            bool owned = await _context.UserAvatars.AnyAsync(ua => ua.UserId == userId && ua.ProfileAvatarId == id);
            if (avatar == null || !owned) return RedirectToAction("Avatars");

            user.ProfileImageUrl = avatar.ImageUrl; // Equip directly
            TempData["SuccessMessage"] = "Avatar değiştirildi!";
            await _context.SaveChangesAsync();

            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Avatars") : Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyStreakFreeze()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Lütfen giriş yapın." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            int freezePrice = 100;
            int maxFreezes = 2;

            if (user.StreakFreezes >= maxFreezes)
            {
                return Json(new { success = false, message = $"En fazla {maxFreezes} adet Seri Dondurma hakkına sahip olabilirsiniz." });
            }

            if (user.WisdomStones < freezePrice)
            {
                return Json(new { success = false, message = "Yetersiz Bilgelik Taşı!" });
            }

            user.WisdomStones -= freezePrice;
            user.StreakFreezes++;
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newBalance = user.WisdomStones, newFreezes = user.StreakFreezes, message = "Seri Dondurma başarıyla satın alındı!" });
        }
    }
}
