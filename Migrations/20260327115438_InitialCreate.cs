using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    ISBN = table.Column<string>(type: "TEXT", nullable: false),
                    CopiesAvailable = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BorrowRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    BorrowedBy = table.Column<string>(type: "TEXT", nullable: false),
                    BorrowedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "CopiesAvailable", "ISBN", "Title" },
                values: new object[,]
                {
                    { 1, "Chimamanda Ngozi Adichie", 5, "9781616953638", "Purple Hibiscus" },
                    { 2, "Chimamanda Ngozi Adichie", 4, "9781400095209", "Half of a Yellow Sun" },
                    { 3, "Chimamanda Ngozi Adichie", 6, "9780307455925", "Americanah" },
                    { 4, "Jordan Peterson", 3, "9780345816023", "12 Rules for Life" },
                    { 5, "Jordan Peterson", 2, "9780593084649", "Beyond Order" },
                    { 6, "Chinua Achebe", 7, "9780385474542", "Things Fall Apart" },
                    { 7, "Ben Okri", 3, "9780385425148", "The Famished Road" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "$2a$12$KIXzH3mCLtpS0uOFqJMiCOU3uXavhU0sO2/3X4l0U8kWjT/R4N6Ge", "Admin", "admin" },
                    { 2, "$2a$12$oVa7hhFbnWFTlVXtWt.hFOLiO.HFLSqk7JFTQJRx/0GNxe2M9jvGS", "User", "libraryuser" }
                });

            migrationBuilder.InsertData(
                table: "BorrowRecords",
                columns: new[] { "Id", "BookId", "BorrowedAt", "BorrowedBy", "ReturnedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "alice", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 1, new DateTime(2024, 2, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "bob", new DateTime(2024, 2, 13, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 1, new DateTime(2024, 3, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "carol", new DateTime(2024, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 1, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "dave", new DateTime(2024, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, 1, new DateTime(2024, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "eve", null },
                    { 6, 3, new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "frank", new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, 3, new DateTime(2024, 2, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "grace", new DateTime(2024, 2, 29, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, 3, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "hank", new DateTime(2024, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, 3, new DateTime(2024, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "ivan", null },
                    { 10, 5, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "judy", new DateTime(2024, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, 5, new DateTime(2024, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "kyle", new DateTime(2024, 3, 13, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, 5, new DateTime(2024, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "lisa", null },
                    { 13, 2, new DateTime(2024, 2, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "mike", new DateTime(2024, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 14, 2, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "nina", null },
                    { 15, 4, new DateTime(2024, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "omar", new DateTime(2024, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_BookId",
                table: "BorrowRecords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowRecords");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
