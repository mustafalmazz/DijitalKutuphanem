using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class GlobalCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            //  VERİ TAŞIMA - sütun düşürülmeden ÖNCE çalışmalı
            //  Kategoriler global hale geliyor. Kullanıcıya ait kategoriler
            //  silinecek; ancak önce o kategorilerdeki kitaplar korunmalı.
            // ============================================================

            // 1) Yedek kategori "Diğer" yoksa oluştur.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = N'Diğer')
                    INSERT INTO Categories (CategoryName, UserId) VALUES (N'Diğer', NULL);
            ");

            // 2) Kullanıcı kategorisindeki kitapları, AYNI İSİMDE bir global
            //    kategori varsa ona taşı (anlam korunur).
            migrationBuilder.Sql(@"
                UPDATE b
                SET b.CategoryId = g.Id
                FROM Books b
                INNER JOIN Categories c ON c.Id = b.CategoryId AND c.UserId IS NOT NULL
                INNER JOIN (
                    SELECT CategoryName, MIN(Id) AS Id
                    FROM Categories
                    WHERE UserId IS NULL
                    GROUP BY CategoryName
                ) g ON g.CategoryName = c.CategoryName;
            ");

            // 3) Eşleşmeyen kitapların hepsi "Diğer"e düşer. Hiçbir kitap
            //    kategorisiz kalmaz; kullanıcı kitaplarını kaybetmez.
            migrationBuilder.Sql(@"
                UPDATE b
                SET b.CategoryId = (SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'Diğer' AND UserId IS NULL ORDER BY Id)
                FROM Books b
                INNER JOIN Categories c ON c.Id = b.CategoryId
                WHERE c.UserId IS NOT NULL;
            ");

            // 4) Artık hiçbir kitap bağlı değil; kullanıcı kategorilerini sil.
            migrationBuilder.Sql(@"
                DELETE FROM Categories WHERE UserId IS NOT NULL;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId",
                table: "Categories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
