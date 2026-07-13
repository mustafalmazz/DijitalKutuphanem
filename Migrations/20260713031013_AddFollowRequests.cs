using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Follows",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Follows");
        }
    }
}
