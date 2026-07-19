using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookManagementApp.Areas.Admin.Models;

namespace BookManagementApp.Models
{
    /// <summary>Kullanıcının satın aldığı banner (sahiplik kaydı). UserAvatar ile aynı desen.</summary>
    public class UserBanner
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public int ProfileBannerId { get; set; }
        [ForeignKey("ProfileBannerId")]
        public ProfileBanner ProfileBanner { get; set; } = null!;

        public DateTime PurchasedAt { get; set; } = DateTime.Now;
    }
}
