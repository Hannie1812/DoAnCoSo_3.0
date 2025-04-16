using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTimNguoiThatLac.Migrations
{
    /// <inheritdoc />
    public partial class TheoDoiLan2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichSuTraCu_AspNetUsers_ApplicationUserId",
                table: "LichSuTraCu");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LichSuTraCu",
                table: "LichSuTraCu");

            migrationBuilder.RenameTable(
                name: "LichSuTraCu",
                newName: "LichSuTimKiem");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuTraCu_ApplicationUserId",
                table: "LichSuTimKiem",
                newName: "IX_LichSuTimKiem_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LichSuTimKiem",
                table: "LichSuTimKiem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuTimKiem_AspNetUsers_ApplicationUserId",
                table: "LichSuTimKiem",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichSuTimKiem_AspNetUsers_ApplicationUserId",
                table: "LichSuTimKiem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LichSuTimKiem",
                table: "LichSuTimKiem");

            migrationBuilder.RenameTable(
                name: "LichSuTimKiem",
                newName: "LichSuTraCu");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuTimKiem_ApplicationUserId",
                table: "LichSuTraCu",
                newName: "IX_LichSuTraCu_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LichSuTraCu",
                table: "LichSuTraCu",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuTraCu_AspNetUsers_ApplicationUserId",
                table: "LichSuTraCu",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
