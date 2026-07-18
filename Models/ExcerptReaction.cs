using BookManagementApp.Areas.Admin.Models;
using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Models
{
    // Kesitlere sadece sabit emoji setiyle tepki verilebilir; kullanıcı başına bir tepki.
    public class ExcerptReaction
    {
        public int Id { get; set; }

        public int ExcerptId { get; set; }
        public BookExcerpt? Excerpt { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [StringLength(10)]
        public string Emoji { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
