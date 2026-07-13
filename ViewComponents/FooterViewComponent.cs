using BookManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookManagementApp.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly MyDbContext _context;

        public FooterViewComponent(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                int requestCount = await _context.Follows.CountAsync(f => f.FollowingId == userId && !f.IsAccepted);
                ViewBag.FollowRequestCount = requestCount;
            }
            return View();
        }
    }
}
