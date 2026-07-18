using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSocialAchievementsToFollowers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "İlk takipçinizi kazandınız.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: "10 takipçiye ulaştınız.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 12,
                column: "Description",
                value: "50 takipçiye ulaştınız.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 13,
                column: "Description",
                value: "100 takipçiye ulaştınız.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 27,
                column: "Description",
                value: "250 takipçiye ulaştınız.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 28,
                column: "Description",
                value: "500 takipçiye ulaştınız.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "İlk takip işleminizi gerçekleştirdiniz.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: "10 kullanıcıyı takip ettiniz.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 12,
                column: "Description",
                value: "50 kullanıcıyı takip ettiniz.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 13,
                column: "Description",
                value: "100 kullanıcıyı takip ettiniz.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 27,
                column: "Description",
                value: "250 kullanıcıyı takip ettiniz.");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 28,
                column: "Description",
                value: "500 kullanıcıyı takip ettiniz.");
        }
    }
}
