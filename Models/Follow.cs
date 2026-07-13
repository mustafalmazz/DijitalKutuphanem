using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookManagementApp.Areas.Admin.Models;

namespace BookManagementApp.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }

        public int FollowerId { get; set; }
        [ForeignKey("FollowerId")]
        public virtual User Follower { get; set; }

        public int FollowingId { get; set; }
        [ForeignKey("FollowingId")]
        public virtual User Following { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsAccepted { get; set; } = false;
    }
}
