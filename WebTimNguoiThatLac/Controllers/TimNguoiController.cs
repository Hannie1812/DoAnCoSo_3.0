using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;
using WebTimNguoiThatLac.BoTro;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Services;
using X.PagedList.Extensions;
using WebTimNguoiThatLac.ViewModels;

namespace WebTimNguoiThatLac.Controllers
{
    public class TimNguoiController : Controller
    {
        private ApplicationDbContext db;
        
        private UserManager<ApplicationUser> _userManager;

        private readonly EmailService _emailService;
        private readonly OtpService _otpService;
        private readonly ILogger<TimNguoiController> _logger;
        private const int ReportThreshold = 3;// vi pham

        /*private static readonly IEnumerable<string> TinhThanhIEnumerable = new List<string>
        {
            "Hà Nội",
            "Hồ Chí Minh",
            "Đà Nẵng",
            "Hải Phòng",
            "Cần Thơ",
            "An Giang",
            "Bà Rịa - Vũng Tàu",
            "Bắc Giang",
            "Bắc Kạn",
            "Bạc Liêu",
            "Bắc Ninh",
            "Bến Tre",
            "Bình Định",
            "Bình Dương",
            "Bình Phước",
            "Bình Thuận",
            "Cà Mau",
            "Cao Bằng",
            "Đắk Lắk",
            "Đắk Nông",
            "Điện Biên",
            "Đồng Nai",
            "Đồng Tháp",
            "Gia Lai",
            "Hà Giang",
            "Hà Nam",
            "Hà Tĩnh",
            "Hải Dương",
            "Hậu Giang",
            "Hòa Bình",
            "Hưng Yên",
            "Khánh Hòa",
            "Kiên Giang",
            "Kon Tum",
            "Lai Châu",
            "Lâm Đồng",
            "Lạng Sơn",
            "Lào Cai",
            "Long An",
            "Nam Định",
            "Nghệ An",
            "Ninh Bình",
            "Ninh Thuận",
            "Phú Thọ",
            "Quảng Bình",
            "Quảng Nam",
            "Quảng Ngãi",
            "Quảng Ninh",
            "Quảng Trị",
            "Sóc Trăng",
            "Sơn La",
            "Tây Ninh",
            "Thái Bình",
            "Thái Nguyên",
            "Thanh Hóa",
            "Thừa Thiên Huế",
            "Tiền Giang",
            "Trà Vinh",
            "Tuyên Quang",
            "Vĩnh Long",
            "Vĩnh Phúc",
            "Yên Bái",
            "Phú Yên"
        };*/

        public TimNguoiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, EmailService emailService, OtpService otpService, ILogger<TimNguoiController> logger)
        {
            this.db = db;
            _userManager = userManager;
            _emailService = emailService;
            _otpService = otpService;
            _logger = logger;
        }

        /*Xác thực mail OTP trước khi đăng bài*/
        [HttpGet]
        public async Task<IActionResult> VerifyOtp()
        {
            string email = string.Empty;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                email = user?.Email;
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Moderator"))
                {
                    // Nếu là admin hoặc moderator thì bỏ qua xác thực OTP, chuyển thẳng đến ThemNguoiCanTim
                    TempData["VerifiedEmail"] = user.Email;
                    return RedirectToAction("ThemNguoiCanTim");
                }
            }

            var model = new OtpVerificationViewModel
            {
                Email = email
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(OtpVerificationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrEmpty(model.OtpCode))
            {
                // Gửi mã OTP mới
                await _otpService.GenerateAndSendOtpAsync(model.Email);
                model.IsVerified = false;
                return View(model);
            }

            // Xác thực mã OTP
            var isValid = await _otpService.VerifyOtpAsync(model.Email, model.OtpCode);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn");
                model.IsVerified = false;
                return View(model);
            }

            // Lưu email đã xác thực vào TempData
            TempData["VerifiedEmail"] = model.Email;
            return RedirectToAction("ThemNguoiCanTim");
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }
            await _otpService.GenerateAndSendOtpAsync(email);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            await _otpService.GenerateAndSendOtpAsync(email);
            return Ok();
        }

        public async Task<IActionResult> Index(string ten, int? tinhThanhId, int? quanHuyenId, string dacDiem, int page = 1)
        {
            int pageSize = 6;

            var query = db.TimNguois
                .Include(u => u.ApplicationUser)
                .Include(u => u.AnhTimNguois)
                .Where(i => i.active == true);

            int d = 0;

            // Áp dụng bộ lọc
            if (!string.IsNullOrEmpty(ten))
            {
                query = query.Where(x => x.HoTen.Contains(ten) || x.TieuDe.Contains(ten));
                d++;
            }    

            if (tinhThanhId.HasValue)
            {
                query = query.Where(x => x.IdTinhThanh == tinhThanhId.Value);
                d++;
            }

            if (quanHuyenId.HasValue)
            {
                query = query.Where(x => x.IdQuanHuyen == quanHuyenId.Value);
                d++;
            }    

            if (!string.IsNullOrEmpty(dacDiem))
            {
                query = query.Where(x => x.DaciemNhanDang.Contains(dacDiem));
                d++;
            }

            if (d > 0)
            {

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
                    // Trước khi tạo lịch sử tìm kiếm
                    string tenTinhThanh = "";
                    string tenQuanHuyen = "";

                    if (tinhThanhId.HasValue)
                    {
                        var tinh = await db.TinhThanhs.FindAsync(tinhThanhId.Value);
                        if (tinh != null)
                            tenTinhThanh = tinh.TenTinhThanh;
                    }

                    if (quanHuyenId.HasValue)
                    {
                        var quan = await db.QuanHuyens.FindAsync(quanHuyenId.Value);
                        if (quan != null)
                            tenQuanHuyen = quan.TenQuanHuyen;
                    }

                    string khuVuc = $"{tenQuanHuyen} {tenTinhThanh}".Trim();

                    // Ghi lịch sử tìm kiếm
                    LichSuTimKiem lichSu = new LichSuTimKiem
                    {
                        IdNguoiDung = nguoiDungId,
                        TuKhoa = $"{ten} {khuVuc} {dacDiem}".Trim(),
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


                                //return Redirect("/Identity/Account/Login");
                                TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                                return RedirectToAction("Index", "LoiViPham", new { area = "" });

                            }
                            else
                            {
                                ViewData["Warning"] = "Bạn đang bị nghi ngờ phá hoại hệ thống. Cần Đăng Nhập Lại";
                            }

                        }
                        // 👉 đăng nhập lại
                        //return Redirect("/Identity/Account/Login");


                    }
                }

            }


            // Fetch data for dropdowns
            ViewBag.TinhThanhList = await db.TinhThanhs.ToListAsync();
            ViewBag.QuanHuyenList = await db.QuanHuyens.ToListAsync();

            ViewBag.SelectedTinhThanh = tinhThanhId;
            ViewBag.SelectedQuanHuyen = quanHuyenId;
            ViewBag.TenFilter = ten;
            ViewBag.DacDiemFilter = dacDiem;

            // Sử dụng ToPagedList thay vì ToPagedListAsync
            var pagedList = query.OrderByDescending(x => x.Id)
                                .ToPagedList(page, pageSize);
            return View(pagedList);
        }
        public async Task<IActionResult> ThemNguoiCanTim()
        {
            // Kiểm tra xem email đã được xác thực chưa
            var verifiedEmail = TempData["VerifiedEmail"] as string;
            if (string.IsNullOrEmpty(verifiedEmail))
            {
                if (User.Identity.IsAuthenticated)
                {
                    var nguoiDung = await _userManager.GetUserAsync(User);
                    if (nguoiDung != null && await _userManager.IsInRoleAsync(nguoiDung, "Admin"))
                    {
                        // Nếu là admin thì cho phép vào luôn
                        verifiedEmail = nguoiDung.Email;
                        TempData["VerifiedEmail"] = verifiedEmail;
                    }
                    else
                    {
                        return RedirectToAction("VerifyOtp");
                    }
                }
                else
                {
                    return RedirectToAction("VerifyOtp");
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                var nguoiDung = await _userManager.GetUserAsync(User);
                if (nguoiDung == null )
                {
                    // Ghi log
                    _logger.LogWarning($"Tài khoản {nguoiDung.Email} đã bị vô hiệu hóa do vi phạm quy định.");
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return Redirect("/Identity/Account/Login");
                }
                if(nguoiDung.Active==false)
                {
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return RedirectToAction("Index", "LoiViPham", new { area = "" });
                }
            }
            else
            {
                TempData["WarningMessage"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
                return Redirect("/Identity/Account/Login");
            }
            ViewBag.DanhSachTinhThanh = await db.TinhThanhs.ToListAsync();
            ViewBag.DanhSachQuanHuyen = await db.QuanHuyens.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemNguoiCanTim(TimNguoi timNguoi, List<IFormFile> DSHinhAnhCapNhat)
        {
            if (User.Identity.IsAuthenticated)
            {
                var nguoiDung = await _userManager.GetUserAsync(User);
                if (nguoiDung == null)
                {
                    // Ghi log
                    _logger.LogWarning($"Tài khoản {nguoiDung.Email} đã bị vô hiệu hóa do vi phạm quy định.");
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return Redirect("/Identity/Account/Login");
                }
                if (nguoiDung.Active == false)
                {
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return RedirectToAction("Index", "LoiViPham", new { area = "" });
                }
            }
            if (ModelState.IsValid)
            {
                if(DSHinhAnhCapNhat == null)
                {
                    ModelState.AddModelError("Lỗi", "Chưa Có Hình Ảnh");
                    ViewBag.DanhSachTinhThanh = await db.TinhThanhs.ToListAsync();
                    ViewBag.DanhSachQuanHuyen = await db.QuanHuyens.ToListAsync();
                    return View(timNguoi);
                }
                timNguoi.active = false;
                db.Add(timNguoi);
                await db.SaveChangesAsync();
                int d = 0;
                if(DSHinhAnhCapNhat.Count == 0)
                {
                    ModelState.AddModelError("Lỗi", "Chưa Có Hình Ảnh");
                    ViewBag.DanhSachTinhThanh = await db.TinhThanhs.ToListAsync();
                    ViewBag.DanhSachQuanHuyen = await db.QuanHuyens.ToListAsync();
                    return View(timNguoi);
                }
                foreach (IFormFile i in DSHinhAnhCapNhat)
                {
                    AnhTimNguoi x = new AnhTimNguoi();
                    x.IdNguoiCanTim = timNguoi.Id;
                    if(d==0)
                    {
                        x.TrangThai = 1;
                        d++;
                    }
                    else
                    {
                        x.TrangThai = 0;
                    }
                    x.HinhAnh = await SaveImage(i, "AnhNguoiCanTim");
                    db.AnhTimNguois.Add(x);
                    await db.SaveChangesAsync();
                }
                //TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";

                return Redirect("/Identity/Account/Manage/Index");
                //return RedirectToAction("Index", "LoiViPham", new { area = "" });

            }
            ViewBag.DanhSachTinhThanh = await db.TinhThanhs.ToListAsync();
            ViewBag.DanhSachQuanHuyen = await db.QuanHuyens.ToListAsync();
            return View(timNguoi);
        }

        public async Task<string> SaveImage(IFormFile ImageURL, string subFolder)
        {
            if (ImageURL == null || ImageURL.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ!");
            }

            // Đường dẫn thư mục lưu ảnh trong wwwroot/uploads/tin-tuc
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file duy nhất để tránh trùng lặp
            string fileExtension = Path.GetExtension(ImageURL.FileName);
            string fileName = Path.GetFileNameWithoutExtension(ImageURL.FileName);
            string uniqueFileName = fileName + "_" + Guid.NewGuid().ToString("N") + fileExtension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file vào thư mục
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ImageURL.CopyToAsync(fileStream);
            }

            // Trả về đường dẫn tương đối để hiển thị ảnh trên web
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }


        public void DeleteImage(string ImageURL, string subFolder)
        {
            if (string.IsNullOrEmpty(ImageURL))
            {
                throw new ArgumentException("Đường dẫn ảnh không hợp lệ!");
            }

            // Lấy đường dẫn tuyệt đối của ảnh trong thư mục wwwroot/uploads/
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            string filePath = Path.Combine(uploadsFolder, Path.GetFileName(ImageURL));

            // Kiểm tra nếu file tồn tại thì xóa
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }


        public async Task<IActionResult> ChiTietBaiTimNguoi(int id, int idBinhLuan = 0)
        {
            if(User.Identity.IsAuthenticated == false)
            {
                TempData["WarningMessage"] = "Bạn cần đăng nhập để thực hiện xem chi tiết để bảo vệ thông tin cá nhân chức năng này.";
                return Redirect("/Identity/Account/login");

            }
            ApplicationUser x = await _userManager.GetUserAsync(User);
            if(x != null)
            {

                if (x.Active == false)
                {
                    // Ghi log
                    _logger.LogWarning($"Tài khoản {x.Email} đã bị vô hiệu hóa do vi phạm quy định.");
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    //return Redirect("/Identity/Account/Login");
                    return RedirectToAction("Index", "LoiViPham", new { area = "" });

                }

                TimNguoi y = db.TimNguois
                    .Include(u => u.AnhTimNguois)
                    .Include(u => u.TimThayNguoiThatLacs)
                    .Include(u => u.QuanHuyen.TinhThanh)
                    .FirstOrDefault(i => i.Id == id);
                ApplicationUser us = await _userManager.FindByIdAsync(y.IdNguoiDung);
                ViewBag.NguoiTim = us;
                ViewBag.DSHinhAnh = await db.AnhTimNguois
                                                        .Where(i => i.IdNguoiCanTim == y.Id)
                                                        .ToListAsync();

                if(y.active == false)
                {
                    TempData["ErrorMessage"] = "Bài Viết Đã Bị Khóa, Nếu Có Thắc Mắc Xin Liên Hệ Với Admin";
                    return RedirectToAction("Index");
                }

                // Pass TinhThanh, QuanHuyen to the view
                ViewBag.TinhThanh = y.QuanHuyen?.TinhThanh?.TenTinhThanh;
                ViewBag.QuanHuyen = y.QuanHuyen?.TenQuanHuyen;
                List<BinhLuan> DSBinhLuan = db.BinhLuans
                                                        .Include(u => u.ApplicationUser)
                                                        .Where(i => i.IdBaiViet ==  id && i.Active == true && i.NguoiDangBaiXoa==false)
                                                        .OrderByDescending(z => z.NgayBinhLuan)
                                                        .ToList();
                if (idBinhLuan != 0)
                {
                    BinhLuan xBinhLuan = db.BinhLuans.FirstOrDefault(i => i.Id == idBinhLuan);
                    if (xBinhLuan != null)
                    {
                        xBinhLuan.DaDoc = true;
                        await db.SaveChangesAsync();
                    }
                }
                ViewBag.DaTimThay = y.TimThayNguoiThatLacs != null;
                ViewBag.DSBinhLuan = DSBinhLuan;
                return View(y);
            }
            return RedirectToAction("ThongTinCaNhan", "NguoiDung");
        }

        [HttpPost]
        public async Task<IActionResult> ThemBinhLuan(int IdBaiViet,string UserId,string NoiDung, IFormFile? HinhAnh)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của người dùng hiện tại
                                                                             // Lấy thông tin bài viết

                var nguoiDung = await _userManager.GetUserAsync(User);
                if (nguoiDung == null || nguoiDung.Active == false)
                {
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return Redirect("/Identity/Account/Login");

                }
                var baiViet = await db.TimNguois
                    .Include(x => x.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.Id == IdBaiViet);

                if(baiViet == null || baiViet.ApplicationUser.Email == null)
                {
                    TempData["Warning"] = "Bài viết không tồn tại hoặc không có thông tin người dùng.";
                    return RedirectToAction("Index", "Home");
                }
                // Xử lý upload hình ảnh
                if (ModelState.IsValid)
                {

                    BinhLuan x = new BinhLuan();
                    if(HinhAnh != null)
                    {
                        x.HinhAnh = await SaveImage(HinhAnh, "BinhLuan");
                    }
                    x.NoiDung = NoiDung;
                    x.UserId = userId;
                    x.IdBaiViet = IdBaiViet;
                    x.DaDoc = false;

                    db.BinhLuans.Add(x);

                   

                    await db.SaveChangesAsync();

                    try
                    {
                        await _emailService.SendEmailAsync(baiViet.ApplicationUser.Email, ThongTinEmail.TieuDeBinhLuanMoi, ThongTinEmail.NoiDungBinhLuanMoi(x.NgayBinhLuan, baiViet.HoTen, x.ApplicationUser?.FullName, x.NoiDung));
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = $"Lỗi khi gửi email: {ex.Message}";
                        return Json(new { success = false, message = "Lỗi: Email đã được gửi Thất Bại! " + ex.Message });
                    }
                    TempData["SuccessMessage"] = "Thêm Bình luận Thành Công";
                    return RedirectToAction("ChiTietBaiTimNguoi", new { id = IdBaiViet });
                }
                TempData["WarningMessage"] = "Có lỗi xảy ra khi thêm bình luận. Vui lòng thử lại.";
                return RedirectToAction("ChiTietBaiTimNguoi", new { id = IdBaiViet }); // Quay lại trang chi tiết

               
            }
            ModelState.AddModelError("Lỗi", "Chưa Đăng Nhập");
            return RedirectToAction("ChiTietBaiTimNguoi", new { id = IdBaiViet }); // Quay lại trang chi tiết
        }

        [HttpGet]
        public async Task<IActionResult> CapNhatBaiViet(int id)
        {
            if(User.Identity.IsAuthenticated)
            {
                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var nguoiDung = await _userManager.GetUserAsync(User);
                if (nguoiDung == null || nguoiDung.Active == false)
                {
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return Redirect("/Identity/Account/Login");
                }
                TimNguoi x = db.TimNguois
                                    .Include(u => u.ApplicationUser)
                                    .Include(u => u.AnhTimNguois)
                                    .Include(u => u.BinhLuans)
                                    .FirstOrDefault(i => i.Id ==  id);
                if(x.IdNguoiDung == userid)
                {
                    ViewBag.DanhSachTinhThanh = await db.TinhThanhs.ToListAsync();
                    ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == id).ToList();
                    return View(x);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }

            ViewBag.DanhSachTinhThanh = await db.TinhThanhs.ToListAsync();
            ViewBag.DanhSachQuanHuyen = await db.QuanHuyens.ToListAsync();
            ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == id).ToList();
            return View();
        }

        [HttpPost]
        
        public async Task<IActionResult> CapNhatBaiViet(TimNguoi x, List<IFormFile>? DSHinhAnhCapNhat)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var nguoiDung = await _userManager.GetUserAsync(User);
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            TimNguoi y = db.TimNguois
                                .Include(u => u.ApplicationUser)
                                .Include(u => u.AnhTimNguois)
                                .Include(u => u.BinhLuans)
                                .Include(u => u.QuanHuyen.TinhThanh)
                                .FirstOrDefault(i => i.Id == x.Id);

            if (y == null || y.IdNguoiDung != userid || nguoiDung.Active == false)
            {
                return RedirectToAction("Login", "Account");
            }

            // Luôn thiết lập ViewBag trước khi trả về View
            ViewBag.DanhSachTinhThanh = await db.TinhThanhs.ToListAsync();
            ViewBag.DanhSachQuanHuyen = await db.QuanHuyens.ToListAsync();
            ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == x.Id).ToList();

            if (!ModelState.IsValid)
            {
                return View(x);
            }

            // Cập nhật thông tin cơ bản (luôn thực hiện dù có ảnh hay không)
            y.MoTa = x.MoTa;
            y.TieuDe = x.TieuDe;
            y.DaciemNhanDang = x.DaciemNhanDang;
            y.GioiTinh = x.GioiTinh;
            y.TrangThai = x.TrangThai;
            y.KhuVuc = x.KhuVuc;
            y.IdTinhThanh = x.IdTinhThanh;
            y.IdQuanHuyen = x.IdQuanHuyen;
            y.NgaySinh = x.NgaySinh;
            y.NgayMatTich = x.NgayMatTich;
            y.HoTen = x.HoTen;
            y.MoiQuanHe = x.MoiQuanHe;

            await db.SaveChangesAsync();

            // Chỉ xử lý ảnh nếu có ảnh mới được chọn
            if (DSHinhAnhCapNhat != null && DSHinhAnhCapNhat.Count > 0)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var dsHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == x.Id).ToList();

                        if (dsHinhAnh != null && dsHinhAnh.Count > 0)
                        {
                            foreach (var i in dsHinhAnh)
                            {
                                DeleteImage(i.HinhAnh, "AnhNguoiCanTim");
                                db.AnhTimNguois.Remove(i);
                            }
                            await db.SaveChangesAsync();
                        }

                        int d = 0;
                        foreach (IFormFile i in DSHinhAnhCapNhat)
                        {
                            var z = new AnhTimNguoi();

                            z.IdNguoiCanTim = y.Id;
                            z.TrangThai = (d == 0) ? 1 : 0;
                            z.HinhAnh = await SaveImage(i, "AnhNguoiCanTim");
                            
                            db.AnhTimNguois.Add(z);
                            await db.SaveChangesAsync();
                            d++;
                        }

                        await db.SaveChangesAsync();
                        await transaction.CommitAsync(); // QUAN TRỌNG: Phải commit transaction


                    }
                    catch
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật hình ảnh");
                        return View(x);
                    }
                }
            }

            // Lưu các thay đổi khác (luôn thực hiện)
            await db.SaveChangesAsync();

            return RedirectToAction("ChiTietBaiTimNguoi", new { id = x.Id });
        }

        [HttpGet]
        public async Task<IActionResult> DemBinhLuanChuaDoc()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { count = 0 });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await db.BinhLuans
                .Include(b => b.TimNguoi)
                .CountAsync(b => b.TimNguoi.IdNguoiDung == userId && !b.DaDoc);

            return Json(new { count });
        }

        // Lấy danh sách bình luận chưa đọc
        [HttpGet]
        public async Task<IActionResult> LayBinhLuanChuaDoc()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return PartialView("_DanhSachThongBao", new List<BinhLuan>());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comments = await db.BinhLuans
                .Include(b => b.ApplicationUser)
                .Include(b => b.TimNguoi)
                .Where(b => b.TimNguoi.IdNguoiDung == userId && !b.DaDoc)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Take(10) // Giới hạn 10 thông báo mới nhất
                .ToListAsync();

            return PartialView("_DanhSachThongBao", comments);
        }

        // Đánh dấu đã đọc
        [HttpPost]
        public async Task<IActionResult> DanhDauDaDoc(int id)
        {
            var binhLuan = await db.BinhLuans.FindAsync(id);
            if (binhLuan != null)
            {
                binhLuan.DaDoc = true;
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BaoCaoBinhLuan(int MaBinhLuan, string LyDo)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện báo cáo" });
            }
            var nguoiDung = await _userManager.GetUserAsync(User);
            if (nguoiDung == null || nguoiDung.Active == false)
            {
                return Json(new { success = false, message = "Tài khoản không hợp lệ" });
            }
            try
            {
                var binhLuan = await db.BinhLuans
                    .Include(b => b.TimNguoi)
                    .FirstOrDefaultAsync(b => b.Id == MaBinhLuan);

                if (binhLuan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận" });
                }

                var currentUser = await _userManager.GetUserAsync(User);

                var baoCao = new BaoCaoBinhLuan
                {
                    MaBinhLuan = MaBinhLuan,
                    MaNguoiBaoCao = currentUser.Id,
                    LyDo = LyDo,
                    NgayBaoCao = DateTime.Now
                };

                db.BaoCaoBinhLuans.Add(baoCao);
                await db.SaveChangesAsync();

                // Gửi thông báo cho admin/quản trị viên
                await _emailService.SendEmailAsync(
                    "dinhcongminh4424@gmail.com",
                    "Báo cáo bình luận mới",
                    $"Bình luận #{MaBinhLuan} trong bài viết '{binhLuan.TimNguoi.HoTen}' đã được báo cáo bởi {currentUser.Email}.\nLý do: {LyDo}"
                );

                await ProcessReportedComments(); // xử lý tự động báo cáo bình luận

                return Json(new { success = true, message = "Báo cáo của bạn đã được gửi thành công" });
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi báo cáo" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Produces("application/json")] // Thêm attribute này
        public async Task<IActionResult> BaoCaoBaiViet(int MaBaiViet, string LyDo,string ChiTiet, IFormFile? HinhAnhBaoCaoBaiViet)
        {
            if (!User.Identity.IsAuthenticated)
            {

                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện báo cáo" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var baiViet = await db.TimNguois.FirstOrDefaultAsync(i => i.Id == MaBaiViet);

                if (currentUser == null || currentUser.Active == false)
                {
                    return Json(new { success = false, message = "Tài khoản không hợp lệ" });
                }

                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết" });
                }

                // Kiểm tra nếu người dùng đã báo cáo bài này trước đó
                var existingReport = await db.BaoCaoBaiViets
                    .AnyAsync(r => r.MaBaiViet == MaBaiViet && r.MaNguoiBaoCao == currentUser.Id);

                if (existingReport)
                {
                    return Conflict(new { success = false, message = "Bạn đã báo cáo bài viết này trước đó" });
                }

                BaoCaoBaiViet x = new BaoCaoBaiViet();
                x.MaBaiViet = MaBaiViet;
                x.LyDo = LyDo;
                x.MaNguoiBaoCao = currentUser.Id;
                x.NgayBaoCao = DateTime.Now;
                x.ChiTiet = ChiTiet;
                if(HinhAnhBaoCaoBaiViet != null && HinhAnhBaoCaoBaiViet.Length > 0)
                {
                    x.HinhAnh = await SaveImage(HinhAnhBaoCaoBaiViet, "AnhMinhTrungBaoCaoBaiViet");
                }


                db.BaoCaoBaiViets.Add(x);
                await db.SaveChangesAsync();

                // Gửi thông báo cho admin/quản trị viên
                await _emailService.SendEmailAsync(
                    "dinhcongminh4424@gmail.com",
                    "Báo cáo bài viết mới",
                    $"Bài viết #{x.MaBaiViet} '{baiViet.HoTen}' đã được báo cáo bởi {currentUser.Email}.\nLý do: {x.LyDo}\nChi tiết: {x.ChiTiet}"
                );

                await ProcessReportedPosts();

                return Json(new { success = true, message = "Báo cáo của bạn đã được gửi thành công" });
            }
            catch (Exception ex)
            {
              
                return Json(new { success = false, message = "Đã xảy ra lỗi khi gửi báo cáo" });
            }
        }

        private async Task ProcessReportedComments() // Xử lý bình luận bị báo cáo
        {
            var problematicComments = await db.BaoCaoBinhLuans
                .Where(b => !b.check)
                .GroupBy(b => b.MaBinhLuan)
                .Where(g => g.Count() >= ReportThreshold)
                .Select(g => new {
                    CommentId = g.Key,
                    ReportCount = g.Count()
                })
                .ToListAsync();

            foreach (var item in problematicComments)
            {
                var comment = await db.BinhLuans
                    .Include(c => c.ApplicationUser)
                    .Include(c => c.TimNguoi)
                    .FirstOrDefaultAsync(c => c.Id == item.CommentId);

                if (comment != null)
                {
                    comment.Active = false;
                    db.Update(comment);

                    var relatedReports = await db.BaoCaoBinhLuans
                        .Where(b => b.MaBinhLuan == item.CommentId)
                        .ToListAsync();

                    foreach (var report in relatedReports)
                    {
                        report.check = true;
                        db.Update(report);
                    }

                    if (comment.ApplicationUser != null)
                    {
                        await _emailService.SendEmailAsync(
                            comment.ApplicationUser.Email,
                            "Bình luận của bạn đã bị ẩn tự động",
                            $"Bình luận của bạn trong bài viết \"{comment.TimNguoi?.TieuDe}\" " +
                            $"đã bị ẩn tự động do nhận được {item.ReportCount} báo cáo vi phạm."
                        );

                        ApplicationUser applicationUser = await db.Users
                            .FirstOrDefaultAsync(u => u.Id == comment.ApplicationUser.Id);
                        if (applicationUser != null)
                        {
                            applicationUser.SoLanViPham += 1;
                            await db.SaveChangesAsync();



                            HanhViDangNgo hanhVi = new HanhViDangNgo
                            {
                                NguoiDungId = applicationUser.Id,
                                HanhDong = "Bình luận bị báo cáo nhiều lần",
                                ThoiGian = DateTime.Now,
                                IdLoiViPham = comment.Id,
                                LoaiViPham = "Bình Luận",

                            };
                            db.HanhViDangNgos.Add(hanhVi);
                            await db.SaveChangesAsync();

                            if (applicationUser.SoLanViPham >= 3)
                            {
                                applicationUser.Active = false;

                                await db.SaveChangesAsync();

                                await _emailService.SendEmailAsync(
                                    applicationUser.Email,
                                    "Tài khoản của bạn đã bị khóa",
                                    "Tài khoản của bạn đã bị khóa do vi phạm quy định nhiều lần. " +
                                    "Vui lòng liên hệ với quản trị viên để biết thêm chi tiết."
                                );

                            }

                            await db.SaveChangesAsync();

                        }
                    }
                }
            }
            await db.SaveChangesAsync();
        }



        private async Task ProcessReportedPosts() // Xử Lý tự động báo cáo bài viết
        {
            // Lấy danh sách bài viết có số báo cáo >= ngưỡng
            var problematicPosts = await db.BaoCaoBaiViets
                .Where(b => !b.check)
                .GroupBy(b => b.TimNguoi.Id)
                .Where(g => g.Count() >= ReportThreshold)
                .Select(g => g.Key)
                .ToListAsync();

            // Vô hiệu hóa các bài viết này
            foreach (var postId in problematicPosts)
            {
                var post = await db.TimNguois.Include(z => z.ApplicationUser).FirstOrDefaultAsync(i => i.Id == postId);
                if (post != null && post.active)
                {
                    post.active = false;
                    db.Update(post);

                    // Gửi email thông báo cho người dùng
                    var user = await db.Users.FirstOrDefaultAsync(m => m.Id == post.ApplicationUser.Id);
                    if (user != null)
                    {
                        await _emailService.SendEmailAsync(
                            user.Email,
                            $"Bài Viết {post.HoTen} đã bị ẩn",
                            $"Bài viết của bạn đã bị ẩn do có nhiều báo cáo. Vui lòng kiểm tra lại nội dung bài viết của bạn."
                        );


                        // Gửi thông báo cho người dùng
                        user.SoLanViPham += 1;
                        if (user.SoLanViPham >= 3)
                        {
                            user.Active = false;

                            await _emailService.SendEmailAsync(
                                user.Email,
                                "Tài Khoản Bị Khóa",
                                "Tài khoản của bạn đã bị khóa do vi phạm nhiều lần. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết."
                            );

                            HanhViDangNgo hanhVi = new HanhViDangNgo
                            {
                                NguoiDungId = user.Id,
                                HanhDong = $"bài viết {post.TieuDe} bị báo cáo nhiều lần",
                                ThoiGian = DateTime.Now,
                                IdLoiViPham = post.Id,
                                LoaiViPham = "Bài Viết",

                            };


                            db.HanhViDangNgos.Add(hanhVi);
                            await db.SaveChangesAsync();
                        }
                    }


                    // Cập nhật số lần vi phạm

                    await db.SaveChangesAsync();
                }
            }

            await db.SaveChangesAsync();
        }
        public async Task<IActionResult> XacNhanTimThay(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                TimNguoi x = db.TimNguois
                                    .Include(u => u.ApplicationUser)
                                    .Include(u => u.AnhTimNguois)
                                    .Include(u => u.BinhLuans)
                                    .FirstOrDefault(i => i.Id == id);
                if (x.IdNguoiDung == userid)
                {
                    ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == id).ToList();
                    ViewBag.TimNguoi = x;
                    return View();
                }
                else
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
                    return RedirectToAction("Login", "Account");
                }
            }

            TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanTimThay(TimThayNguoiThatLac model, IFormFile? HinhAnhCapNhat)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var timNguoi = await db.TimNguois.FindAsync(model.TimNguoiId);

                    if (timNguoi == null)
                        return Json(new { success = false, message = "Không tìm thấy bài viết" });

                    // Tạo mới đối tượng
                    var timThay = new TimThayNguoiThatLac
                    {
                        TimNguoiId = model.TimNguoiId,
                        IdNguoiLamDon = userId,
                        NgayTimThay = model.NgayTimThay,
                        GioTimThay = model.GioTimThay,
                        DiaDiemTimThay = model.DiaDiemTimThay,
                        TinhTrangSucKhoe = model.TinhTrangSucKhoe,
                        DaDuaVeGiaDinh = model.DaDuaVeGiaDinh,
                        HienDangO = model.HienDangO,
                        NguoiTimThay = model.NguoiTimThay,
                        SoDienThoaiNguoiXacNhan = model.SoDienThoaiNguoiXacNhan,
                        MoTaChiTiet = model.MoTaChiTiet
                    };

                    // Xử lý upload ảnh
                    if (HinhAnhCapNhat != null && HinhAnhCapNhat.Length > 0)
                    {
                        timThay.AnhMinhChung = await SaveImage(HinhAnhCapNhat, "AnhMinhChungTimThay");
                    }

                    db.TimThayNguoiThatLacs.Add(timThay);
                    timNguoi.TrangThai = "Đã Tìm Thấy";
                    await db.SaveChangesAsync();

                    // Tạo file Word
                    var wordService = new WordExportService();
                    var user = await db.Users.FindAsync(userId);
                    var fullRecord = await db.TimThayNguoiThatLacs
                        .Include(u => u.ApplicationUser)
                        .Include(u => u.TimNguoi)
                        .FirstOrDefaultAsync(i => i.Id == timThay.Id);

                    byte[] fileContents = wordService.CreateFoundReport(fullRecord, user);
                    var fileName = $"DonXacNhanTimThay_{timNguoi.Id}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                    // Lưu file vĩnh viễn trên server
                    var folderPath = Path.Combine("wwwroot", "XacNhanTimThay");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, fileContents);

                    // Cập nhật đường dẫn file vào database
                    timThay.DonXacNhanTimThay = Path.Combine("XacNhanTimThay", fileName);
                    await db.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        redirectUrl = Url.Action("ChiTietBaiTimNguoi", new { id = model.TimNguoiId }),
                        downloadUrl = Url.Action("DownloadFile", new { id = timThay.Id })
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xác nhận tìm thấy");
                    return Json(new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi xử lý yêu cầu",
                        error = ex.Message
                    });
                }
            }

            // Nếu ModelState không valid
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                errors
            });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var record = await db.TimThayNguoiThatLacs.FindAsync(id);
            if (record == null || string.IsNullOrEmpty(record.DonXacNhanTimThay))
                return NotFound();

            var filePath = Path.Combine("wwwroot", record.DonXacNhanTimThay);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileContents = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileContents,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }


        [HttpPost]
        public async Task<IActionResult> XoaBinhLuan(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện báo cáo" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var binhluan = await db.BinhLuans
                    .Include(b => b.TimNguoi)
                    .Include(b => b.ApplicationUser)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (binhluan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bình luận" });
                }
               

                binhluan.NguoiDangBaiXoa = true;

                await db.SaveChangesAsync();

                // Gửi thông báo cho admin/quản trị viên
                await _emailService.SendEmailAsync(
                     "dinhcongminh4424@gmail.com",
                     "Báo cáo bình luận mới",
                     $"Bình luận #{binhluan.NoiDung} trong bài viết '{binhluan.TimNguoi.HoTen}' đã được xóa bởi {currentUser.Email} người đăng bài"
                 );

                return Json(new { success = true, message = "Bạn đã xóa bình luận thành công" });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa bình luận" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetQuanHuyenByTinhThanh(int tinhThanhId)
        {
            var quanHuyens = await db.QuanHuyens
                .Where(q => q.IdTinhThanh == tinhThanhId)
                .Select(q => new { id = q.Id, tenQuanHuyen = q.TenQuanHuyen })
                .ToListAsync();

            return Json(quanHuyens);
        }


    }

}
