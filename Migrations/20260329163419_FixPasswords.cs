using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$zP8zaB7NuZ.a38xCp5..yu0Ik5irGzIGqqPepVQ2YV6DeTv4NfZLy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$KnITbSDoQR8i2FeCFKjEruu.7fKFpDdgNLyjmaQhSqUoLtASNoZnK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$12$KIXzH3mCLtpS0uOFqJMiCOU3uXavhU0sO2/3X4l0U8kWjT/R4N6Ge");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$12$oVa7hhFbnWFTlVXtWt.hFOLiO.HFLSqk7JFTQJRx/0GNxe2M9jvGS");
        }
    }
}
