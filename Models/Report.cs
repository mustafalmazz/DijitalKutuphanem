using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookManagementApp.Areas.Admin.Models;

namespace BookManagementApp.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        public int ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public virtual User Reporter { get; set; }

        public int ReportedUserId { get; set; }
        [ForeignKey("ReportedUserId")]
        public virtual User ReportedUser { get; set; }

        [Required]
        [StringLength(50)]
        public string Reason { get; set; }

        [StringLength(500)]
        public string Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsResolved { get; set; } = false;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }
}
