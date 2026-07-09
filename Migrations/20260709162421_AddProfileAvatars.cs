using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAvatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileAvatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileAvatars", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ProfileAvatars",
                columns: new[] { "Id", "ImageUrl", "Name" },
                values: new object[,]
                {
                    { 1, "https://cdn-icons-png.flaticon.com/512/3906/3906607.png", "Avatar 1" },
                    { 2, "https://cdn-icons-png.flaticon.com/512/4042/4042356.png", "Avatar 2" },
                    { 3, "https://cdn-icons-png.flaticon.com/512/523/523461.png", "Avatar 3" },
                    { 4, "https://cdn-icons-png.flaticon.com/512/4086/4086679.png", "Avatar 4" },
                    { 5, "https://cdn-icons-png.flaticon.com/512/4042/4042422.png", "Avatar 5" },
                    { 6, "https://cdn-icons-png.flaticon.com/512/4086/4086577.png", "Avatar 6" },
                    { 7, "https://cdn-icons-png.flaticon.com/512/189/189162.png", "Avatar 7" },
                    { 8, "https://cdn-icons-png.flaticon.com/512/4322/4322991.png", "Avatar 8" },
                    { 9, "https://cdn-icons-png.flaticon.com/512/4140/4140047.png", "Avatar 9" },
                    { 10, "https://cdn-icons-png.flaticon.com/512/8854/8854242.png", "Avatar 10" },
                    { 11, "https://cdn-icons-png.flaticon.com/512/17715/17715285.png", "Avatar 11" },
                    { 12, "https://cdn-icons-png.flaticon.com/512/8231/8231329.png", "Avatar 12" },
                    { 13, "https://cdn-icons-png.flaticon.com/512/1326/1326377.png", "Avatar 13" },
                    { 14, "https://cdn-icons-png.flaticon.com/512/9308/9308979.png", "Avatar 14" },
                    { 15, "https://cdn-icons-png.flaticon.com/512/780/780258.png", "Avatar 15" },
                    { 16, "https://cdn-icons-png.flaticon.com/512/624/624150.png", "Avatar 16" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileAvatars");
        }
    }
}
