using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTimNguoiThatLac.Migrations
{
    /// <inheritdoc />
    public partial class TheoDoiLan3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichSuTimKiem_AspNetUsers_ApplicationUserId",
                table: "LichSuTimKiem");

            migrationBuilder.DropIndex(
                name: "IX_LichSuTimKiem_ApplicationUserId",
                table: "LichSuTimKiem");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "LichSuTimKiem");

            migrationBuilder.AlterColumn<string>(
                name: "IdNguoiDung",
                table: "LichSuTimKiem",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTimKiem_IdNguoiDung",
                table: "LichSuTimKiem",
                column: "IdNguoiDung");

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuTimKiem_AspNetUsers_IdNguoiDung",
                table: "LichSuTimKiem",
                column: "IdNguoiDung",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichSuTimKiem_AspNetUsers_IdNguoiDung",
                table: "LichSuTimKiem");

            migrationBuilder.DropIndex(
                name: "IX_LichSuTimKiem_IdNguoiDung",
                table: "LichSuTimKiem");

            migrationBuilder.AlterColumn<string>(
                name: "IdNguoiDung",
                table: "LichSuTimKiem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "LichSuTimKiem",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTimKiem_ApplicationUserId",
                table: "LichSuTimKiem",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuTimKiem_AspNetUsers_ApplicationUserId",
                table: "LichSuTimKiem",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
