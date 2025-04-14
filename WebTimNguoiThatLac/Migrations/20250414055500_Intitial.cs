using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTimNguoiThatLac.Migrations
{
    /// <inheritdoc />
    public partial class Intitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CCCD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GioiThieu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioiThieu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HopThoaiTinNhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDeChat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGroup = table.Column<bool>(type: "bit", nullable: false),
                    LastMessageTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopThoaiTinNhan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungLienHe",
                columns: table => new
                {
                    MaLienHeNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNguoiDungLienHe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailNguoiDungLienHe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VanDeLienHe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiBungLienHe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNguoiDungLienHe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayLienHe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungLienHe", x => x.MaLienHeNguoiDung);
                });

            migrationBuilder.CreateTable(
                name: "TinTuc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaNgan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    TacGia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinTuc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrungBayHinhAnh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrungBayHinhAnh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimNguoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaciemNhanDang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhuVuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoiQuanHe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayMatTich = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdNguoiDung = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DonDangKiTrinhBao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimNguoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimNguoi_AspNetUsers_IdNguoiDung",
                        column: x => x.IdNguoiDung,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NguoiThamGia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopThoaiTinNhan = table.Column<int>(type: "int", nullable: true),
                    MaNguoiThamGia = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NgayThamGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiThamGia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiThamGia_AspNetUsers_MaNguoiThamGia",
                        column: x => x.MaNguoiThamGia,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NguoiThamGia_HopThoaiTinNhan_MaHopThoaiTinNhan",
                        column: x => x.MaHopThoaiTinNhan,
                        principalTable: "HopThoaiTinNhan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TinNhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopThoaiTinNhan = table.Column<int>(type: "int", nullable: true),
                    MaNguoiGui = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinNhan_AspNetUsers_MaNguoiGui",
                        column: x => x.MaNguoiGui,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TinNhan_HopThoaiTinNhan_MaHopThoaiTinNhan",
                        column: x => x.MaHopThoaiTinNhan,
                        principalTable: "HopThoaiTinNhan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnhTimNguoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: true),
                    IdNguoiCanTim = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhTimNguoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnhTimNguoi_TimNguoi_IdNguoiCanTim",
                        column: x => x.IdNguoiCanTim,
                        principalTable: "TimNguoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoBaiViet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBaiViet = table.Column<int>(type: "int", nullable: true),
                    MaNguoiBaoCao = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBaoCao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    check = table.Column<bool>(type: "bit", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoBaiViet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoBaiViet_AspNetUsers_MaNguoiBaoCao",
                        column: x => x.MaNguoiBaoCao,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaoCaoBaiViet_TimNguoi_MaBaiViet",
                        column: x => x.MaBaiViet,
                        principalTable: "TimNguoi",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BinhLuan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBinhLuan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IdBaiViet = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BinhLuan_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BinhLuan_TimNguoi_IdBaiViet",
                        column: x => x.IdBaiViet,
                        principalTable: "TimNguoi",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TimThayNguoiThatLac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimNguoiId = table.Column<int>(type: "int", nullable: true),
                    IdNguoiLamDon = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NgayTimThay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioTimThay = table.Column<TimeSpan>(type: "time", nullable: true),
                    DiaDiemTimThay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiTimThay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhTrangSucKhoe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaDuaVeGiaDinh = table.Column<bool>(type: "bit", nullable: false),
                    HienDangO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTaChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoanTatHoSoPhapLy = table.Column<bool>(type: "bit", nullable: false),
                    AnhMinhChung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiXacNhan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoaiNguoiXacNhan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonXacNhanTimThay = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimThayNguoiThatLac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimThayNguoiThatLac_AspNetUsers_IdNguoiLamDon",
                        column: x => x.IdNguoiLamDon,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimThayNguoiThatLac_TimNguoi_TimNguoiId",
                        column: x => x.TimNguoiId,
                        principalTable: "TimNguoi",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaoCaoBinhLuan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBinhLuan = table.Column<int>(type: "int", nullable: true),
                    MaNguoiBaoCao = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBaoCao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    check = table.Column<bool>(type: "bit", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoBinhLuan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaoBinhLuan_AspNetUsers_MaNguoiBaoCao",
                        column: x => x.MaNguoiBaoCao,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaoCaoBinhLuan_BinhLuan_MaBinhLuan",
                        column: x => x.MaBinhLuan,
                        principalTable: "BinhLuan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhTimNguoi_IdNguoiCanTim",
                table: "AnhTimNguoi",
                column: "IdNguoiCanTim");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoBaiViet_MaBaiViet",
                table: "BaoCaoBaiViet",
                column: "MaBaiViet");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoBaiViet_MaNguoiBaoCao",
                table: "BaoCaoBaiViet",
                column: "MaNguoiBaoCao");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoBinhLuan_MaBinhLuan",
                table: "BaoCaoBinhLuan",
                column: "MaBinhLuan");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoBinhLuan_MaNguoiBaoCao",
                table: "BaoCaoBinhLuan",
                column: "MaNguoiBaoCao");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_IdBaiViet",
                table: "BinhLuan",
                column: "IdBaiViet");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_UserId",
                table: "BinhLuan",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiThamGia_MaHopThoaiTinNhan",
                table: "NguoiThamGia",
                column: "MaHopThoaiTinNhan");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiThamGia_MaNguoiThamGia",
                table: "NguoiThamGia",
                column: "MaNguoiThamGia");

            migrationBuilder.CreateIndex(
                name: "IX_TimNguoi_IdNguoiDung",
                table: "TimNguoi",
                column: "IdNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_TimThayNguoiThatLac_IdNguoiLamDon",
                table: "TimThayNguoiThatLac",
                column: "IdNguoiLamDon");

            migrationBuilder.CreateIndex(
                name: "IX_TimThayNguoiThatLac_TimNguoiId",
                table: "TimThayNguoiThatLac",
                column: "TimNguoiId");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_MaHopThoaiTinNhan",
                table: "TinNhan",
                column: "MaHopThoaiTinNhan");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_MaNguoiGui",
                table: "TinNhan",
                column: "MaNguoiGui");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhTimNguoi");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BaoCaoBaiViet");

            migrationBuilder.DropTable(
                name: "BaoCaoBinhLuan");

            migrationBuilder.DropTable(
                name: "GioiThieu");

            migrationBuilder.DropTable(
                name: "NguoiDungLienHe");

            migrationBuilder.DropTable(
                name: "NguoiThamGia");

            migrationBuilder.DropTable(
                name: "TimThayNguoiThatLac");

            migrationBuilder.DropTable(
                name: "TinNhan");

            migrationBuilder.DropTable(
                name: "TinTuc");

            migrationBuilder.DropTable(
                name: "TrungBayHinhAnh");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BinhLuan");

            migrationBuilder.DropTable(
                name: "HopThoaiTinNhan");

            migrationBuilder.DropTable(
                name: "TimNguoi");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
