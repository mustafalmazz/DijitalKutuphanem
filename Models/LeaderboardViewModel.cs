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
