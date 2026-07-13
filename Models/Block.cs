using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookManagementApp.Areas.Admin.Models;

namespace BookManagementApp.Models
{
    public class Block
    {
        [Key]
        public int Id { get; set; }

        public int BlockerId { get; set; }
        [ForeignKey("BlockerId")]
        public virtual User Blocker { get; set; }

        public int BlockedId { get; set; }
        [ForeignKey("BlockedId")]
        public virtual User Blocked { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
