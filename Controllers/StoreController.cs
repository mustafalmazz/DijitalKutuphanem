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

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var viewModel = new StoreViewModel
            {
                Frames = await _context.ProfileFrames.ToListAsync(),
                Avatars = await _context.ProfileAvatars.ToListAsync()
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

            return View(viewModel);
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
            if (frame == null) return RedirectToAction("Index");

            bool alreadyOwned = await _context.UserFrames.AnyAsync(uf => uf.UserId == userId && uf.ProfileFrameId == id);
            if (alreadyOwned)
            {
                TempData["ErrorMessage"] = "Bu çerçeveye zaten sahipsiniz!";
                return RedirectToAction("Index");
            }

            if (frame.RequiredBookCount > 0)
            {
                var userBookCount = await _context.Books.CountAsync(b => b.UserId == userId);
                if (userBookCount < frame.RequiredBookCount)
                {
                    TempData["ErrorMessage"] = $"Bu çerçeve için en az {frame.RequiredBookCount} kitap eklemiş olmalısınız!";
                    return RedirectToAction("Index");
                }
            }

            if (user.WisdomStones < frame.PriceInStones)
            {
                TempData["ErrorMessage"] = "Yeterli Bilgelik Taşınız yok!";
                return RedirectToAction("Index");
            }

            user.WisdomStones -= frame.PriceInStones;
            _context.UserFrames.Add(new UserFrame { UserId = userId.Value, ProfileFrameId = id, PurchasedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{frame.Name}\" başarıyla satın alındı!";
            return RedirectToAction("Index");
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
            if (avatar == null) return RedirectToAction("Index");

            bool alreadyOwned = await _context.UserAvatars.AnyAsync(ua => ua.UserId == userId && ua.ProfileAvatarId == id);
            if (alreadyOwned)
            {
                TempData["ErrorMessage"] = "Bu avatara zaten sahipsiniz!";
                return RedirectToAction("Index");
            }

            if (avatar.RequiredBookCount > 0)
            {
                var userBookCount = await _context.Books.CountAsync(b => b.UserId == userId);
                if (userBookCount < avatar.RequiredBookCount)
                {
                    TempData["ErrorMessage"] = $"Bu avatar için en az {avatar.RequiredBookCount} kitap eklemiş olmalısınız!";
                    return RedirectToAction("Index");
                }
            }

            if (user.WisdomStones < avatar.PriceInStones)
            {
                TempData["ErrorMessage"] = "Yeterli Bilgelik Taşınız yok!";
                return RedirectToAction("Index");
            }

            user.WisdomStones -= avatar.PriceInStones;
            _context.UserAvatars.Add(new UserAvatar { UserId = userId.Value, ProfileAvatarId = id });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{avatar.Name}\" başarıyla satın alındı!";
            return RedirectToAction("Index");
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
                return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index") : Redirect(returnUrl);
            }

            var frame = await _context.ProfileFrames.FindAsync(id);
            bool owned = await _context.UserFrames.AnyAsync(uf => uf.UserId == userId && uf.ProfileFrameId == id);
            if (frame == null || !owned) return RedirectToAction("Index");

            if (user.ActiveFrameImageUrl == frame.ImageUrl) user.ActiveFrameImageUrl = null;
            else user.ActiveFrameImageUrl = frame.ImageUrl;

            await _context.SaveChangesAsync();
            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index") : Redirect(returnUrl);
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
            if (avatar == null || !owned) return RedirectToAction("Index");

            user.ProfileImageUrl = avatar.ImageUrl; // Equip directly
            TempData["SuccessMessage"] = "Avatar değiştirildi!";
            await _context.SaveChangesAsync();

            return string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index") : Redirect(returnUrl);
        }
    }
}
