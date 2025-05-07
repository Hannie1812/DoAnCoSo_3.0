using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.ViewModels;

public class ThongBaoMoiViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ThongBaoMoiViewComponent(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new ThongBaoViewModel
        {
            AllNotifications = new List<ThongBaoMoiViewModel>(),
            PostMessages = new List<ThongBaoMoiViewModel>(),
            UserMessages = new List<ThongBaoMoiViewModel>(),
            Violations = new List<ThongBaoMoiViewModel>()
        };

        if (!User.Identity.IsAuthenticated)
        {
            return View("_DanhSachThongBao", model);
        }

        var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);

        if (user == null || user.Id == null)
        {
            return View("_DanhSachThongBao", model);
        }
        var userId = user.Id;
        // 1. Tin nhắn bài viết
        var postComments = await _db.BinhLuans
            .Include(b => b.ApplicationUser)
            .Include(b => b.TimNguoi)
            .Where(b => b.TimNguoi.IdNguoiDung == userId && !b.DaDoc)
            .OrderByDescending(b => b.NgayBinhLuan)
            .Take(10)
            .ToListAsync();

        foreach (var comment in postComments)
        {
            var notification = new ThongBaoMoiViewModel
            {
                TieuDe = comment.ApplicationUser.FullName,
                LinkDuongDan = $"/TimNguoi/ChiTietBaiTimNguoi?id={comment.TimNguoi.Id}&idBinhLuan={comment.Id}",
                NoiDung = comment.NoiDung,
                DanhMuc = "Tin Nhắn Bài Viết",
                HinhAnh = comment.ApplicationUser.HinhAnh,
                ThoiGian = comment.NgayBinhLuan
            };
            model.PostMessages.Add(notification);
            model.AllNotifications.Add(notification);
        }

        // 2. Tin nhắn người dùng
        var conversations = await _db.NguoiThamGias
            .Include(ng => ng.HopThoai)
                .ThenInclude(h => h.TinNhans.OrderByDescending(t => t.NgayGui).Take(1))
            .Include(ng => ng.HopThoai)
                .ThenInclude(h => h.NguoiThamGias)
                    .ThenInclude(tv => tv.ApplicationUser)
            .Where(ng => ng.MaNguoiThamGia == userId &&
                        ng.HopThoai.TinNhans.Any(tn => !tn.IsRead && tn.MaNguoiGui != userId))
            .ToListAsync();

        foreach (var participant in conversations)
        {
            var lastMessage = participant.HopThoai.TinNhans.FirstOrDefault();
            var otherUser = participant.HopThoai.NguoiThamGias
                .FirstOrDefault(tv => tv.MaNguoiThamGia != userId)?
                .ApplicationUser;

            if (otherUser != null && lastMessage != null)
            {
                var notification = new ThongBaoMoiViewModel
                {
                    TieuDe = otherUser.Email,
                    NoiDung = lastMessage.NoiDung,
                    HinhAnh = otherUser.HinhAnh,
                    DanhMuc = "Tin Nhắn Người Dùng",
                    LinkDuongDan = $"/Chat/Index?hopThoaiId={participant.HopThoai.Id}",
                    ThoiGian = lastMessage.NgayGui,

                };
                model.UserMessages.Add(notification);
                model.AllNotifications.Add(notification);
            }
        }

        // 3. Hành vi đáng ngờ
        var violations = await _db.HanhViDangNgos
            .Include(h => h.ApplicationUser)
            .Where(h => h.ApplicationUser.Id == userId && !h.DaXem)
            .OrderByDescending(h => h.ThoiGian)
            .ToListAsync();

        foreach (var violation in violations)
        {
            var notification = new ThongBaoMoiViewModel
            {
                TieuDe = violation.HanhDong,
                NoiDung = violation.ChiTiet,
                HinhAnh = "",
                DanhMuc = "Hành Vi Đáng Ngờ",
                LinkDuongDan = "/LoiViPham/Index",
                ThoiGian = violation.ThoiGian
            };
            model.Violations.Add(notification);
            model.AllNotifications.Add(notification);
        }

        // Sắp xếp tất cả thông báo theo thời gian
        model.AllNotifications = model.AllNotifications
            .OrderByDescending(n => n.ThoiGian)
            .ToList();

        return View("_DanhSachThongBao", model);
    }
}