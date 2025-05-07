using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Services;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Controllers
{
    public class TinTucController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TimNguoiController> _logger;
        private readonly EmailService _emailService;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public TinTucController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger<TimNguoiController> logger, EmailService _emailService)
        {

            this.db = db;
            _userManager = userManager;
            _logger = logger;
            this._emailService = _emailService;
        }
        public async Task<IActionResult> Index(string TimKiem = "", int Page = 1)
        {

            IEnumerable<TinTuc> ds = await db.TinTucs.ToListAsync();
            ViewBag.TimKiem = TimKiem;
            int sodongtren1trang = 5;

            if (TimKiem.IsNullOrEmpty())
            {
                var dsTrang = ds.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
            else
            {
                TimKiem = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(TimKiem);
                List<TinTuc> dsTimKiem = new List<TinTuc>();
                foreach (TinTuc i in ds)
                {
                    string r1 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.TieuDe);
                    if (r1.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    string r2 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.TacGia);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }

                }

                // Lưu lịch sử tìm kiếm
                string nguoiDungId = null;

                var diaChiIP = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (User.Identity.IsAuthenticated)
                {
                    var nguoiDung = await _userManager.GetUserAsync(User);
                    nguoiDungId = nguoiDung.Id;

                    if (nguoiDung.Active == false)
                    {

                        // Ghi log
                        _logger.LogWarning($"Tài khoản {nguoiDung.Email} đã bị vô hiệu hóa do vi phạm quy định.");
                        TempData["Warning"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                        return Redirect("/Identity/Account/Login");
                    }

                    // Ghi lịch sử tìm kiếm
                    LichSuTimKiem lichSu = new LichSuTimKiem
                    {
                        IdNguoiDung = nguoiDungId,
                        TuKhoa = TimKiem,
                        ThoiGianTimKiem = DateTime.UtcNow,
                        DiaChiIP = diaChiIP
                    };
                    db.LichSuTimKiems.Add(lichSu);
                    await db.SaveChangesAsync();

                    // Kiểm tra hành vi đáng ngờ
                    var soLanTimTrong1Phut = db.LichSuTimKiems
                        .Where(x => x.IdNguoiDung == nguoiDungId && x.ThoiGianTimKiem > DateTime.UtcNow.AddMinutes(-1))
                        .Count();

                    if (soLanTimTrong1Phut > 10)
                    {
                        var hanhVi = new HanhViDangNgo
                        {
                            NguoiDungId = nguoiDungId,
                            HanhDong = "Tìm kiếm quá nhiều",
                            ThoiGian = DateTime.UtcNow,
                            ChiTiet = $"Đã tìm kiếm {soLanTimTrong1Phut} lần trong vòng 1 phút, Nghi ngờ bạn đang có ý định xâm hại hệ thống"
                        };
                        db.HanhViDangNgos.Add(hanhVi);
                        await db.SaveChangesAsync();

                        // 👉 Tăng số lần vi phạm của người dùng
                        ApplicationUser nguoiDungViPham = await db.Users.FirstOrDefaultAsync(u => u.Id == nguoiDungId);
                        if (nguoiDungViPham != null)
                        {
                            nguoiDungViPham.SoLanViPham++;
                            await db.SaveChangesAsync();

                            if (nguoiDungViPham.SoLanViPham >= 5)
                            {
                                nguoiDungViPham.Active = false;
                                await db.SaveChangesAsync();

                                // 👉 Gửi email thông báo
                                await _emailService.SendEmailAsync(nguoiDungViPham.Email, "Tài khoản của bạn đã bị vô hiệu hóa", "Tài khoản của bạn đã bị vô hiệu hóa do vi phạm quy định của hệ thống. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.");

                                // 👉 Ghi log
                                _logger.LogWarning($"Tài khoản {nguoiDungViPham.Email} đã bị vô hiệu hóa do vi phạm quy định.");

                                TempData["WarningMessage"] = "Bạn đang bị nghi ngờ phá hoại hệ thống. Cần Đăng Nhập Lại";
                                return Redirect("/Identity/Account/Login");

                            }
                            else
                            {
                                ViewData["Warning"] = "Bạn đang bị nghi ngờ phá hoại hệ thống. Cần Đăng Nhập Lại";
                            }

                        }
                        TempData["WarningMessage"] = "Bạn đang bị nghi ngờ phá hoại hệ thống. Cần Đăng Nhập Lại";

                        // 👉 đăng nhập lại
                        
                        // logout 
                        await _signInManager.SignOutAsync();
                        return Redirect("/Identity/Account/Login");


                    }
                }

                var dsTrang = dsTimKiem.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
        }

        public async Task<IActionResult> Details(int id)
        {

            TinTuc y = db.TinTucs.FirstOrDefault(i => i.Id == id);
            if (y == null)
            {
                return RedirectToAction("Index");
            }
            List<TinTuc> dsMoi = await db.TinTucs.OrderByDescending(i => i.NgayDang).Take(10).ToListAsync();
            ViewBag.DSTinTucMoi = dsMoi;
            return View(y);

        }
    }
}
