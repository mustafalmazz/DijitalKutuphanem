using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Models
{
    public class ProfileAvatar
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Avatar resmi zorunludur.")]
        public string ImageUrl { get; set; }

        public string? Name { get; set; } // Opsiyonel: Yönetici için isimlendirme kolaylığı
        
        public string? Description { get; set; }

        public int PriceInStones { get; set; } = 0; // Mağaza Fiyatı

        public int RequiredBookCount { get; set; } = 0; // Kilit Açma Şartı
        
        public string? PackCategory { get; set; } // Hangi pakete ait olduğu (Örn: "AnimalPack")
    }
}
