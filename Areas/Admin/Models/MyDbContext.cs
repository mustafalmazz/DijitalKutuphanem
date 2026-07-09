using BookManagementApp.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace BookManagementApp.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<ProfileFrame> ProfileFrames { get; set; }
        public DbSet<UserFrame> UserFrames { get; set; }
        public DbSet<ProfileAvatar> ProfileAvatars { get; set; }
        public DbSet<UserAvatar> UserAvatars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<User>()
                .HasMany(u => u.Books)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFrame>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFrame>()
                .HasOne(uf => uf.ProfileFrame)
                .WithMany()
                .HasForeignKey(uf => uf.ProfileFrameId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- MAĞAZA SEED DATA ---
            modelBuilder.Entity<ProfileFrame>().HasData(
                // Seviye 1 - Başlangıç
                new ProfileFrame { Id = 1, Name = "Zümrüt Bahçesi", IconEmoji = "🌿", ImageUrl = "/images/frames/emerald.png", PriceInStones = 100, RequiredBookCount = 0, Description = "Doğanın dinginliği profilinde." },
                new ProfileFrame { Id = 2, Name = "Gün Batımı", IconEmoji = "🌅", ImageUrl = "/images/frames/sunset.png", PriceInStones = 120, RequiredBookCount = 0, Description = "Her sayfa yeni bir ufuk." },
                new ProfileFrame { Id = 3, Name = "Buz Kristali", IconEmoji = "❄️", ImageUrl = "/images/frames/ice.png", PriceInStones = 150, RequiredBookCount = 5, Description = "Soğukkanlı okurların tercihi." },
                // Seviye 2 - Gelişmiş
                new ProfileFrame { Id = 4, Name = "Kadim Bilgelik", IconEmoji = "📜", ImageUrl = "/images/frames/ancient.png", PriceInStones = 250, RequiredBookCount = 10, Description = "Asırlık kütüphanelerin ruhu." },
                new ProfileFrame { Id = 5, Name = "Kraliyet Asaleti", IconEmoji = "💜", ImageUrl = "/images/frames/royal.png", PriceInStones = 300, RequiredBookCount = 15, Description = "Altın işlemeli mor ihtişam." },
                new ProfileFrame { Id = 6, Name = "Siber Neon", IconEmoji = "🔮", ImageUrl = "/images/frames/neon.png", PriceInStones = 350, RequiredBookCount = 20, Description = "Geceleri parlayan dijital aura." },
                // Seviye 3 - Epik
                new ProfileFrame { Id = 7, Name = "Kraliyet Altını", IconEmoji = "🥇", ImageUrl = "/images/frames/gold.png", PriceInStones = 500, RequiredBookCount = 30, Description = "Parlayan saf altın ışıltısı." },
                new ProfileFrame { Id = 8, Name = "Cehennem Alevi", IconEmoji = "🔥", ImageUrl = "/images/frames/fire.png", PriceInStones = 600, RequiredBookCount = 40, Description = "Kıvılcımlar saçan okuma tutkusu." },
                new ProfileFrame { Id = 9, Name = "Gece Lordu", IconEmoji = "🌑", ImageUrl = "/images/frames/shadow.png", PriceInStones = 700, RequiredBookCount = 50, Description = "Karanlığın gizemli gücü." },
                // Seviye 4 - Efsanevi
                new ProfileFrame { Id = 10, Name = "Elmas Pırıltısı", IconEmoji = "💎", ImageUrl = "/images/frames/diamond.png", PriceInStones = 1000, RequiredBookCount = 75, Description = "Yıldız tozuyla bezeli prestij." },
                new ProfileFrame { Id = 11, Name = "Derin Uzay", IconEmoji = "🌌", ImageUrl = "/images/frames/galaxy.png", PriceInStones = 1250, RequiredBookCount = 100, Description = "Dönen nebulalar ve yıldız tarlası." },
                new ProfileFrame { Id = 12, Name = "Efsanevi Prizma", IconEmoji = "🌈", ImageUrl = "/images/frames/rainbow.png", PriceInStones = 2000, RequiredBookCount = 150, Description = "Tüm spektruma hükmedenlerin çerçevesi." }
            );

            // --- AVATAR SEED DATA ---
            modelBuilder.Entity<ProfileAvatar>().HasData(
                new ProfileAvatar { Id = 1, Name = "Klasik Prens", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/3906/3906607.png" },
                new ProfileAvatar { Id = 2, Name = "Klasik Prenses", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/4042/4042356.png" },
                new ProfileAvatar { Id = 3, Name = "Sevimli Ninja", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/523/523461.png" },
                new ProfileAvatar { Id = 4, Name = "Bilge Baykuş", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/4086/4086679.png" },
                new ProfileAvatar { Id = 5, Name = "Kitap Kurdu 1", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/4042/4042422.png" },
                new ProfileAvatar { Id = 6, Name = "Gözlüklü Bilgin", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/4086/4086577.png" },
                new ProfileAvatar { Id = 7, Name = "Şapkalı Büyücü", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/189/189162.png" },
                new ProfileAvatar { Id = 8, Name = "Gizemli Okur", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/4322/4322991.png" },
                new ProfileAvatar { Id = 9, Name = "Genç Öğrenci", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/4140/4140047.png" },
                new ProfileAvatar { Id = 10, Name = "Mutlu Çocuk", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/8854/8854242.png" },
                new ProfileAvatar { Id = 11, Name = "Kedi Dostu", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/17715/17715285.png" },
                new ProfileAvatar { Id = 12, Name = "Sevimli Köpek", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/8231/8231329.png" },
                new ProfileAvatar { Id = 13, Name = "Uzaylı Okuyucu", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/1326/1326377.png" },
                new ProfileAvatar { Id = 14, Name = "Robot Kitapkurdu", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/9308/9308979.png" },
                new ProfileAvatar { Id = 15, Name = "Canavar Okur", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/780/780258.png" },
                new ProfileAvatar { Id = 16, Name = "Sevimli Hayalet", Description = "Başlangıç Avatarı", PriceInStones = 0, RequiredBookCount = 0, ImageUrl = "https://cdn-icons-png.flaticon.com/512/624/624150.png" }
            );
        }
    }
}
