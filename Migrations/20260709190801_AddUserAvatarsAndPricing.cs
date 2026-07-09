using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatarsAndPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProfileAvatars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceInStones",
                table: "ProfileAvatars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredBookCount",
                table: "ProfileAvatars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserAvatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProfileAvatarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAvatars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAvatars_ProfileAvatars_ProfileAvatarId",
                        column: x => x.ProfileAvatarId,
                        principalTable: "ProfileAvatars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAvatars_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Klasik Prens", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Klasik Prenses", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Sevimli Ninja", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Bilge Baykuş", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Kitap Kurdu 1", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Gözlüklü Bilgin", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Şapkalı Büyücü", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Gizemli Okur", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Genç Öğrenci", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Mutlu Çocuk", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Kedi Dostu", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Sevimli Köpek", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Uzaylı Okuyucu", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Robot Kitapkurdu", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Canavar Okur", 0, 0 });

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Description", "Name", "PriceInStones", "RequiredBookCount" },
                values: new object[] { "Başlangıç Avatarı", "Sevimli Hayalet", 0, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_UserAvatars_ProfileAvatarId",
                table: "UserAvatars",
                column: "ProfileAvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAvatars_UserId",
                table: "UserAvatars",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAvatars");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ProfileAvatars");

            migrationBuilder.DropColumn(
                name: "PriceInStones",
                table: "ProfileAvatars");

            migrationBuilder.DropColumn(
                name: "RequiredBookCount",
                table: "ProfileAvatars");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Avatar 1");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Avatar 2");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Avatar 3");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Avatar 4");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Avatar 5");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Avatar 6");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Avatar 7");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Avatar 8");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Avatar 9");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Avatar 10");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Avatar 11");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Avatar 12");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "Avatar 13");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Avatar 14");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Avatar 15");

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "Avatar 16");
        }
    }
}
