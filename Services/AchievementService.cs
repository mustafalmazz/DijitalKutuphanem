using BookManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookManagementApp.Services
{
    public class AchievementService
    {
        private readonly MyDbContext _context;

        public AchievementService(MyDbContext context)
        {
            _context = context;
        }

        // Kategori bazlı kullanıcı metrikleri — hem ödül kontrolünde hem de
        // profil sayfasındaki ilerleme çubuklarında aynı kaynak kullanılır.
        public async Task<Dictionary<string, int>> GetUserStatsAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new Dictionary<string, int>();

            return new Dictionary<string, int>
            {
                { "Okuma", await _context.Books.CountAsync(b => b.UserId == userId) },
                { "Odaklanma", await _context.StudySessions.CountAsync(s => s.UserId == userId) },
                // Takip ETTİĞİ değil, TAKİPÇİ sayısı. Kaç kişiyi takip ettiğin
                // kendi kontrolündedir (istenirse şişirilebilir); asıl sosyal başarı
                // kaç kişinin seni takip ettiğidir.
                { "Sosyal", await _context.Follows.CountAsync(f => f.FollowingId == userId && f.IsAccepted) },
                { "Etkileşim", await _context.BookComments.CountAsync(c => c.UserId == userId) },
                // Bakiye değil toplam kazanç: mağazada harcamak başarımı geri almamalı.
                { "Bilgelik", user.TotalStonesEarned },
                { "İstikrar", user.CurrentStreak }
            };
        }

        // NOT: Otomatik ödüllendirme kaldırıldı. Başarımlar artık kullanıcı tamamladığında
        // manuel olarak toplanır (bkz. ClaimAchievementAsync). Bu metot geriye dönük uyumluluk
        // için korunuyor ancak hiçbir otomatik ödül vermez.
        public Task CheckAndAwardAchievementsAsync(int userId) => Task.CompletedTask;

        // Kullanıcının toplayabileceği (tamamlanmış ama henüz toplanmamış) başarım sayısı.
        // Menü/özetim kırmızı uyarıları ve partial içindeki tek doğruluk kaynağı budur.
        public async Task<int> GetClaimableCountAsync(int userId)
        {
            var stats = await GetUserStatsAsync(userId);
            var achievements = await _context.Achievements.ToListAsync();
            var earnedIds = (await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync()).ToHashSet();

            return achievements.Count(a =>
                !earnedIds.Contains(a.Id) &&
                stats.TryGetValue(a.Category ?? "", out int sv) &&
                sv >= a.TargetValue);
        }

        // Kullanıcının tamamladığı bir başarımın ödülünü manuel toplaması.
        public async Task<(bool Success, string Message, int RewardStones, int NewTotal)> ClaimAchievementAsync(int userId, int achievementId)
        {
            var user = await _context.Users
                .Include(u => u.UserAchievements)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return (false, "Kullanıcı bulunamadı.", 0, 0);

            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == achievementId);
            if (achievement == null) return (false, "Başarım bulunamadı.", 0, user.WisdomStones);

            // Zaten toplanmış mı? (çift toplama engeli)
            bool alreadyClaimed = user.UserAchievements?.Any(ua => ua.AchievementId == achievementId) ?? false;
            if (alreadyClaimed) return (false, "Bu başarım zaten toplandı.", 0, user.WisdomStones);

            // Hedefe gerçekten ulaşılmış mı? (sunucu tarafı güvenlik doğrulaması)
            var stats = await GetUserStatsAsync(userId);
            if (!stats.TryGetValue(achievement.Category ?? "", out int currentValue) || currentValue < achievement.TargetValue)
            {
                return (false, "Bu başarımı henüz tamamlamadınız.", 0, user.WisdomStones);
            }

            if (user.UserAchievements == null)
            {
                user.UserAchievements = new List<UserAchievement>();
            }

            user.UserAchievements.Add(new UserAchievement
            {
                UserId = user.Id,
                AchievementId = achievement.Id,
                EarnedDate = System.DateTime.Now
            });

            if (achievement.RewardStones > 0)
            {
                user.EarnStones(achievement.RewardStones);
            }

            await _context.SaveChangesAsync();

            return (true, "Ödül toplandı!", achievement.RewardStones, user.WisdomStones);
        }
    }
}
