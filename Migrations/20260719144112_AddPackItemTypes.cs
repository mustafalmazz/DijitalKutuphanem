using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPackItemTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "StorePackages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackCategory",
                table: "ProfileFrames",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackCategory",
                table: "ProfileBanners",
                type: "nvarchar(max)",
                nullable: true);

            // Mevcut paketlerin hepsi avatar dagitiyordu; bos string yerine dogru tur yazilir.
            // Bu olmazsa eski paketler hicbir ture uymaz ve icerikleri bulunamaz.
            migrationBuilder.Sql("UPDATE StorePackages SET ItemType = N'Avatar' WHERE ItemType IS NULL OR ItemType = N'';");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 1,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 2,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 3,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 4,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 5,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 6,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 7,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 8,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 9,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 10,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 11,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 12,
                column: "PackCategory",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "StorePackages");

            migrationBuilder.DropColumn(
                name: "PackCategory",
                table: "ProfileFrames");

            migrationBuilder.DropColumn(
                name: "PackCategory",
                table: "ProfileBanners");
        }
    }
}
