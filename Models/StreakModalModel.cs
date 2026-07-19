using BookManagementApp.Areas.Admin.Models;

namespace BookManagementApp.Models
{
    /// <summary>
    /// Seri modalının modeli. Kullanıcının yanında, sıradaki İstikrar kilometre taşını
    /// da taşır; böylece modal ayrıca sorgu açmak zorunda kalmaz.
    /// </summary>
    public class StreakModalModel
    {
        public User User { get; set; } = null!;

        /// <summary>Kullanıcının mevcut serisinden büyük ilk İstikrar başarımı. Hepsi bittiyse null.</summary>
        public Achievement? NextMilestone { get; set; }
    }
}
