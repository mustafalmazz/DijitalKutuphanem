using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWisdomStones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WisdomStones",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WisdomStones",
                table: "Users");
        }
    }
}
