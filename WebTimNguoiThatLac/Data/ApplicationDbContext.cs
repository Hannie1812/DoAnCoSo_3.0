﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<TimNguoi> TimNguois { get; set; }
        public DbSet<AnhTimNguoi> AnhTimNguois { get; set; }
        public DbSet<BinhLuan> BinhLuans { get; set; }
        public DbSet<BaoCaoBinhLuan> BaoCaoBinhLuans { get; set; }
        public DbSet<BaoCaoBaiViet> BaoCaoBaiViets { get; set; }
        public DbSet<HopThoaiTinNhan> HopThoaiTinNhans { get; set; }
        public DbSet<TinNhan> TinNhans { get; set; }
        public DbSet<NguoiThamGia> NguoiThamGias { get; set; }
        public DbSet<GioiThieu> GioiThieus { get; set; }
        public DbSet<TinTuc> TinTucs { get; set; }
        public DbSet<TrungBayHinhAnh> TrungBayHinhAnhs { get; set; }
        public DbSet<TimThayNguoiThatLac> TimThayNguoiThatLacs { get; set; }
        public DbSet<NguoiDungLienHe> NguoiDungLienHes { get; set; }
        public DbSet<LichSuTimKiem> LichSuTimKiems { get; set; }
        public DbSet<HanhViDangNgo> HanhViDangNgos { get; set; }
        public DbSet<NhanChung> NhanChungs { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }

        // ghi log  -> thêm bảng ghi log
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        public DbSet<TinhThanh> TinhThanhs { get; set; }
        public DbSet<QuanHuyen> QuanHuyens { get; set; }

        //public DbSet<FaceDescriptor> FaceDescriptors { get; set; } // Bảng lưu trữ FaceDescriptor

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tạo index duy nhất cho PhoneNumber
            builder.Entity<ApplicationUser>()
                   .HasIndex(u => u.PhoneNumber)
                   .IsUnique();
        }

    }
}
