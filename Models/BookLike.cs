using BookManagementApp.Areas.Admin.Models;
using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Models
{
    public class BookLike
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Book Book { get; set; }
        public virtual User User { get; set; }
    }
}
