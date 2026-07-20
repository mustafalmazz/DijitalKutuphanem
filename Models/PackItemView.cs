namespace BookManagementApp.Models
{
    /// <summary>
    /// Paket içeriğinin tür bağımsız gösterimi.
    /// Avatar / çerçeve / banner üçü de buna indirgenir; böylece mağaza görünümü
    /// ve gacha akışı her tür için tek kod yolundan geçer.
    /// </summary>
    public class PackItemView
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }

        /// <summary>Ait olduğu paketin CategoryCode değeri.</summary>
        public string? PackCategory { get; set; }

        /// <summary>"Avatar" | "Frame" | "Banner"</summary>
        public string ItemType { get; set; } = PackItemTypes.Avatar;

        /// <summary>Banner geniş (4:1), avatar/çerçeve kare gösterilir.</summary>
        public bool IsWide => ItemType == PackItemTypes.Banner;
    }
}
