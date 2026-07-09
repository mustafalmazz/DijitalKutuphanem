using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveFrameCssClass",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProfileFrames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriceInStones = table.Column<int>(type: "int", nullable: false),
                    RequiredBookCount = table.Column<int>(type: "int", nullable: false),
                    CssClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconEmoji = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileFrames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFrames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProfileFrameId = table.Column<int>(type: "int", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFrames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFrames_ProfileFrames_ProfileFrameId",
                        column: x => x.ProfileFrameId,
                        principalTable: "ProfileFrames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFrames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProfileFrames",
                columns: new[] { "Id", "CssClassName", "Description", "IconEmoji", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[,]
                {
                    { 1, "frame-gold", "Klasik ve gösterişli altın parıltı. Profilini asil bir dokunuşla süsle.", "👑", "Altın Çerçeve", 500, 0 },
                    { 2, "frame-neon", "Siber dünyanın enerjisini hisset. Parlayan neon mavisi ile dikkat çek.", "💠", "Neon Işık", 800, 0 },
                    { 3, "frame-fire", "Tutkulu okuyucuların simgesi. Profil fotoğrafını alevlerle sar.", "🔥", "Ateş Halkası", 1200, 0 },
                    { 4, "frame-diamond", "Sadece gerçek bilgelere özel. Bu çerçeveyi hak etmen gerekiyor!", "💎", "Elmas Taç", 2000, 50 },
                    { 5, "frame-galaxy", "Evreni keşfedenlerin profil çerçevesi. Yıldızlarla dans et.", "🌌", "Galaksi", 1500, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFrames_ProfileFrameId",
                table: "UserFrames",
                column: "ProfileFrameId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFrames_UserId",
                table: "UserFrames",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFrames");

            migrationBuilder.DropTable(
                name: "ProfileFrames");

            migrationBuilder.DropColumn(
                name: "ActiveFrameCssClass",
                table: "Users");
        }
    }
}
