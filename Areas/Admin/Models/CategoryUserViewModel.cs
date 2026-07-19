using BookManagementApp.Models;

namespace BookManagementApp.Areas.Admin.Models
{
    public class CategoryUserViewModel
    {
        public IEnumerable<Category>? Categories { get; set; }
        public User? User; 

        /// <summary>Seri modalı için hazırlanmış veri (kullanıcı + sıradaki kilometre taşı).</summary>
        public StreakModalModel? StreakModal { get; set; }
    }
}
