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

        // ---- SERİ (STREAK) ----
        // Seri MUTLAK sayaçtır, asla döngüye girmez: kullanıcının gördüğü sayı budur.
        // Ödül ise haftalık döngüde ilerler; böylece uzun seride sınırsız büyümez.
        // Uzun serinin karşılığı "İstikrar" başarımlarından tek seferlik olarak verilir.

        /// <summary>
        /// Serinin haftalık ödül döngüsündeki sırası (1-7).
        /// 1.gün→1 ... 7.gün→7, 8.gün→1, 10.gün→3
        /// </summary>
        public int DayInStreakWeek => CurrentStreak <= 0 ? 1 : ((CurrentStreak - 1) % 7) + 1;

        /// <summary>
        /// Bugün toplanabilecek günlük giriş ödülü.
        /// Ekran ve sunucu AYNI kaynağı kullanmalı; ayrı formüller yazılırsa
        /// kullanıcıya gösterilen ile verilen tutar birbirinden ayrışır.
        /// </summary>
        public int DailyRewardStones => DailyRewardForWeekDay(DayInStreakWeek);

        /// <summary>Haftanın belirli bir günü için ödül (modaldaki tabloyu da bu besler).</summary>
        public static int DailyRewardForWeekDay(int weekDay)
        {
            if (weekDay < 1) weekDay = 1;
            if (weekDay > 7) weekDay = 7;
            return 10 + ((weekDay - 1) * 5);
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

        // Profil üst şeridinde gösterilen banner. null ise varsayılan gradyan kullanılır.
        // Yalnızca satın alınmış bir banner takılabilir (doğrulama StoreController.EquipBanner'da).
        public int? ActiveBannerId { get; set; }
        [ForeignKey("ActiveBannerId")]
        public ProfileBanner? ActiveBanner { get; set; }

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
