using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileBanners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveBannerId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProfileBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceInStones = table.Column<int>(type: "int", nullable: false),
                    RequiredBookCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileBanners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProfileBannerId = table.Column<int>(type: "int", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBanners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBanners_ProfileBanners_ProfileBannerId",
                        column: x => x.ProfileBannerId,
                        principalTable: "ProfileBanners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBanners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveBannerId",
                table: "Users",
                column: "ActiveBannerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBanners_ProfileBannerId",
                table: "UserBanners",
                column: "ProfileBannerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBanners_UserId",
                table: "UserBanners",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ProfileBanners_ActiveBannerId",
                table: "Users",
                column: "ActiveBannerId",
                principalTable: "ProfileBanners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ProfileBanners_ActiveBannerId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserBanners");

            migrationBuilder.DropTable(
                name: "ProfileBanners");

            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveBannerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveBannerId",
                table: "Users");
        }
    }
}
