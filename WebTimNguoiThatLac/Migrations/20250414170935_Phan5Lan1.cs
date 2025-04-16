using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTimNguoiThatLac.Migrations
{
    /// <inheritdoc />
    public partial class Phan5Lan1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HanhViDangNgo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HanhDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HanhViDangNgo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HanhViDangNgo_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LichSuTraCu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TuKhoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianTimKiem = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuTraCu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuTraCu_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HanhViDangNgo_ApplicationUserId",
                table: "HanhViDangNgo",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTraCu_ApplicationUserId",
                table: "LichSuTraCu",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HanhViDangNgo");

            migrationBuilder.DropTable(
                name: "LichSuTraCu");
        }
    }
}
