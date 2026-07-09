using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFrameToImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveFrameCssClass",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CssClassName",
                table: "ProfileFrames");

            migrationBuilder.AddColumn<string>(
                name: "ActiveFrameImageUrl",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProfileFrames",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "/images/frames/emerald.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "/images/frames/sunset.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "/images/frames/ice.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImageUrl",
                value: "/images/frames/ancient.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImageUrl",
                value: "/images/frames/royal.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImageUrl",
                value: "/images/frames/neon.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImageUrl",
                value: "/images/frames/gold.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImageUrl",
                value: "/images/frames/fire.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImageUrl",
                value: "/images/frames/shadow.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImageUrl",
                value: "/images/frames/diamond.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 11,
                column: "ImageUrl",
                value: "/images/frames/galaxy.png");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 12,
                column: "ImageUrl",
                value: "/images/frames/rainbow.png");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveFrameImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProfileFrames");

            migrationBuilder.AddColumn<string>(
                name: "ActiveFrameCssClass",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CssClassName",
                table: "ProfileFrames",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 1,
                column: "CssClassName",
                value: "frame-emerald");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 2,
                column: "CssClassName",
                value: "frame-sunset");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 3,
                column: "CssClassName",
                value: "frame-ice");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 4,
                column: "CssClassName",
                value: "frame-ancient");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 5,
                column: "CssClassName",
                value: "frame-royal");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 6,
                column: "CssClassName",
                value: "frame-neon");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 7,
                column: "CssClassName",
                value: "frame-gold");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 8,
                column: "CssClassName",
                value: "frame-fire");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 9,
                column: "CssClassName",
                value: "frame-shadow");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 10,
                column: "CssClassName",
                value: "frame-diamond");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 11,
                column: "CssClassName",
                value: "frame-galaxy");

            migrationBuilder.UpdateData(
                table: "ProfileFrames",
                keyColumn: "Id",
                keyValue: 12,
                column: "CssClassName",
                value: "frame-rainbow");
        }
    }
}
