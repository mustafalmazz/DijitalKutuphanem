namespace BookManagementApp.Models
{
    // Akış sayfası ve "daha fazla yükle" partial'ının ortak modeli
    public class ExcerptFeedPageViewModel
    {
        public List<BookExcerpt> Excerpts { get; set; } = new();
        public Dictionary<int, Dictionary<string, int>> ReactionCounts { get; set; } = new();
        public Dictionary<int, string> MyReactions { get; set; } = new();
        public int CurrentUserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
