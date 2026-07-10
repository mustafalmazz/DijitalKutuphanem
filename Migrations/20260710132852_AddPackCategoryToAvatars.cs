using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPackCategoryToAvatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PackCategory",
                table: "ProfileAvatars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 1,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 2,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 3,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 4,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 5,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 6,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 7,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 8,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 9,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 10,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 11,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 12,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 13,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 14,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 15,
                column: "PackCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "ProfileAvatars",
                keyColumn: "Id",
                keyValue: 16,
                column: "PackCategory",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackCategory",
                table: "ProfileAvatars");
        }
    }
}
