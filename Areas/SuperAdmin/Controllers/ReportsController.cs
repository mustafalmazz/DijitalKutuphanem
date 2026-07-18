using BookManagementApp.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ReportsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly Cloudinary _cloudinary;

        public ReportsController(MyDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .OrderBy(r => r.IsResolved)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Akış (kesit) şikayetleri
            var excerptReports = await _context.ExcerptReports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .OrderBy(r => r.IsResolved)
                .ThenByDescending(r => r.CreatedAt)
                .Take(200)
                .ToListAsync();

            var excerptIds = excerptReports.Select(r => r.ExcerptId).Distinct().ToList();
            ViewBag.ExcerptReports = excerptReports;
            ViewBag.LiveExcerptIds = await _context.BookExcerpts
                .Where(e => excerptIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsResolved(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.IsResolved = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Şikayet çözüldü olarak işaretlendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(int id, int reportId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = true;

            var report = await _context.Reports.FindAsync(reportId);
            if (report != null)
            {
                report.IsResolved = true;
                report.AdminNotes = "Kullanıcı süresiz banlandı.";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kullanıcı başarıyla banlandı ve sistemden uzaklaştırıldı.";
            return RedirectToAction(nameof(Index));
        }

        // --- KESİT (AKIŞ) ŞİKAYETLERİ ---

        [HttpPost]
        public async Task<IActionResult> ResolveExcerptReport(int id)
        {
            var report = await _context.ExcerptReports.FindAsync(id);
            if (report == null) return NotFound();

            report.IsResolved = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kesit şikayeti çözüldü olarak işaretlendi.";
            return RedirectToAction(nameof(Index));
        }

        // Şikayet edilen kesiti kaldırır (Cloudinary görseliyle birlikte)
        // ve o kesite ait tüm şikayetleri çözüldü işaretler.
        [HttpPost]
        public async Task<IActionResult> DeleteExcerpt(int id)
        {
            var excerpt = await _context.BookExcerpts.FirstOrDefaultAsync(e => e.Id == id);
            if (excerpt == null)
            {
                TempData["Success"] = "Kesit zaten akıştan kalkmış.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(excerpt.ImagePublicId))
            {
                try
                {
                    await _cloudinary.DestroyAsync(new DeletionParams(excerpt.ImagePublicId) { Invalidate = true });
                }
                catch (Exception)
                {
                    // Cloudinary hatası kesit silmeyi engellemesin
                }
            }

            _context.BookExcerpts.Remove(excerpt);

            var relatedReports = await _context.ExcerptReports
                .Where(r => r.ExcerptId == id && !r.IsResolved)
                .ToListAsync();
            foreach (var r in relatedReports) r.IsResolved = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Kesit kaldırıldı, ilgili şikayetler çözüldü olarak işaretlendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
