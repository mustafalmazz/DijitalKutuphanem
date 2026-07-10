using BookManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookManagementApp.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly MyDbContext _context;

        public LeaderboardController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new LeaderboardViewModel();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            // =========================================================
            // 1. Ayın Kitap Kurtları
            // =========================================================
            var allBookworms = await _context.Books
                .Where(b => b.CreateDate.Month == currentMonth && b.CreateDate.Year == currentYear && b.UserId != null)
                .GroupBy(b => b.UserId)
                .Select(g => new { UserId = g.Key.Value, BookCount = g.Count() })
                .OrderByDescending(x => x.BookCount)
                .ToListAsync();

            var topBookworms = allBookworms.Take(10).ToList();
            int rank = 1;
            foreach (var item in topBookworms)
            {
                var user = await _context.Users.FindAsync(item.UserId);
                if (user != null)
                {
                    viewModel.TopBookworms.Add(new LeaderboardUserItem
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        ProfileImageUrl = user.ProfileImageUrl,
                        ActiveFrameImageUrl = user.ActiveFrameImageUrl,
                        Score = item.BookCount,
                        Rank = rank++
                    });
                }
            }

            if (currentUserId.HasValue)
            {
                var userIndex = allBookworms.FindIndex(x => x.UserId == currentUserId.Value);
                if (userIndex >= 10 || userIndex == -1)
                {
                    var u = await _context.Users.FindAsync(currentUserId.Value);
                    if (u != null)
                    {
                        viewModel.CurrentUserBookworm = new LeaderboardUserItem
                        {
                            UserId = u.Id,
                            UserName = u.UserName,
                            ProfileImageUrl = u.ProfileImageUrl,
                            ActiveFrameImageUrl = u.ActiveFrameImageUrl,
                            Score = userIndex != -1 ? allBookworms[userIndex].BookCount : 0,
                            Rank = userIndex != -1 ? userIndex + 1 : allBookworms.Count + 1
                        };
                    }
                }
            }

            // =========================================================
            // 2. En Çalışkanlar
            // =========================================================
            var allStudiers = await _context.StudySessions
                .Where(s => s.IsCompleted && (s.SessionType == "Pomodoro" || s.SessionType == "Stopwatch") && s.CreatedAt.Month == currentMonth && s.CreatedAt.Year == currentYear)
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, TotalMinutes = g.Sum(s => s.DurationInMinutes) })
                .OrderByDescending(x => x.TotalMinutes)
                .ToListAsync();

            var topStudiers = allStudiers.Take(10).ToList();
            rank = 1;
            foreach (var item in topStudiers)
            {
                var user = await _context.Users.FindAsync(item.UserId);
                if (user != null)
                {
                    viewModel.TopStudiers.Add(new LeaderboardUserItem
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        ProfileImageUrl = user.ProfileImageUrl,
                        ActiveFrameImageUrl = user.ActiveFrameImageUrl,
                        Score = item.TotalMinutes,
                        Rank = rank++
                    });
                }
            }

            if (currentUserId.HasValue)
            {
                var userIndex = allStudiers.FindIndex(x => x.UserId == currentUserId.Value);
                if (userIndex >= 10 || userIndex == -1)
                {
                    var u = await _context.Users.FindAsync(currentUserId.Value);
                    if (u != null)
                    {
                        viewModel.CurrentUserStudier = new LeaderboardUserItem
                        {
                            UserId = u.Id,
                            UserName = u.UserName,
                            ProfileImageUrl = u.ProfileImageUrl,
                            ActiveFrameImageUrl = u.ActiveFrameImageUrl,
                            Score = userIndex != -1 ? allStudiers[userIndex].TotalMinutes : 0,
                            Rank = userIndex != -1 ? userIndex + 1 : allStudiers.Count + 1
                        };
                    }
                }
            }

            // =========================================================
            // 3. En Uzun Seriler
            // =========================================================
            var allStreaks = await _context.Users
                .Where(u => u.LongestStreak > 0)
                .OrderByDescending(u => u.LongestStreak)
                .ToListAsync();

            var topStreaks = allStreaks.Take(10).ToList();
            rank = 1;
            foreach (var user in topStreaks)
            {
                viewModel.LongestStreaks.Add(new LeaderboardUserItem
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    ActiveFrameImageUrl = user.ActiveFrameImageUrl,
                    Score = user.LongestStreak,
                    Rank = rank++
                });
            }

            if (currentUserId.HasValue)
            {
                var userIndex = allStreaks.FindIndex(x => x.Id == currentUserId.Value);
                if (userIndex >= 10 || userIndex == -1)
                {
                    var u = await _context.Users.FindAsync(currentUserId.Value);
                    if (u != null)
                    {
                        viewModel.CurrentUserStreak = new LeaderboardUserItem
                        {
                            UserId = u.Id,
                            UserName = u.UserName,
                            ProfileImageUrl = u.ProfileImageUrl,
                            ActiveFrameImageUrl = u.ActiveFrameImageUrl,
                            Score = u.LongestStreak, // even if 0
                            Rank = userIndex != -1 ? userIndex + 1 : allStreaks.Count + 1
                        };
                    }
                }
            }

            return View(viewModel);
        }
    }
}
