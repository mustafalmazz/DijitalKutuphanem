using BookManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookManagementApp.Areas.Admin.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kullanıcı adı zorunludur."),Display(Name ="Kullanıcı Adı")]
        [StringLength(50)]
        public string? UserName { get; set; }
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Şifre zorunludur."), Display(Name = "Şifre")]
        [StringLength(100)]
        public string? PasswordHash { get; set; }
        [Compare("PasswordHash", ErrorMessage = "Şifreler eşleşmiyor."), Display(Name = "Şifre Tekrar")]
        [NotMapped]
        public string? ConfirmPassword { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
        [StringLength(20)]
        public string Role { get; set; } = "User";
        public string? ProfileImageUrl { get; set; }
        public int YearlyReadingGoal { get; set; }
        public int MonthlyReadingGoal { get; set; }
        public int MonthlyPagesRead { get; set; }
        
        // Harcanabilir bakiye. Mağazada azalır.
        public int WisdomStones { get; set; } = 0;

        // Kullanıcının bugüne kadar KAZANDIĞI toplam taş. Asla azalmaz.
        // İlerlemeye bakan her şey (Bilgelik başarımları vb.) bunu kullanmalı;
        // WisdomStones harcanabildiği için ilerleme ölçütü olamaz.
        public int TotalStonesEarned { get; set; } = 0;

        /// <summary>
        /// Taş kazandırmanın TEK doğru yolu. Hem bakiyeyi hem toplam kazancı birlikte artırır.
        /// Doğrudan WisdomStones += yazmayın; ikisi ayrışır ve ilerleme bozulur.
        /// </summary>
        public void EarnStones(int amount)
        {
            if (amount <= 0) return;
            WisdomStones += amount;
            TotalStonesEarned += amount;
        }

        public DateTime? LastLoginDate { get; set; }
        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;
        public int StreakFreezes { get; set; } = 0;
        public DateTime? LastRewardClaimDate { get; set; }
        [StringLength(255)]
        public string? ActiveFrameImageUrl { get; set; }

        // Kullanıcının profilinde göstermeyi seçtiği başarım etiketi.
        // null ise profilde etiket gösterilmez. Yalnızca kazanılmış bir başarım takılabilir
        // (doğrulama ProfileController.EquipTitle içinde yapılır).
        public int? ActiveTitleAchievementId { get; set; }
        [ForeignKey("ActiveTitleAchievementId")]
        public Achievement? ActiveTitleAchievement { get; set; }

        [StringLength(160, ErrorMessage = "Biyografi en fazla 160 karakter olabilir.")]
        public string? Bio { get; set; }

        public bool IsPrivate { get; set; } = true;

        public bool IsBanned { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public ICollection<Book>? Books { get; set; }
        public ICollection<Contact>? Contacts { get; set; } 
        public ICollection<UserAchievement>? UserAchievements { get; set; }
    }
}
