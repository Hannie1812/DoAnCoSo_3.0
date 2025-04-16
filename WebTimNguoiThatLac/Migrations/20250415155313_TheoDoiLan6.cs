using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTimNguoiThatLac.Migrations
{
    /// <inheritdoc />
    public partial class TheoDoiLan6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaXem",
                table: "HanhViDangNgo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaXuLy",
                table: "HanhViDangNgo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KhangNghi",
                table: "HanhViDangNgo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaXem",
                table: "HanhViDangNgo");

            migrationBuilder.DropColumn(
                name: "DaXuLy",
                table: "HanhViDangNgo");

            migrationBuilder.DropColumn(
                name: "KhangNghi",
                table: "HanhViDangNgo");
        }
    }
}
