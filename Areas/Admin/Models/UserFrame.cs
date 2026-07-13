using BookManagementApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookManagementApp.Areas.Admin.Models
{
    public class UserFrame
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int ProfileFrameId { get; set; }
        [ForeignKey("ProfileFrameId")]
        public ProfileFrame? ProfileFrame { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.Now;
    }
}
