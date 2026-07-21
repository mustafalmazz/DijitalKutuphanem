namespace BookManagementApp.Models
{
    // Pomodoro "Ortam Sesleri" mikserindeki tek bir ses. Yönetici panelinden eklenir,
    // wwwroot/sounds/manifest.json içinde tutulur (ayrı bir DB tablosu gerektirmez).
    public class AmbientSoundItem
    {
        public string Name { get; set; } = string.Empty;      // Örn: "Yağmur"
        public string Icon { get; set; } = "fa-solid fa-music"; // FontAwesome sınıfı
        public string File { get; set; } = string.Empty;       // Örn: "/sounds/xxxx.mp3"
    }
}
