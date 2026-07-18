using BookManagementApp.Areas.Admin.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookManagementApp.Models
{
    // Kesit görseli şikayeti. Kesit 24 saat sonra otomatik silindiği için
    // ExcerptId'ye FK konulmuyor; görsel URL'si ve paylaşan kullanıcı
    // kayıt olarak saklanır ki admin sonradan da inceleyebilsin.
    public class ExcerptReport
    {
        public int Id { get; set; }

        public int ExcerptId { get; set; }

        public int ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }

        public int ReportedUserId { get; set; }
        [ForeignKey("ReportedUserId")]
        public User? ReportedUser { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsResolved { get; set; } = false;
    }
}
