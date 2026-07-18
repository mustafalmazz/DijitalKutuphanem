using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Achievements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RewardStones",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetValue",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tier",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "ColorHex", "Description", "RewardStones", "TargetValue", "Tier" },
                values: new object[] { "Okuma", "#cd7f32", "İlk kitabınızı okudunuz.", 10, 1, 1 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "ColorHex", "Description", "Name", "RewardStones", "TargetValue", "Tier" },
                values: new object[] { "Okuma", "#c0c0c0", "10 kitap okudunuz.", "Kitap Sever", 50, 10, 2 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "ColorHex", "Description", "IconClass", "Name", "RewardStones", "TargetValue", "Tier" },
                values: new object[] { "Okuma", "#ffd700", "50 kitap okudunuz.", "fa-layer-group", "Kütüphaneci", 150, 50, 3 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "ColorHex", "Description", "IconClass", "Name", "RewardStones", "TargetValue", "Tier" },
                values: new object[] { "Okuma", "#e5e4e2", "100 kitap okudunuz.", "fa-book-journal-whills", "Kitap Kurdu", 500, 100, 4 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "ColorHex", "Description", "IconClass", "Name", "RewardStones", "TargetValue", "Tier" },
                values: new object[] { "Okuma", "#b9f2ff", "500 kitap okudunuz.", "fa-book-atlas", "Okuma Üstadı", 2000, 500, 5 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "ColorHex", "Description", "IconClass", "Name", "RewardStones", "TargetValue", "Tier" },
                values: new object[] { "Odaklanma", "#cd7f32", "İlk pomodoro seansınızı tamamladınız.", "fa-stopwatch", "Odak Çırağı", 10, 1, 1 });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Category", "ColorHex", "Description", "IconClass", "Name", "RewardStones", "TargetValue", "Tier" },
                values: new object[,]
                {
                    { 7, "Odaklanma", "#c0c0c0", "25 pomodoro seansı tamamladınız.", "fa-hourglass-half", "Zaman Bekçisi", 50, 25, 2 },
                    { 8, "Odaklanma", "#ffd700", "100 pomodoro seansı tamamladınız.", "fa-hourglass-end", "Zamanın Hakimi", 250, 100, 3 },
                    { 9, "Odaklanma", "#e5e4e2", "500 pomodoro seansı tamamladınız.", "fa-brain", "Odaklanma Ustası", 1000, 500, 4 },
                    { 10, "Sosyal", "#cd7f32", "İlk takip işleminizi gerçekleştirdiniz.", "fa-user-plus", "Merhaba Dünya", 10, 1, 1 },
                    { 11, "Sosyal", "#c0c0c0", "10 kullanıcıyı takip ettiniz.", "fa-users", "Sosyalleşen", 40, 10, 2 },
                    { 12, "Sosyal", "#ffd700", "50 kullanıcıyı takip ettiniz.", "fa-people-group", "Çevresi Geniş", 100, 50, 3 },
                    { 13, "Sosyal", "#e5e4e2", "100 takipçiye ulaştınız.", "fa-star", "Fenomen", 500, 100, 4 },
                    { 14, "Etkileşim", "#cd7f32", "İlk kitap yorumunuzu yaptınız.", "fa-comment", "İlk Bakış", 15, 1, 1 },
                    { 15, "Etkileşim", "#c0c0c0", "10 kitap yorumu yaptınız.", "fa-comments", "Eleştirmen", 75, 10, 2 },
                    { 16, "Etkileşim", "#ffd700", "50 kitap yorumu yaptınız.", "fa-pen-fancy", "Uzman Yazar", 200, 50, 3 },
                    { 17, "Etkileşim", "#e5e4e2", "100 kitap yorumu yaptınız.", "fa-magnifying-glass-chart", "Edebiyat Dedektifi", 500, 100, 4 },
                    { 18, "Bilgelik", "#cd7f32", "100 Bilgelik taşı topladınız.", "fa-gem", "Çaylak", 0, 100, 1 },
                    { 19, "Bilgelik", "#c0c0c0", "1000 Bilgelik taşı topladınız.", "fa-medal", "Bilge Kişi", 0, 1000, 2 },
                    { 20, "Bilgelik", "#ffd700", "5000 Bilgelik taşı topladınız.", "fa-crown", "Aydınlanmış", 0, 5000, 3 },
                    { 21, "Bilgelik", "#e5e4e2", "10000 Bilgelik taşı topladınız.", "fa-chess-king", "Bilgeliğin Efendisi", 0, 10000, 4 },
                    { 22, "İstikrar", "#c0c0c0", "7 gün üst üste uygulamayı kullandınız.", "fa-calendar-check", "İstikrarlı", 100, 7, 2 },
                    { 23, "İstikrar", "#ffd700", "30 gün üst üste uygulamayı kullandınız.", "fa-fire", "Demir Disiplin", 500, 30, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "RewardStones",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "Tier",
                table: "Achievements");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ColorHex", "Description" },
                values: new object[] { "#4CAF50", "İlk kitabınızı eklediniz." });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ColorHex", "Description", "Name" },
                values: new object[] { "#2196F3", "10 kitap eklediniz.", "Kitap Kurdu" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ColorHex", "Description", "IconClass", "Name" },
                values: new object[] { "#FF9800", "İlk Odaklanma (Pomodoro) seansınızı tamamladınız.", "fa-stopwatch", "Odaklanma Ustası" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ColorHex", "Description", "IconClass", "Name" },
                values: new object[] { "#9C27B0", "İlk takip işleminizi gerçekleştirdiniz.", "fa-users", "Sosyalleşen" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ColorHex", "Description", "IconClass", "Name" },
                values: new object[] { "#00BCD4", "100 Bilgelik taşı topladınız.", "fa-gem", "Bilge Kişi" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ColorHex", "Description", "IconClass", "Name" },
                values: new object[] { "#F44336", "İlk kitap yorumunuzu yaptınız.", "fa-comment-dots", "Eleştirmen" });
        }
    }
}
