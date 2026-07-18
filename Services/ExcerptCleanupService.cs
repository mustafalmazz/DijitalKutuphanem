using BookManagementApp.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.Services
{
    // 24 saati dolan kesitleri hem veritabanından hem Cloudinary'den siler.
    // Yarım saatte bir çalışır; akış sorgusu zaten 24 saat filtresi uyguladığı
    // için kullanıcılar temizlik gecikmesini fark etmez.
    public class ExcerptCleanupService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<ExcerptCleanupService> _logger;

        public ExcerptCleanupService(IServiceScopeFactory scopeFactory, Cloudinary cloudinary,
            ILogger<ExcerptCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _cloudinary = cloudinary;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesit temizliği sırasında hata oluştu.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task CleanupAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            var cutoff = DateTime.Now - MaxAge;
            var expired = await context.BookExcerpts
                .Where(e => e.CreatedAt < cutoff)
                .ToListAsync(ct);

            if (expired.Count == 0) return;

            foreach (var excerpt in expired)
            {
                if (!string.IsNullOrEmpty(excerpt.ImagePublicId))
                {
                    try
                    {
                        // Invalidate: CDN önbelleğindeki kopya da temizlensin
                        await _cloudinary.DestroyAsync(new DeletionParams(excerpt.ImagePublicId) { Invalidate = true });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Cloudinary görseli silinemedi: {PublicId}", excerpt.ImagePublicId);
                    }
                }
            }

            // Tepkiler FK cascade ile birlikte silinir
            context.BookExcerpts.RemoveRange(expired);
            await context.SaveChangesAsync(ct);

            _logger.LogInformation("{Count} adet süresi dolan kesit temizlendi.", expired.Count);
        }
    }
}
