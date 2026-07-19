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
        public DbSet<StorePackage> StorePackages { get; set; }
        public DbSet<UserAvatar> UserAvatars { get; set; }
        public DbSet<ProfileBanner> ProfileBanners { get; set; }
        public DbSet<UserBanner> UserBanners { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<BookLike> BookLikes { get; set; }
        public DbSet<BookComment> BookComments { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<BookExcerpt> BookExcerpts { get; set; }
        public DbSet<ExcerptReaction> ExcerptReactions { get; set; }
        public DbSet<ExcerptReport> ExcerptReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<BookExcerpt>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExcerptReaction>()
                .HasIndex(r => new { r.ExcerptId, r.UserId })
                .IsUnique();

            modelBuilder.Entity<ExcerptReaction>()
                .HasOne(r => r.Excerpt)
                .WithMany()
                .HasForeignKey(r => r.ExcerptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExcerptReaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExcerptReport>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExcerptReport>()
                .HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Achievement>().HasData(
                // ------------------ OKUMA SERİSİ ------------------
                new Achievement { Id = 1, Name = "İlk Adım", Description = "İlk kitabınızı okudunuz.", IconClass = "fa-book-open", ColorHex = "#cd7f32", Category = "Okuma", Tier = 1, TargetValue = 1, RewardStones = 10 },
                new Achievement { Id = 2, Name = "Kitap Sever", Description = "10 kitap okudunuz.", IconClass = "fa-book", ColorHex = "#c0c0c0", Category = "Okuma", Tier = 2, TargetValue = 10, RewardStones = 50 },
                new Achievement { Id = 3, Name = "Kütüphaneci", Description = "50 kitap okudunuz.", IconClass = "fa-layer-group", ColorHex = "#ffd700", Category = "Okuma", Tier = 3, TargetValue = 50, RewardStones = 150 },
                new Achievement { Id = 4, Name = "Kitap Kurdu", Description = "100 kitap okudunuz.", IconClass = "fa-book-journal-whills", ColorHex = "#2fb8a0", Category = "Okuma", Tier = 4, TargetValue = 100, RewardStones = 500 },
                new Achievement { Id = 24, Name = "Raf Fatihi", Description = "250 kitap okudunuz.", IconClass = "fa-book-bookmark", ColorHex = "#b9f2ff", Category = "Okuma", Tier = 5, TargetValue = 250, RewardStones = 1000 },
                new Achievement { Id = 5, Name = "Okuma Üstadı", Description = "500 kitap okudunuz.", IconClass = "fa-book-atlas", ColorHex = "#c084fc", Category = "Okuma", Tier = 6, TargetValue = 500, RewardStones = 2000 },

                // ------------------ ODAKLANMA SERİSİ (POMODORO) ------------------
                new Achievement { Id = 6, Name = "Odak Çırağı", Description = "İlk pomodoro seansınızı tamamladınız.", IconClass = "fa-stopwatch", ColorHex = "#cd7f32", Category = "Odaklanma", Tier = 1, TargetValue = 1, RewardStones = 10 },
                new Achievement { Id = 7, Name = "Zaman Bekçisi", Description = "25 pomodoro seansı tamamladınız.", IconClass = "fa-hourglass-half", ColorHex = "#c0c0c0", Category = "Odaklanma", Tier = 2, TargetValue = 25, RewardStones = 50 },
                new Achievement { Id = 8, Name = "Zamanın Hakimi", Description = "100 pomodoro seansı tamamladınız.", IconClass = "fa-hourglass-end", ColorHex = "#ffd700", Category = "Odaklanma", Tier = 3, TargetValue = 100, RewardStones = 250 },
                new Achievement { Id = 9, Name = "Odaklanma Ustası", Description = "500 pomodoro seansı tamamladınız.", IconClass = "fa-brain", ColorHex = "#2fb8a0", Category = "Odaklanma", Tier = 4, TargetValue = 500, RewardStones = 1000 },
                new Achievement { Id = 25, Name = "Derin Odak", Description = "1000 pomodoro seansı tamamladınız.", IconClass = "fa-meteor", ColorHex = "#b9f2ff", Category = "Odaklanma", Tier = 5, TargetValue = 1000, RewardStones = 2000 },
                new Achievement { Id = 26, Name = "Zamanın Efsanesi", Description = "2500 pomodoro seansı tamamladınız.", IconClass = "fa-infinity", ColorHex = "#c084fc", Category = "Odaklanma", Tier = 6, TargetValue = 2500, RewardStones = 5000 },

                // ------------------ SOSYAL SERİ ------------------
                new Achievement { Id = 10, Name = "Merhaba Dünya", Description = "İlk takipçinizi kazandınız.", IconClass = "fa-user-plus", ColorHex = "#cd7f32", Category = "Sosyal", Tier = 1, TargetValue = 1, RewardStones = 10 },
                new Achievement { Id = 11, Name = "Sosyalleşen", Description = "10 takipçiye ulaştınız.", IconClass = "fa-users", ColorHex = "#c0c0c0", Category = "Sosyal", Tier = 2, TargetValue = 10, RewardStones = 40 },
                new Achievement { Id = 12, Name = "Çevresi Geniş", Description = "50 takipçiye ulaştınız.", IconClass = "fa-people-group", ColorHex = "#ffd700", Category = "Sosyal", Tier = 3, TargetValue = 50, RewardStones = 100 },
                new Achievement { Id = 13, Name = "Fenomen", Description = "100 takipçiye ulaştınız.", IconClass = "fa-star", ColorHex = "#2fb8a0", Category = "Sosyal", Tier = 4, TargetValue = 100, RewardStones = 500 },
                new Achievement { Id = 27, Name = "Topluluk Yıldızı", Description = "250 takipçiye ulaştınız.", IconClass = "fa-handshake", ColorHex = "#b9f2ff", Category = "Sosyal", Tier = 5, TargetValue = 250, RewardStones = 1000 },
                new Achievement { Id = 28, Name = "Kitap Kulübü Efsanesi", Description = "500 takipçiye ulaştınız.", IconClass = "fa-heart", ColorHex = "#c084fc", Category = "Sosyal", Tier = 6, TargetValue = 500, RewardStones = 2500 },

                // ------------------ ETKİLEŞİM SERİSİ (YORUM/İNCELEME) ------------------
                new Achievement { Id = 14, Name = "İlk Bakış", Description = "İlk kitap yorumunuzu yaptınız.", IconClass = "fa-comment", ColorHex = "#cd7f32", Category = "Etkileşim", Tier = 1, TargetValue = 1, RewardStones = 15 },
                new Achievement { Id = 15, Name = "Eleştirmen", Description = "10 kitap yorumu yaptınız.", IconClass = "fa-comments", ColorHex = "#c0c0c0", Category = "Etkileşim", Tier = 2, TargetValue = 10, RewardStones = 75 },
                new Achievement { Id = 16, Name = "Uzman Yazar", Description = "50 kitap yorumu yaptınız.", IconClass = "fa-pen-fancy", ColorHex = "#ffd700", Category = "Etkileşim", Tier = 3, TargetValue = 50, RewardStones = 200 },
                new Achievement { Id = 17, Name = "Edebiyat Dedektifi", Description = "100 kitap yorumu yaptınız.", IconClass = "fa-magnifying-glass-chart", ColorHex = "#2fb8a0", Category = "Etkileşim", Tier = 4, TargetValue = 100, RewardStones = 500 },
                new Achievement { Id = 29, Name = "Kalem Ustası", Description = "250 kitap yorumu yaptınız.", IconClass = "fa-feather", ColorHex = "#b9f2ff", Category = "Etkileşim", Tier = 5, TargetValue = 250, RewardStones = 1000 },
                new Achievement { Id = 30, Name = "Edebiyatın Sesi", Description = "500 kitap yorumu yaptınız.", IconClass = "fa-scroll", ColorHex = "#c084fc", Category = "Etkileşim", Tier = 6, TargetValue = 500, RewardStones = 2500 },

                // ------------------ BİLGELİK / YATIRIM SERİSİ ------------------
                new Achievement { Id = 18, Name = "Çaylak", Description = "100 Bilgelik taşı topladınız.", IconClass = "fa-gem", ColorHex = "#cd7f32", Category = "Bilgelik", Tier = 1, TargetValue = 100, RewardStones = 0 },
                new Achievement { Id = 19, Name = "Bilge Kişi", Description = "1000 Bilgelik taşı topladınız.", IconClass = "fa-medal", ColorHex = "#c0c0c0", Category = "Bilgelik", Tier = 2, TargetValue = 1000, RewardStones = 0 },
                new Achievement { Id = 20, Name = "Aydınlanmış", Description = "5000 Bilgelik taşı topladınız.", IconClass = "fa-crown", ColorHex = "#ffd700", Category = "Bilgelik", Tier = 3, TargetValue = 5000, RewardStones = 0 },
                new Achievement { Id = 21, Name = "Bilgeliğin Efendisi", Description = "10000 Bilgelik taşı topladınız.", IconClass = "fa-chess-king", ColorHex = "#2fb8a0", Category = "Bilgelik", Tier = 4, TargetValue = 10000, RewardStones = 0 },
                new Achievement { Id = 31, Name = "Hazine Avcısı", Description = "25000 Bilgelik taşı topladınız.", IconClass = "fa-coins", ColorHex = "#b9f2ff", Category = "Bilgelik", Tier = 5, TargetValue = 25000, RewardStones = 0 },
                new Achievement { Id = 32, Name = "Ebedi Bilge", Description = "50000 Bilgelik taşı topladınız.", IconClass = "fa-hat-wizard", ColorHex = "#c084fc", Category = "Bilgelik", Tier = 6, TargetValue = 50000, RewardStones = 0 },

                // ------------------ İSTİKRAR (STREAK) SERİSİ ------------------
                new Achievement { Id = 33, Name = "İlk Kıvılcım", Description = "3 gün üst üste uygulamayı kullandınız.", IconClass = "fa-bolt", ColorHex = "#cd7f32", Category = "İstikrar", Tier = 1, TargetValue = 3, RewardStones = 25 },
                new Achievement { Id = 22, Name = "İstikrarlı", Description = "7 gün üst üste uygulamayı kullandınız.", IconClass = "fa-calendar-check", ColorHex = "#c0c0c0", Category = "İstikrar", Tier = 2, TargetValue = 7, RewardStones = 100 },
                new Achievement { Id = 23, Name = "Demir Disiplin", Description = "30 gün üst üste uygulamayı kullandınız.", IconClass = "fa-fire", ColorHex = "#ffd700", Category = "İstikrar", Tier = 3, TargetValue = 30, RewardStones = 500 },
                new Achievement { Id = 34, Name = "Sarsılmaz", Description = "100 gün üst üste uygulamayı kullandınız.", IconClass = "fa-shield-halved", ColorHex = "#2fb8a0", Category = "İstikrar", Tier = 4, TargetValue = 100, RewardStones = 1500 },
                new Achievement { Id = 35, Name = "Adanmış Okur", Description = "180 gün üst üste uygulamayı kullandınız.", IconClass = "fa-mountain", ColorHex = "#b9f2ff", Category = "İstikrar", Tier = 5, TargetValue = 180, RewardStones = 3000 },
                new Achievement { Id = 36, Name = "Bir Yılın Hikayesi", Description = "365 gün üst üste uygulamayı kullandınız.", IconClass = "fa-trophy", ColorHex = "#c084fc", Category = "İstikrar", Tier = 6, TargetValue = 365, RewardStones = 10000 }
            );

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

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany()
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookLike>()
                .HasOne(bl => bl.User)
                .WithMany()
                .HasForeignKey(bl => bl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookLike>()
                .HasOne(bl => bl.Book)
                .WithMany()
                .HasForeignKey(bl => bl.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookComment>()
                .HasOne(bc => bc.User)
                .WithMany()
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookComment>()
                .HasOne(bc => bc.Book)
                .WithMany()
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Block>()
                .HasOne(b => b.Blocker)
                .WithMany()
                .HasForeignKey(b => b.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Block>()
                .HasOne(b => b.Blocked)
                .WithMany()
                .HasForeignKey(b => b.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

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
