using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTimNguoiThatLac.Migrations
{
    /// <inheritdoc />
    public partial class TheoDoiLan4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HanhViDangNgo_AspNetUsers_ApplicationUserId",
                table: "HanhViDangNgo");

            migrationBuilder.DropIndex(
                name: "IX_HanhViDangNgo_ApplicationUserId",
                table: "HanhViDangNgo");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "HanhViDangNgo");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiDungId",
                table: "HanhViDangNgo",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HanhViDangNgo_NguoiDungId",
                table: "HanhViDangNgo",
                column: "NguoiDungId");

            migrationBuilder.AddForeignKey(
                name: "FK_HanhViDangNgo_AspNetUsers_NguoiDungId",
                table: "HanhViDangNgo",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HanhViDangNgo_AspNetUsers_NguoiDungId",
                table: "HanhViDangNgo");

            migrationBuilder.DropIndex(
                name: "IX_HanhViDangNgo_NguoiDungId",
                table: "HanhViDangNgo");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiDungId",
                table: "HanhViDangNgo",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "HanhViDangNgo",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HanhViDangNgo_ApplicationUserId",
                table: "HanhViDangNgo",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HanhViDangNgo_AspNetUsers_ApplicationUserId",
                table: "HanhViDangNgo",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
