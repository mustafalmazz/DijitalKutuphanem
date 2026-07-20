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
                    .Where(f => f.PackCategory == null)
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
                    .ToListAsync(),
                Banners = await _context.ProfileBanners
                    .Where(b => b.PackCategory == null)
                    .OrderBy(b => b.PriceInStones)
                    .ThenBy(b => b.RequiredBookCount)
                    .ToListAsync()
            };

            // Paket içerikleri tür bağımsız tek listede toplanır
            var packAvatarItems = await _context.ProfileAvatars
                .Where(a => a.PackCategory != null)
                .Select(a => new PackItemView { Id = a.Id, Name = a.Name, ImageUrl = a.ImageUrl, PackCategory = a.PackCategory, ItemType = PackItemTypes.Avatar })
                .ToListAsync();

            var packFrameItems = await _context.ProfileFrames
                .Where(f => f.PackCategory != null)
                .Select(f => new PackItemView { Id = f.Id, Name = f.Name, ImageUrl = f.ImageUrl, PackCategory = f.PackCategory, ItemType = PackItemTypes.Frame })
                .ToListAsync();

            var packBannerItems = await _context.ProfileBanners
                .Where(b => b.PackCategory != null)
                .Select(b => new PackItemView { Id = b.Id, Name = b.Name, ImageUrl = b.ImageUrl, PackCategory = b.PackCategory, ItemType = PackItemTypes.Banner })
                .ToListAsync();

            viewModel.PackItems = packAvatarItems.Concat(packFrameItems).Concat(packBannerItems).ToList();

            var ownedFrameIds = await _context.UserFrames
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.ProfileFrameId)
                .ToListAsync();

            var ownedAvatarIds = await _context.UserAvatars
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.ProfileAvatarId)
                .ToListAsync();

            var ownedBannerIds = await _context.UserBanners
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.ProfileBannerId)
                .ToListAsync();

            var userBookCount = await _context.Books.CountAsync(b => b.UserId == userId);

            ViewBag.User = user;
            ViewBag.OwnedFrameIds = ownedFrameIds;
            ViewBag.OwnedAvatarIds = ownedAvatarIds;
            ViewBag.OwnedBannerIds = ownedBannerIds;

            // Paket içeriğinde "sende var" işareti için tür bazlı sahiplik kümesi
            ViewBag.OwnedPackKeys = ownedAvatarIds.Select(id => $"{PackItemTypes.Avatar}:{id}")
                .Concat(ownedFrameIds.Select(id => $"{PackItemTypes.Frame}:{id}"))
                .Concat(ownedBannerIds.Select(id => $"{PackItemTypes.Banner}:{id}"))
                .ToHashSet();
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

        public async Task<IActionResult> Banners()
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

            // Paket içeriği türe göre okunur; üçü de aynı akıştan geçer
            var packItems = await GetPackItemsAsync(package);

            if (!packItems.Any())
                return Json(new { success = false, message = $"Bu pakette henüz hiç {PackItemTypes.DisplayName(package.ItemType).ToLower()} yok!" });

            var ownedIds = await GetOwnedIdsForTypeAsync(package.ItemType, userId.Value);
            var unownedItems = packItems.Where(i => !ownedIds.Contains(i.Id)).ToList();

            if (!unownedItems.Any())
                return Json(new { success = false, message = "Tebrikler! Bu paketteki TÜM ürünlere zaten sahipsiniz. Başka çekiliş yapamazsınız." });

            var random = new Random();
            var drawn = unownedItems[random.Next(unownedItems.Count)];

            user.WisdomStones -= package.PriceInStones;
            GrantPackItem(package.ItemType, userId.Value, drawn.Id);
            await _context.SaveChangesAsync();

            return Json(new {
                success = true,
                newBalance = user.WisdomStones,
                itemId = drawn.Id,
                itemName = drawn.Name,
                itemImage = drawn.ImageUrl,
                itemType = package.ItemType,
                isWide = drawn.IsWide
            });
        }

        /// <summary>Paketin türüne göre içeriğini tür bağımsız biçimde döner.</summary>
        private async Task<List<PackItemView>> GetPackItemsAsync(StorePackage package)
        {
            return package.ItemType switch
            {
                PackItemTypes.Frame => await _context.ProfileFrames
                    .Where(f => f.PackCategory == package.CategoryCode)
                    .Select(f => new PackItemView { Id = f.Id, Name = f.Name, ImageUrl = f.ImageUrl, ItemType = PackItemTypes.Frame })
                    .ToListAsync(),

                PackItemTypes.Banner => await _context.ProfileBanners
                    .Where(b => b.PackCategory == package.CategoryCode)
                    .Select(b => new PackItemView { Id = b.Id, Name = b.Name, ImageUrl = b.ImageUrl, ItemType = PackItemTypes.Banner })
                    .ToListAsync(),

                _ => await _context.ProfileAvatars
                    .Where(a => a.PackCategory == package.CategoryCode)
                    .Select(a => new PackItemView { Id = a.Id, Name = a.Name, ImageUrl = a.ImageUrl, ItemType = PackItemTypes.Avatar })
                    .ToListAsync()
            };
        }

        /// <summary>Kullanıcının o türde sahip olduğu ürün id'leri.</summary>
        private async Task<HashSet<int>> GetOwnedIdsForTypeAsync(string itemType, int userId)
        {
            return itemType switch
            {
                PackItemTypes.Frame => (await _context.UserFrames
                    .Where(x => x.UserId == userId).Select(x => x.ProfileFrameId).ToListAsync()).ToHashSet(),

                PackItemTypes.Banner => (await _context.UserBanners
                    .Where(x => x.UserId == userId).Select(x => x.ProfileBannerId).ToListAsync()).ToHashSet(),

                _ => (await _context.UserAvatars
                    .Where(x => x.UserId == userId).Select(x => x.ProfileAvatarId).ToListAsync()).ToHashSet()
            };
        }

        /// <summary>Çekilişten çıkan ürünü doğru sahiplik tablosuna yazar.</summary>
        private void GrantPackItem(string itemType, int userId, int itemId)
        {
            switch (itemType)
            {
                case PackItemTypes.Frame:
                    _context.UserFrames.Add(new UserFrame { UserId = userId, ProfileFrameId = itemId, PurchasedAt = DateTime.Now });
                    break;
                case PackItemTypes.Banner:
                    _context.UserBanners.Add(new UserBanner { UserId = userId, ProfileBannerId = itemId, PurchasedAt = DateTime.Now });
                    break;
                default:
                    _context.UserAvatars.Add(new UserAvatar { UserId = userId, ProfileAvatarId = itemId });
                    break;
            }
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
        public async Task<IActionResult> BuyBanner(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            var banner = await _context.ProfileBanners.FindAsync(id);
            if (banner == null) return RedirectToAction("Banners");

            bool alreadyOwned = await _context.UserBanners.AnyAsync(ub => ub.UserId == userId && ub.ProfileBannerId == id);
            if (alreadyOwned)
            {
                TempData["ErrorMessage"] = "Bu bannera zaten sahipsiniz!";
                return RedirectToAction("Banners");
            }

            if (banner.RequiredBookCount > 0)
            {
                var userBookCount = await _context.Books.CountAsync(b => b.UserId == userId);
                if (userBookCount < banner.RequiredBookCount)
                {
                    TempData["ErrorMessage"] = $"Bu banner için en az {banner.RequiredBookCount} kitap eklemiş olmalısınız!";
                    return RedirectToAction("Banners");
                }
            }

            if (user.WisdomStones < banner.PriceInStones)
            {
                TempData["ErrorMessage"] = "Yeterli Bilgelik Taşınız yok!";
                return RedirectToAction("Banners");
            }

            user.WisdomStones -= banner.PriceInStones;
            _context.UserBanners.Add(new UserBanner { UserId = userId.Value, ProfileBannerId = id, PurchasedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"\"{banner.Name}\" başarıyla satın alındı!";
            return RedirectToAction("Banners");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EquipBanner(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");

            // id = 0: banneri kaldır, varsayılan gradyana dön
            if (id == 0)
            {
                user.ActiveBannerId = null;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Banner kaldırıldı.";
                return RedirectToAction("Banners");
            }

            // Sunucu tarafı doğrulama: yalnızca sahip olunan banner takılabilir
            bool owned = await _context.UserBanners.AnyAsync(ub => ub.UserId == userId && ub.ProfileBannerId == id);
            if (!owned) return RedirectToAction("Banners");

            // Takılıysa tekrar basmak çıkarır (çerçevelerle aynı davranış)
            user.ActiveBannerId = user.ActiveBannerId == id ? null : id;
            await _context.SaveChangesAsync();
            return RedirectToAction("Banners");
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
