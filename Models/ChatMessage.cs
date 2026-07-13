using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookManagementApp.Areas.Admin.Models;

namespace BookManagementApp.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }

        public int ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
