using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Models
{
    /// <summary>
    /// Profil üst şeridi (kapak) görseli. Avatar/çerçeve gibi mağazada satılır;
    /// yönetici panelden yükler. GIF/WebP yüklenirse tarayıcı animasyonu
    /// kendiliğinden oynatır, ek bir mekanizma gerekmez.
    /// </summary>
    public class ProfileBanner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Banner görseli zorunludur.")]
        public string ImageUrl { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public int PriceInStones { get; set; } = 0;

        public int RequiredBookCount { get; set; } = 0; // Kilit açma şartı (çerçevelerle aynı desen)

        /// <summary>Dosya uzantısından animasyon tespiti; mağazada "Animasyonlu" rozeti basar.</summary>
        public bool IsAnimated =>
            !string.IsNullOrEmpty(ImageUrl) &&
            (ImageUrl.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase) ||
             ImageUrl.EndsWith(".webp", System.StringComparison.OrdinalIgnoreCase) ||
             ImageUrl.EndsWith(".apng", System.StringComparison.OrdinalIgnoreCase));
    }
}
