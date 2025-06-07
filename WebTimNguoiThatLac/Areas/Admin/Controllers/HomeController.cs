using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Areas.Admin.Models;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Moderator},{SD.Role_Admin}")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _us;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> us)
        {
            _context = context;
            _roleManager = roleManager;
            _us = us;
        }

        public async Task<IActionResult> Index(int? year, int? month, int? day, int? tinhThanhId, string trangThai, int? tuan)
        {
            ApplicationUser user = _us.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }


            // Khối mới
            // Tính toán các thống kê mới
            var tongBaiViet = await _context.TimNguois.CountAsync();
            var tongBaiVietTruoc = await _context.TimNguois
                .Where(x => x.NgayDang < DateTime.Now.AddMonths(-1))
                .CountAsync();

            var soLuongNam = await _context.TimNguois.CountAsync(x => x.GioiTinh == 1);
            var soLuongNu = await _context.TimNguois.CountAsync(x => x.GioiTinh == 2);

            var trangThaiPhoBien = await _context.TimNguois
                .GroupBy(x => x.TrangThai)
                .Select(g => new { TrangThai = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            // Truyền dữ liệu sang View
            ViewBag.TongBaiViet = tongBaiViet;
            ViewBag.PhanTramTangBaiViet = tongBaiVietTruoc > 0 ?
                Math.Round((double)(tongBaiViet - tongBaiVietTruoc) / tongBaiVietTruoc * 100) : 100;
            ViewBag.TangGiamBaiViet = tongBaiViet > tongBaiVietTruoc ?
                $"Tăng {tongBaiViet - tongBaiVietTruoc}" : $"Giảm {tongBaiVietTruoc - tongBaiViet}";

            ViewBag.SoLuongNam = soLuongNam;
            ViewBag.SoLuongNu = soLuongNu;
            ViewBag.PhanTramNam = tongBaiViet > 0 ? Math.Round((double)soLuongNam / tongBaiViet * 100) : 0;
            ViewBag.PhanTramNu = tongBaiViet > 0 ? Math.Round((double)soLuongNu / tongBaiViet * 100) : 0;

            ViewBag.TrangThaiPhoBien = trangThaiPhoBien?.TrangThai ?? "Không xác định";
            ViewBag.PhanTramTrangThaiPhoBien = 
                                tongBaiViet > 0
                                ? Math.Round((trangThaiPhoBien?.Count ?? 0) / (double)tongBaiViet * 100)
                                : 0;




            // End Khối Mới

            var query = _context.TimNguois
                .Include(x => x.TinhThanh)
                .Include(x => x.QuanHuyen)
                .AsQueryable();

            // Áp dụng các bộ lọc
            if (year.HasValue) query = query.Where(x => x.NgayDang.Year == year.Value);
            if (month.HasValue) query = query.Where(x => x.NgayDang.Month == month.Value);
            if (day.HasValue) query = query.Where(x => x.NgayDang.Day == day.Value);
            if (tinhThanhId.HasValue) query = query.Where(x => x.IdTinhThanh == tinhThanhId);
            if (!string.IsNullOrEmpty(trangThai)) query = query.Where(x => x.TrangThai == trangThai);

            var baiViets = await query.ToListAsync();


           

            // Xử lý dữ liệu thống kê
            var now = DateTime.Now;

            int soNgay = 7; // Mặc định 7 ngày
            if (tuan.HasValue)
            {
                if (tuan.Value == 0) // Lấy toàn bộ thời gian
                {
                    var minDate = await _context.TimNguois.MinAsync(x => x.NgayDang);
                    var maxDate = await _context.TimNguois.MaxAsync(x => x.NgayDang);
                    var totalDays = (maxDate - minDate).Days + 1;
                    soNgay = totalDays > 0 ? totalDays : 1;
                }
                else
                {
                    soNgay = tuan.Value * 7; // Xử lý như cũ
                }
            }

            List<string> ngayLabels;
            if (tuan.HasValue && tuan.Value == 0)
            {
                var minDate = await _context.TimNguois.MinAsync(x => x.NgayDang);
                ngayLabels = Enumerable.Range(0, soNgay)
                    .Select(i => minDate.AddDays(i).Date)
                    .Select(d => d.ToString("dd/MM/yyyy"))
                    .ToList();
            }
            else
            {
                ngayLabels = Enumerable.Range(0, soNgay)
                    .Select(i => now.AddDays(-i).Date)
                    .OrderBy(d => d)
                    .Select(d => d.ToString("dd/MM"))
                    .ToList();
            }

            List<int> baiVietTheoNgay;
            if (tuan.HasValue && tuan.Value == 0)
            {
                var minDate = await _context.TimNguois.MinAsync(x => x.NgayDang);
                baiVietTheoNgay = Enumerable.Range(0, soNgay)
                    .Select(i => minDate.AddDays(i).Date)
                    .Select(ngay => baiViets.Count(x => x.NgayDang.Date == ngay))
                    .ToList();
            }
            else
            {
                baiVietTheoNgay = Enumerable.Range(0, soNgay)
                    .Select(i => now.AddDays(-i).Date)
                    .OrderBy(d => d)
                    .Select(ngay => baiViets.Count(x => x.NgayDang.Date == ngay))
                    .ToList();
            }

            // Thống kê theo Tỉnh Thành (thay cho Khu Vực)
            var tinhThanhStats = await _context.TinhThanhs
                    .Select(t => new
                    {
                        Id = t.Id,
                        TenTinhThanh = t.TenTinhThanh,
                        Count = _context.TimNguois.Count(b => b.IdTinhThanh == t.Id)
                    })
                    .Where(t => t.Count > 0)
                    .OrderByDescending(t => t.Count)
                    .ThenBy(t => t.TenTinhThanh)
                    .ToListAsync();

            var trangThaiStats = baiViets
                .GroupBy(x => x.TrangThai)
                .Select(g => new { TrangThai = g.Key ?? "Không xác định", Count = g.Count() })
                .OrderBy(x => x.TrangThai)
                .ToList();

            var gioiTinhStats = new
            {
                Labels = new List<string> { "Nam", "Nữ", "Khác" },
                Counts = new List<int>
            {
                baiViets.Count(x => x.GioiTinh == 1),
                baiViets.Count(x => x.GioiTinh == 2),
                baiViets.Count(x => x.GioiTinh != 1 && x.GioiTinh != 2)
            }
            };

            // Lấy danh sách các giá trị distinct cho dropdown

            var allTrangThai = await _context.TimNguois
                .Select(x => x.TrangThai)
                .Distinct()
                .ToListAsync();

            // Truyền dữ liệu sang view
            ViewBag.AllTrangThai = allTrangThai;
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.Day = day;
            ViewBag.TinhThanhId = tinhThanhId;
            ViewBag.TrangThai = trangThai;
            ViewBag.Tuan = tuan;
            ViewBag.NgayLabels = ngayLabels;
            ViewBag.BaiVietTheoNgay = baiVietTheoNgay;
            ViewBag.TrangThaiStats = trangThaiStats;
            ViewBag.GioiTinhStats = gioiTinhStats;

            // Xử lý AJAX request
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    tinhThanhs = tinhThanhStats.Select(x => x.TenTinhThanh).ToList(),
                    soLuongsTinhThanh = tinhThanhStats.Select(x => x.Count).ToList(),
                    trangThais = trangThaiStats.Select(x => x.TrangThai).ToList(),
                    soLuongsTrangThai = trangThaiStats.Select(x => x.Count).ToList(),
                    soLuongsGioiTinh = gioiTinhStats.Counts,
                    baiVietTheoNgay = baiVietTheoNgay,
                    ngayLabels = ngayLabels,
                    label = GetTimeLabel(year, month, day, tuan)
                });
            }


            ViewBag.TinhThanhStats = tinhThanhStats;
            //ViewBag.AllTinhThanh = await _context.TinhThanhs.OrderBy(x => x.TenTinhThanh).ToListAsync();
            ViewBag.AllTinhThanh = await _context.TinhThanhs
                                 .Include(u => u.TimNguois)
                                 .Where(t => _context.TimNguois.Any(b => b.IdTinhThanh == t.Id))
                                 .OrderBy(t => t.TenTinhThanh)
                                 .ToListAsync();

            return View(baiViets);
        }

        private string GetTimeLabel(int? year, int? month, int? day, int? tuan)
        {
            if (tuan.HasValue && tuan.Value == 0)
                return "Toàn bộ thời gian";

            if (day.HasValue && month.HasValue && year.HasValue)
                return $"{day.Value:D2}/{month.Value:D2}/{year.Value}";
            if (month.HasValue && year.HasValue)
                return $"Tháng {month.Value:D2}/{year.Value}";
            if (year.HasValue)
                return $"Năm {year.Value}";
            if (tuan.HasValue)
                return $"{tuan.Value} tuần gần nhất";
            return "7 ngày gần nhất";
        }
    }
}