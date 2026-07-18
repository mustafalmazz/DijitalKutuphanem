using System.Collections.Generic;

namespace BookManagementApp.Models
{
    public class LeaderboardUserItem
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ActiveFrameImageUrl { get; set; }
        public int Score { get; set; } // Dakika, Kitap Sayısı veya Gün Serisi olabilir
        public int Rank { get; set; }

        // Kullanıcının profilinde göstermeyi seçtiği başarım etiketi (yoksa null)
        public string? TitleName { get; set; }
        public string? TitleIcon { get; set; }
        public string? TitleColor { get; set; }
        public int TitleTier { get; set; }
        public string? TitleDescription { get; set; }
    }

    public class LeaderboardViewModel
    {
        public List<LeaderboardUserItem> TopStudiers { get; set; } = new List<LeaderboardUserItem>();
        public List<LeaderboardUserItem> TopBookworms { get; set; } = new List<LeaderboardUserItem>();
        public List<LeaderboardUserItem> LongestStreaks { get; set; } = new List<LeaderboardUserItem>();

        public LeaderboardUserItem? CurrentUserBookworm { get; set; }
        public LeaderboardUserItem? CurrentUserStudier { get; set; }
        public LeaderboardUserItem? CurrentUserStreak { get; set; }
    }
}
