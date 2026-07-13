using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Areas.Admin.Models
{
    public class ProfileFrame
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int PriceInStones { get; set; }

        public int RequiredBookCount { get; set; } = 0;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [StringLength(50)]
        public string? IconEmoji { get; set; }
    }
}
