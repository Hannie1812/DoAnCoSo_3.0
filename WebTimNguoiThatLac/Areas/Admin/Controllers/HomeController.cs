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

            var conversations = await _context.NguoiThamGias
               .Include(ng => ng.HopThoai)
                   .ThenInclude(h => h.TinNhans.OrderByDescending(t => t.NgayGui).Take(1))
               .Include(ng => ng.HopThoai)
                   .ThenInclude(h => h.NguoiThamGias)
                        .ThenInclude(tv => tv.ApplicationUser)
               .Where(ng => ng.MaNguoiThamGia == user.Id && ng.HopThoai.TinNhans.Any(tn => tn.IsRead == false && tn.MaNguoiGui != user.Id))
               .ToListAsync();

            ViewBag.DSTinNhanMoi = conversations.Select(ng => ng.HopThoai).ToList() ?? new List<HopThoaiTinNhan>();

            ViewBag.DemBaoCaoBinhLuan = await _context.BaoCaoBinhLuans
                .Include(b => b.BinhLuan)
                .Where(b => b.DaDoc == false)
                .CountAsync();


            ViewBag.DemBaoCaoBaiViet = await _context.BaoCaoBaiViets
                .Include(b => b.TimNguoi)
                .Where(b => b.DaDoc == false)
                .CountAsync();


            ViewBag.DemLienHeNguoiDung = await _context.NguoiDungLienHes
                .Where(b => b.isRead == false).CountAsync();





            // Lấy dữ liệu từ database
            // Lấy dữ liệu từ database với include TinhThanh
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
            int soNgay = tuan.HasValue ? tuan.Value * 7 : 7;
            var ngayLabels = Enumerable.Range(0, soNgay)
                .Select(i => now.AddDays(-i).Date)
                .OrderBy(d => d)
                .Select(d => d.ToString("dd/MM"))
                .ToList();

            var baiVietTheoNgay = Enumerable.Range(0, soNgay)
                .Select(i => now.AddDays(-i).Date)
                .OrderBy(d => d)
                .Select(ngay => baiViets.Count(x => x.NgayDang.Date == ngay))
                .ToList();

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