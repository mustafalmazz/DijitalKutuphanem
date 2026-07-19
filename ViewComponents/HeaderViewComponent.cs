using BookManagementApp.Areas.Admin.Models;
using BookManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly MyDbContext _context;
        public HeaderViewComponent(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var viewModel = new CategoryUserViewModel();

            // Kategoriler global; oturumdan bağımsız olarak herkese gösterilir.
            // (Önceden yalnızca giriş yapmışlara yükleniyordu ve anonim kullanıcıda
            //  header'daki Categories.Any() null referansla patlıyordu.)
            viewModel.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            if (userId != null)
            {
                viewModel.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                int requestCount = await _context.Follows.CountAsync(f => f.FollowingId == userId && !f.IsAccepted);
                ViewBag.FollowRequestCount = requestCount;

                if (viewModel.User != null)
                {
                    // Seri modalında gösterilecek "sıradaki hedef": mevcut seriden büyük
                    // ilk İstikrar başarımı. Hepsi tamamlandıysa null kalır.
                    var next = await _context.Achievements
                        .Where(a => a.Category == "İstikrar" && a.TargetValue > viewModel.User.CurrentStreak)
                        .OrderBy(a => a.TargetValue)
                        .FirstOrDefaultAsync();

                    viewModel.StreakModal = new BookManagementApp.Models.StreakModalModel
                    {
                        User = viewModel.User,
                        NextMilestone = next
                    };
                }
            }
            return View(viewModel);
        }
    }
}
