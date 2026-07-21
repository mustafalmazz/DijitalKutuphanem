using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Models
{
    public class StorePackage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Paket Kodu zorunludur.")]
        public string CategoryCode { get; set; } // Örn: "AnimalPack"

        [Required(ErrorMessage = "Paket Başlığı zorunludur.")]
        public string Title { get; set; } // Örn: "Hayvanlar Paketi"

        public string? Description { get; set; } // Örn: "İçerisinden rastgele hayvan avatarı çıkar!"

        public int PriceInStones { get; set; } = 300; // Paket Fiyatı

        public string? IconClass { get; set; } // Örn: "fa-solid fa-box" veya "fa-solid fa-paw"
        
        public string? ThemeColor { get; set; } // Örn: "#ff9800"

        /// <summary>
        /// Paketin dağıttığı ürün türü: "Avatar", "Frame" veya "Banner".
        /// Mevcut paketler avatar olduğu için varsayılan "Avatar".
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ItemType { get; set; } = PackItemTypes.Avatar;

        /// <summary>
        /// Paketin son geçerlilik tarihi. null ise paket süresizdir (hep görünür).
        /// Bu tarih geçtiğinde paket Packs sayfasında gizlenir; kayıt ve içeriği silinmez,
        /// admin tarihi güncelleyerek paketi tekrar yayınlayabilir.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>Paketin süresi dolmuş mu? null tarih hiç dolmaz.</summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.Now;
    }

    /// <summary>Paket türleri tek yerde; string sabitleri kod içine dağılmasın.</summary>
    public static class PackItemTypes
    {
        public const string Avatar = "Avatar";
        public const string Frame  = "Frame";
        public const string Banner = "Banner";

        public static readonly string[] All = { Avatar, Frame, Banner };

        public static string DisplayName(string? type) => type switch
        {
            Frame  => "Çerçeve",
            Banner => "Banner",
            _      => "Avatar"
        };

        public static string Icon(string? type) => type switch
        {
            Frame  => "fa-solid fa-crown",
            Banner => "fa-solid fa-panorama",
            _      => "fa-solid fa-user-astronaut"
        };
    }
}
