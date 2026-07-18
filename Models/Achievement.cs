using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Models
{
    public class Achievement
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(50)]
        public string IconClass { get; set; } // Örn: "fa-medal"

        [StringLength(7)]
        public string ColorHex { get; set; } // Örn: "#FFD700"

        [StringLength(50)]
        public string Category { get; set; } // Örn: "Okuma", "Sosyal", "Odaklanma", "Zenginlik", "İstikrar"

        public int Tier { get; set; } // 1: Bronz, 2: Gümüş, 3: Altın, 4: Elmas vs.

        public int TargetValue { get; set; } // Kazanmak için gereken hedef rakam

        public int RewardStones { get; set; } // Kazanınca verilecek Bilgelik Taşı
    }
}
