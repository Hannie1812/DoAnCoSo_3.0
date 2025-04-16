using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTimNguoiThatLac.Migrations
{
    /// <inheritdoc />
    public partial class TheoDoiLan5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NguoiDangBaiXoa",
                table: "BinhLuan",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NguoiDangBaiXoa",
                table: "BinhLuan");
        }
    }
}
