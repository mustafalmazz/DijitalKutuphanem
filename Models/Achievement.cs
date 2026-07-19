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

        /// <summary>
        /// Kategori adından HTML çapası üretir ("İstikrar" -> "ach-istikrar").
        /// Başarımlar sayfasındaki bölüm id'si ile modaldaki bağlantı bunu ortak kullanır;
        /// tek yerde durduğu için ikisi ayrışamaz.
        /// </summary>
        public static string CategoryAnchor(string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return "ach";

            var normalized = category
                .Replace("İ", "i").Replace("I", "i").Replace("ı", "i")
                .Replace("Ş", "s").Replace("ş", "s")
                .Replace("Ğ", "g").Replace("ğ", "g")
                .Replace("Ü", "u").Replace("ü", "u")
                .Replace("Ö", "o").Replace("ö", "o")
                .Replace("Ç", "c").Replace("ç", "c")
                .ToLowerInvariant();

            var sb = new System.Text.StringBuilder("ach-");
            foreach (var ch in normalized)
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
