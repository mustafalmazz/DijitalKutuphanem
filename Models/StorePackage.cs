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
    }
}
