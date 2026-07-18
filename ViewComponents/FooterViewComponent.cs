using BookManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookManagementApp.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly MyDbContext _context;
        private readonly Services.AchievementService _achievementService;

        public FooterViewComponent(MyDbContext context, Services.AchievementService achievementService)
        {
            _context = context;
            _achievementService = achievementService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                int requestCount = await _context.Follows.CountAsync(f => f.FollowingId == userId && !f.IsAccepted);
                ViewBag.FollowRequestCount = requestCount;

                // Toplanmayı bekleyen başarım sayısı (menü + özetim kırmızı uyarısı için)
                ViewBag.ClaimableAchievementCount = await _achievementService.GetClaimableCountAsync(userId.Value);

                // Rol, uygulamanın geri kalanıyla aynı kaynaktan (veritabanı) okunur.
                var role = await _context.Users.Where(u => u.Id == userId).Select(u => u.Role).FirstOrDefaultAsync();
                ViewBag.IsSuperAdmin = role == "SuperAdmin";
            }
            return View();
        }
    }
}
