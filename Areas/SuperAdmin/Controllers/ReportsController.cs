using BookManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    public class ReportsController : Controller
    {
        private readonly MyDbContext _context;

        public ReportsController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .OrderBy(r => r.IsResolved)
                .ThenByDescending(r => r.CreatedAt)
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
    }
}

