using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedProfileFrames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones" },
                values: new object[] { "frame-emerald", "Doğanın dinginliği profilinde.", "🌿", "Zümrüt Bahçesi", 100 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones" },
                values: new object[] { "frame-sunset", "Her sayfa yeni bir ufuk.", "🌅", "Gün Batımı", 120 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "frame-ice", "Soğukkanlı okurların tercihi.", "❄️", "Buz Kristali", 150, 5 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "frame-ancient", "Asırlık kütüphanelerin ruhu.", "📜", "Kadim Bilgelik", 250, 10 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "frame-royal", "Altın işlemeli mor ihtişam.", "💜", "Kraliyet Asaleti", 300, 15 });

            migrationBuilder.InsertData(
                table: "ProfileFrames",
                columns: new[] { "Id", "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[,]
                {
                    { 6, "frame-neon", "Geceleri parlayan dijital aura.", "🔮", "Siber Neon", 350, 20 },
                    { 7, "frame-gold", "Parlayan saf altın ışıltısı.", "🥇", "Kraliyet Altını", 500, 30 },
                    { 8, "frame-fire", "Kıvılcımlar saçan okuma tutkusu.", "🔥", "Cehennem Alevi", 600, 40 },
                    { 9, "frame-shadow", "Karanlığın gizemli gücü.", "🌑", "Gece Lordu", 700, 50 },
                    { 10, "frame-diamond", "Yıldız tozuyla bezeli prestij.", "💎", "Elmas Pırıltısı", 1000, 75 },
                    { 11, "frame-galaxy", "Dönen nebulalar ve yıldız tarlası.", "🌌", "Derin Uzay", 1250, 100 },
                    { 12, "frame-rainbow", "Tüm spektruma hükmedenlerin çerçevesi.", "🌈", "Efsanevi Prizma", 2000, 150 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones" },
                values: new object[] { "frame-gold", "Klasik ve gösterişli altın parıltı. Profilini asil bir dokunuşla süsle.", "👑", "Altın Çerçeve", 500 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones" },
                values: new object[] { "frame-neon", "Siber dünyanın enerjisini hisset. Parlayan neon mavisi ile dikkat çek.", "💠", "Neon Işık", 800 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "frame-fire", "Tutkulu okuyucuların simgesi. Profil fotoğrafını alevlerle sar.", "🔥", "Ateş Halkası", 1200, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "frame-diamond", "Sadece gerçek bilgelere özel. Bu çerçeveyi hak etmen gerekiyor!", "💎", "Elmas Taç", 2000, 50 });

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "frame-galaxy", "Evreni keşfedenlerin profil çerçevesi. Yıldızlarla dans et.", "🌌", "Galaksi", 1500, 0 });
        }
    }
}
