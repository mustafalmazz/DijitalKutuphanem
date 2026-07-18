using BookManagementApp.Areas.Admin.Models;
using System.ComponentModel.DataAnnotations;

namespace BookManagementApp.Models
{
    // Kitap Akışı: kullanıcının kayıtlı kitabından fotoğraf olarak paylaştığı kesit.
    // 24 saat sonra ExcerptCleanupService tarafından (Cloudinary görseliyle birlikte) silinir.
    public class BookExcerpt
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        // Kitap bilgileri seçilen kitaptan kopyalanır (snapshot); kitap sonradan
        // silinse bile 24 saatlik kesit bozulmasın diye FK tutulmuyor.
        [StringLength(150)]
        public string BookTitle { get; set; } = string.Empty;

        [StringLength(200)]
        public string? BookAuthor { get; set; }

        [StringLength(200)]
        [Display(Name = "Not (isteğe bağlı)")]
        public string? Caption { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        // Cloudinary'den silebilmek için
        [StringLength(200)]
        public string ImagePublicId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
