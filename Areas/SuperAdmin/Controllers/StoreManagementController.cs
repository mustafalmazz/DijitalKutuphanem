using BookManagementApp.Models;
using BookManagementApp.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace BookManagementApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize] // Eğer Role bazlı yetkilendirme varsa [Authorize(Roles = "SuperAdmin")] yapılabilir. Şu anlık varsayılan Authorize koyduk.
    public class StoreManagementController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StoreManagementController(MyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                _context.StorePackages.Remove(package);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Paket silindi.";
            }
            return RedirectToAction(nameof(PackagesIndex));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePackageAvatars(string categoryCode, List<int> avatarIds)
        {
            if (string.IsNullOrEmpty(categoryCode)) return RedirectToAction(nameof(PackagesIndex));

            var existingAvatars = await _context.ProfileAvatars.Where(a => a.PackCategory == categoryCode).ToListAsync();
            foreach (var avatar in existingAvatars)
            {
                avatar.PackCategory = null;
            }

            if (avatarIds != null && avatarIds.Any())
            {
                var newAvatars = await _context.ProfileAvatars.Where(a => avatarIds.Contains(a.Id)).ToListAsync();
                foreach (var avatar in newAvatars)
                {
                    avatar.PackCategory = categoryCode;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Paket içeriği başarıyla güncellendi.";
            return RedirectToAction(nameof(PackagesIndex));
        }
    }
}
