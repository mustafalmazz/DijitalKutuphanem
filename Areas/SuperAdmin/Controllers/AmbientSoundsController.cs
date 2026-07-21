using BookManagementApp.Models;
using BookManagementApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AmbientSoundsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        private const long MaxAudioBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExt = { ".mp3", ".ogg", ".wav", ".m4a", ".aac", ".webm" };

        public AmbientSoundsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View(AmbientSoundStore.Load(_env));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Create(string name, string? icon, IFormFile audioFile)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Ses adı zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            if (audioFile == null || audioFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir ses dosyası seçin.";
                return RedirectToAction(nameof(Index));
            }

            var ext = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
            {
                TempData["ErrorMessage"] = "Yalnızca MP3, OGG, WAV, M4A, AAC veya WEBM yükleyebilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (audioFile.Length > MaxAudioBytes)
            {
                TempData["ErrorMessage"] = "Ses dosyası en fazla 10 MB olabilir.";
                return RedirectToAction(nameof(Index));
            }

            var soundsDir = Path.Combine(_env.WebRootPath, "sounds");
            Directory.CreateDirectory(soundsDir);

            var uniqueName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(soundsDir, uniqueName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            var items = AmbientSoundStore.Load(_env);
            items.Add(new AmbientSoundItem
            {
                Name = name.Trim(),
                Icon = string.IsNullOrWhiteSpace(icon) ? "fa-solid fa-music" : icon.Trim(),
                File = "/sounds/" + uniqueName
            });
            AmbientSoundStore.Save(_env, items);

            TempData["SuccessMessage"] = "Ortam sesi eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return RedirectToAction(nameof(Index));

            var items = AmbientSoundStore.Load(_env);
            var item = items.FirstOrDefault(i => i.File == file);
            if (item != null)
            {
                items.Remove(item);
                AmbientSoundStore.Save(_env, items);

                // Fiziksel dosyayı da sil (yalnızca sounds klasöründeki dosya adıyla)
                try
                {
                    var physical = Path.Combine(_env.WebRootPath, "sounds", Path.GetFileName(item.File));
                    if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
                }
                catch { /* dosya silinemese de kayıt kaldırıldı */ }

                TempData["SuccessMessage"] = "Ortam sesi silindi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
