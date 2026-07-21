using System.Text.Json;
using BookManagementApp.Models;

namespace BookManagementApp.Services
{
    // Ortam seslerini wwwroot/sounds/manifest.json içinde saklayan basit depo.
    // DB tablosu/migration gerektirmez; yönetici panelinden yönetilir.
    public static class AmbientSoundStore
    {
        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static string SoundsDir(IWebHostEnvironment env) => Path.Combine(env.WebRootPath, "sounds");
        private static string ManifestPath(IWebHostEnvironment env) => Path.Combine(SoundsDir(env), "manifest.json");

        public static List<AmbientSoundItem> Load(IWebHostEnvironment env)
        {
            var path = ManifestPath(env);
            if (!File.Exists(path)) return new List<AmbientSoundItem>();
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<AmbientSoundItem>>(json, ReadOptions) ?? new List<AmbientSoundItem>();
            }
            catch
            {
                return new List<AmbientSoundItem>();
            }
        }

        public static void Save(IWebHostEnvironment env, List<AmbientSoundItem> items)
        {
            var dir = SoundsDir(env);
            Directory.CreateDirectory(dir);
            File.WriteAllText(ManifestPath(env), JsonSerializer.Serialize(items, WriteOptions));
        }

        /// <summary>Pomodoro sayfasına gömülecek JSON (camelCase: name/icon/file).</summary>
        public static string ToJson(List<AmbientSoundItem> items)
            => JsonSerializer.Serialize(items, WriteOptions);
    }
}
