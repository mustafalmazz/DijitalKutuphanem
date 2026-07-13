using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookManagementApp.Areas.Admin.Models;

namespace BookManagementApp.Models
{
    public class UserAvatar
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int ProfileAvatarId { get; set; }
        [ForeignKey("ProfileAvatarId")]
        public ProfileAvatar ProfileAvatar { get; set; }
    }
}
