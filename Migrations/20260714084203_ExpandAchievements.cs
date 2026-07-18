using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class ExpandAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ColorHex", "Tier" },
                values: new object[] { "#c084fc", 6 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 13,
                column: "Description",
                value: "100 kullanıcıyı takip ettiniz.");

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Category", "ColorHex", "Description", "IconClass", "Name", "RewardStones", "TargetValue", "Tier" },
                values: new object[,]
                {
                    { 24, "Okuma", "#b9f2ff", "250 kitap okudunuz.", "fa-book-bookmark", "Raf Fatihi", 1000, 250, 5 },
                    { 25, "Odaklanma", "#b9f2ff", "1000 pomodoro seansı tamamladınız.", "fa-meteor", "Derin Odak", 2000, 1000, 5 },
                    { 26, "Odaklanma", "#c084fc", "2500 pomodoro seansı tamamladınız.", "fa-infinity", "Zamanın Efsanesi", 5000, 2500, 6 },
                    { 27, "Sosyal", "#b9f2ff", "250 kullanıcıyı takip ettiniz.", "fa-handshake", "Topluluk Yıldızı", 1000, 250, 5 },
                    { 28, "Sosyal", "#c084fc", "500 kullanıcıyı takip ettiniz.", "fa-heart", "Kitap Kulübü Efsanesi", 2500, 500, 6 },
                    { 29, "Etkileşim", "#b9f2ff", "250 kitap yorumu yaptınız.", "fa-feather", "Kalem Ustası", 1000, 250, 5 },
                    { 30, "Etkileşim", "#c084fc", "500 kitap yorumu yaptınız.", "fa-scroll", "Edebiyatın Sesi", 2500, 500, 6 },
                    { 31, "Bilgelik", "#b9f2ff", "25000 Bilgelik taşı topladınız.", "fa-coins", "Hazine Avcısı", 0, 25000, 5 },
                    { 32, "Bilgelik", "#c084fc", "50000 Bilgelik taşı topladınız.", "fa-hat-wizard", "Ebedi Bilge", 0, 50000, 6 },
                    { 33, "İstikrar", "#cd7f32", "3 gün üst üste uygulamayı kullandınız.", "fa-bolt", "İlk Kıvılcım", 25, 3, 1 },
                    { 34, "İstikrar", "#e5e4e2", "100 gün üst üste uygulamayı kullandınız.", "fa-shield-halved", "Sarsılmaz", 1500, 100, 4 },
                    { 35, "İstikrar", "#b9f2ff", "180 gün üst üste uygulamayı kullandınız.", "fa-mountain", "Adanmış Okur", 3000, 180, 5 },
                    { 36, "İstikrar", "#c084fc", "365 gün üst üste uygulamayı kullandınız.", "fa-trophy", "Bir Yılın Hikayesi", 10000, 365, 6 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ColorHex", "Tier" },
                values: new object[] { "#b9f2ff", 5 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 13,
                column: "Description",
                value: "100 takipçiye ulaştınız.");
        }
    }
}
