using BookManagementApp.Models;
using BookManagementApp.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace BookManagementApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class StoreManagementController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StoreManagementController(MyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // --- BANNER YÖNETİMİ ---

        public async Task<IActionResult> BannersIndex()
        {
            var banners = await _context.ProfileBanners.OrderBy(b => b.PriceInStones).ToListAsync();
            return View(banners);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBanner(ProfileBanner banner, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // GIF/WebP kabul edilir; tarayıcı animasyonu kendiliğinden oynatır
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    TempData["ErrorMessage"] = "Yalnızca PNG, JPG, GIF veya WebP yükleyebilirsiniz.";
                    return RedirectToAction(nameof(BannersIndex));
                }

                if (ImageFile.Length > 8 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Dosya 8 MB'den büyük olamaz.";
                    return RedirectToAction(nameof(BannersIndex));
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "banners");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                banner.ImageUrl = "/images/banners/" + uniqueFileName;
            }

            ModelState.Remove("ImageUrl");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid && !string.IsNullOrEmpty(banner.ImageUrl))
            {
                _context.ProfileBanners.Add(banner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Banner başarıyla eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen bir görsel seçin.";
            }

            return RedirectToAction(nameof(BannersIndex));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBanner(ProfileBanner banner)
        {
            var existing = await _context.ProfileBanners.FindAsync(banner.Id);
            if (existing == null) return NotFound();

            existing.Name = banner.Name;
            existing.Description = banner.Description;
            existing.PriceInStones = banner.PriceInStones;
            existing.RequiredBookCount = banner.RequiredBookCount;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Banner güncellendi.";
            return RedirectToAction(nameof(BannersIndex));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.ProfileBanners.FindAsync(id);
            if (banner == null) return NotFound();

            // Takılı olduğu profillerde varsayılana dönülür; sahiplik kayıtları temizlenir
            var wearers = await _context.Users.Where(u => u.ActiveBannerId == id).ToListAsync();
            foreach (var w in wearers) w.ActiveBannerId = null;

            var ownerships = await _context.UserBanners.Where(ub => ub.ProfileBannerId == id).ToListAsync();
            _context.UserBanners.RemoveRange(ownerships);

            _context.ProfileBanners.Remove(banner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Banner silindi.";
            return RedirectToAction(nameof(BannersIndex));
        }

        // --- ÇERÇEVE YÖNETİMİ ---

        public async Task<IActionResult> Index()
        {
            var frames = await _context.ProfileFrames.ToListAsync();
            return View(frames);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFrame(ProfileFrame frame, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "frames");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                frame.ImageUrl = "/images/frames/" + uniqueFileName;
            }

            // Gerekli değilse validation hatası vermemesi için
            ModelState.Remove("ImageUrl"); 
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                _context.ProfileFrames.Add(frame);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Çerçeve başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Çerçeve eklenirken bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditFrame(ProfileFrame frame)
        {
            var existingFrame = await _context.ProfileFrames.FindAsync(frame.Id);
            if (existingFrame == null) return NotFound();

            existingFrame.Name = frame.Name;
            existingFrame.Description = frame.Description;
            existingFrame.IconEmoji = frame.IconEmoji;
            existingFrame.PriceInStones = frame.PriceInStones;
            existingFrame.RequiredBookCount = frame.RequiredBookCount;
            // Not: Resmi değiştirmek için ayrı bir mekanizma yapılabilir veya şimdilik sadece fiyatları güncelliyoruz.

            _context.ProfileFrames.Update(existingFrame);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Çerçeve bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFrame(int id)
        {
            var frame = await _context.ProfileFrames.FindAsync(id);
            if (frame != null)
            {
                _context.ProfileFrames.Remove(frame);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Çerçeve silindi.";
            }
            return RedirectToAction(nameof(Index));
        }


        // --- AVATAR YÖNETİMİ ---

        public async Task<IActionResult> AvatarsIndex()
        {
            var avatars = await _context.ProfileAvatars.ToListAsync();
            ViewBag.Packages = await _context.StorePackages.ToListAsync();
            return View(avatars);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAvatar(ProfileAvatar avatar, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                avatar.ImageUrl = "/images/avatars/" + uniqueFileName;
            }

            ModelState.Remove("ImageUrl");
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid && !string.IsNullOrEmpty(avatar.ImageUrl))
            {
                _context.ProfileAvatars.Add(avatar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Avatar başarıyla eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen bir görsel seçin.";
            }

            return RedirectToAction(nameof(AvatarsIndex));
        }

        [HttpPost]
        public async Task<IActionResult> EditAvatar(ProfileAvatar avatar)
        {
            var existingAvatar = await _context.ProfileAvatars.FindAsync(avatar.Id);
            if (existingAvatar == null) return NotFound();

            existingAvatar.Name = avatar.Name;
            existingAvatar.Description = avatar.Description;
            existingAvatar.PriceInStones = avatar.PriceInStones;
            existingAvatar.RequiredBookCount = avatar.RequiredBookCount;
            existingAvatar.PackCategory = avatar.PackCategory;

            _context.ProfileAvatars.Update(existingAvatar);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Avatar bilgileri güncellendi.";
            return RedirectToAction(nameof(AvatarsIndex));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAvatar(int id)
        {
            var avatar = await _context.ProfileAvatars.FindAsync(id);
            if (avatar != null)
            {
                _context.ProfileAvatars.Remove(avatar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Avatar silindi.";
            }
            return RedirectToAction(nameof(AvatarsIndex));
        }

        // --- PAKET YÖNETİMİ ---

        public async Task<IActionResult> PackagesIndex()
        {
            var packages = await _context.StorePackages.ToListAsync();
            ViewBag.AllAvatars = await _context.ProfileAvatars.ToListAsync();
            ViewBag.AllFrames = await _context.ProfileFrames.ToListAsync();
            ViewBag.AllBanners = await _context.ProfileBanners.ToListAsync();
            return View(packages);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePackage(StorePackage package)
        {
            if (ModelState.IsValid)
            {
                _context.StorePackages.Add(package);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yeni paket başarıyla oluşturuldu.";
            }
            return RedirectToAction(nameof(PackagesIndex));
        }

        [HttpPost]
        public async Task<IActionResult> EditPackage(StorePackage package)
        {
            var existingPackage = await _context.StorePackages.FindAsync(package.Id);
            if (existingPackage == null) return NotFound();

            existingPackage.CategoryCode = package.CategoryCode;
            existingPackage.Title = package.Title;
            existingPackage.Description = package.Description;
            existingPackage.PriceInStones = package.PriceInStones;
            existingPackage.IconClass = package.IconClass;
            existingPackage.ThemeColor = package.ThemeColor;

            // Tür değişirse eski içerik paketten çözülür; yoksa öksüz kayıt kalır
            if (existingPackage.ItemType != package.ItemType)
            {
                await ClearPackContentAsync(existingPackage.ItemType, existingPackage.CategoryCode);
                existingPackage.ItemType = package.ItemType;
            }

            _context.StorePackages.Update(existingPackage);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Paket bilgileri güncellendi.";
            return RedirectToAction(nameof(PackagesIndex));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var package = await _context.StorePackages.FindAsync(id);
            if (package != null)
            {
                // İçerik pakete bağlı kalmasın; ürünler normal mağazaya geri döner
                await ClearPackContentAsync(package.ItemType, package.CategoryCode);
                _context.StorePackages.Remove(package);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Paket silindi, içeriği normal mağazaya döndü.";
            }
            return RedirectToAction(nameof(PackagesIndex));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePackageItems(string categoryCode, string itemType, List<int> itemIds)
        {
            if (string.IsNullOrEmpty(categoryCode)) return RedirectToAction(nameof(PackagesIndex));
            if (!PackItemTypes.All.Contains(itemType)) itemType = PackItemTypes.Avatar;

            // Önce paketin mevcut içeriği çözülür, sonra seçilenler bağlanır
            await ClearPackContentAsync(itemType, categoryCode);

            if (itemIds != null && itemIds.Any())
            {
                switch (itemType)
                {
                    case PackItemTypes.Frame:
                        foreach (var f in await _context.ProfileFrames.Where(x => itemIds.Contains(x.Id)).ToListAsync())
                            f.PackCategory = categoryCode;
                        break;
                    case PackItemTypes.Banner:
                        foreach (var b in await _context.ProfileBanners.Where(x => itemIds.Contains(x.Id)).ToListAsync())
                            b.PackCategory = categoryCode;
                        break;
                    default:
                        foreach (var a in await _context.ProfileAvatars.Where(x => itemIds.Contains(x.Id)).ToListAsync())
                            a.PackCategory = categoryCode;
                        break;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Paket içeriği başarıyla güncellendi.";
            return RedirectToAction(nameof(PackagesIndex));
        }

        /// <summary>Bir paketin o türdeki tüm içeriğini paketten çözer (PackCategory = null).</summary>
        private async Task ClearPackContentAsync(string itemType, string categoryCode)
        {
            switch (itemType)
            {
                case PackItemTypes.Frame:
                    foreach (var f in await _context.ProfileFrames.Where(x => x.PackCategory == categoryCode).ToListAsync())
                        f.PackCategory = null;
                    break;
                case PackItemTypes.Banner:
                    foreach (var b in await _context.ProfileBanners.Where(x => x.PackCategory == categoryCode).ToListAsync())
                        b.PackCategory = null;
                    break;
                default:
                    foreach (var a in await _context.ProfileAvatars.Where(x => x.PackCategory == categoryCode).ToListAsync())
                        a.PackCategory = null;
                    break;
            }
        }
    }
}
