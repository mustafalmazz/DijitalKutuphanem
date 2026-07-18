using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileTitleAndTotalStonesEarned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveTitleAchievementId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalStonesEarned",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveTitleAchievementId",
                table: "Users",
                column: "ActiveTitleAchievementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Achievements_ActiveTitleAchievementId",
                table: "Users",
                column: "ActiveTitleAchievementId",
                principalTable: "Achievements",
                principalColumn: "Id");

            // --- MEVCUT KULLANICILAR İÇİN GERİ DOLDURMA ---
            // TotalStonesEarned yeni bir alan; 0 kalırsa mevcut kullanıcılar Bilgelik
            // başarımlarını kaybetmiş görünür. Geçmiş kazanç kaydı tutulmadığı için
            // eldeki en iyi tahmin mevcut bakiyedir (harcananları kapsamaz, yani düşük tahmin).
            migrationBuilder.Sql(@"
                UPDATE Users SET TotalStonesEarned = WisdomStones;
            ");

            // Düşük tahmin, zaten TOPLANMIŞ bir Bilgelik başarımının hedefinin altında
            // kalabilir ve rozet kaybolur. Bunu önlemek için toplanmış başarımların
            // en yüksek hedefine kadar yükseltiyoruz.
            migrationBuilder.Sql(@"
                UPDATE u
                SET u.TotalStonesEarned = x.MaxTarget
                FROM Users u
                INNER JOIN (
                    SELECT ua.UserId, MAX(a.TargetValue) AS MaxTarget
                    FROM UserAchievements ua
                    INNER JOIN Achievements a ON a.Id = ua.AchievementId
                    WHERE a.Category = N'Bilgelik'
                    GROUP BY ua.UserId
                ) x ON x.UserId = u.Id
                WHERE u.TotalStonesEarned < x.MaxTarget;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Achievements_ActiveTitleAchievementId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveTitleAchievementId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveTitleAchievementId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalStonesEarned",
                table: "Users");
        }
    }
}
