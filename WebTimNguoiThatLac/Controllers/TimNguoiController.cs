
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;
using WebTimNguoiThatLac.BoTro;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Services;
using X.PagedList.Extensions;
using WebTimNguoiThatLac.ViewModels;
using WebTimNguoiThatLac.Areas.Admin.Models;

using Newtonsoft.Json;

using WebTimNguoiThatLac.Extensions;
using WebTimNguoiThatLac.ThuMucTimKiem;
using WebTimNguoiThatLac.Helpers;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml.Linq;


namespace WebTimNguoiThatLac.Controllers
{
    public class TimNguoiController : Controller
    {
        private ApplicationDbContext db;

        private UserManager<ApplicationUser> _userManager;
        private object model;
        private readonly EmailService _emailService;
        private readonly OtpService _otpService;
        private readonly ILogger<TimNguoiController> _logger;
        private const int ReportThreshold = 3;// vi pham
        private readonly IWebHostEnvironment _env;

        public TimNguoiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, EmailService emailService, OtpService otpService, ILogger<TimNguoiController> logger, IWebHostEnvironment env)
        {
            this.db = db;
            _userManager = userManager;
            _emailService = emailService;
            _otpService = otpService;
            _logger = logger;
            _env = env;

        }

        [HttpGet]
        public async Task<IActionResult> VerifyOtp()
        {
            string email = string.Empty;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                email = user?.Email;
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin") && await _userManager.IsInRoleAsync(user, "Moderator"))
                {
                    // Nếu là admin thì bỏ qua xác thực OTP, chuyển thẳng đến ThemNguoiCanTim
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
                TempData["SuccessMessage"] = "Đã Gửi Mã OTP Thành Công Vui Lòng Kiểm Tra Email Của Bạn";
                return View(model);
            }

            // Xác thực mã OTP
            var isValid = await _otpService.VerifyOtpAsync(model.Email, model.OtpCode);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn");
                model.IsVerified = false;
                TempData["ErrorMessage"] = "Mã OTP không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.";
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


        [HttpGet]
        public async Task<IActionResult> GetQuanHuyenByTinhThanh(int tinhThanhId)
        {
            var quanHuyenList = await db.QuanHuyens
                .Where(q => q.IdTinhThanh == tinhThanhId)
                .OrderBy(q => q.TenQuanHuyen)
                .Select(q => new {
                    id = q.Id,
                    tenQuanHuyen = q.TenQuanHuyen
                })
                .ToListAsync();


            return Json(quanHuyenList);
        }



        public async Task<IActionResult> Index(string ten, int? tinhThanhId, int? quanHuyenId, string khuVuc, string dacDiem, int page = 1)
        {
            int pageSize = 6; // Số bài viết mỗi trang

            var query = db.TimNguois
                .Include(u => u.ApplicationUser)
                .Include(u => u.AnhTimNguois)
                .Include(u => u.TinhThanh)
                .Include(u => u.QuanHuyen)
                .Where(i => i.active == true && i.TrangThai != "Đã Tìm Thấy");

            int d = 0;
            // Áp dụng bộ lọc tên
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

            // Áp dụng bộ lọc đặc điểm nhận dạng (nâng cấp)
            if (!string.IsNullOrEmpty(dacDiem))
            {
                // Tách các đặc điểm bằng dấu phẩy và loại bỏ khoảng trắng thừa
                var dacDiemList = dacDiem.Split(new[] { ',', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x => x.Trim())
                                      .Where(x => !string.IsNullOrWhiteSpace(x))
                                      .ToList();

                if (dacDiemList.Any())
                {
                    // Tìm kiếm theo OR - bài viết chứa bất kỳ đặc điểm nào trong danh sách
                    query = query.Where(x => dacDiemList.Any(d => x.DaciemNhanDang.Contains(d)));
                    d++;
                }
            }
            string tenTinhThanh = "";
            if (tinhThanhId.HasValue)
            {
                var tinh = await db.TinhThanhs.FindAsync(tinhThanhId.Value);
                if (tinh != null)
                    tenTinhThanh = tinh.TenTinhThanh;
            }
            if (d > 0)
            {
                string tenQuanHuyen = "";

               

                if (quanHuyenId.HasValue)
                {
                    var quan = await db.QuanHuyens.FindAsync(quanHuyenId.Value);
                    if (quan != null)
                        tenQuanHuyen = quan.TenQuanHuyen;
                }

                khuVuc = $"{tenQuanHuyen} {tenTinhThanh}".Trim();
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
                        TuKhoa = ten + khuVuc + dacDiem,
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

                            if (nguoiDungViPham.SoLanViPham >= 3)
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

            // Lưu các giá trị filter vào ViewBag
            ViewBag.TenFilter = ten;

            // XL KHU VỰC
            ViewBag.KhuVucFilter = khuVuc;

            ViewBag.TinhThanhList = await db.TinhThanhs.ToListAsync();
            if (tinhThanhId.HasValue)
            {
                ViewBag.QuanHuyenList = await db.QuanHuyens
                    .Where(q => q.IdTinhThanh == tinhThanhId.Value)
                    .OrderBy(q => q.TenQuanHuyen)
                    .ToListAsync();
            }
            else
            {
                ViewBag.QuanHuyenList = new List<QuanHuyen>();
            }

            ViewBag.SelectedTinhThanh = tinhThanhId;
            ViewBag.SelectedQuanHuyen = quanHuyenId;
            ViewBag.SelectedNameTinhThanh = tenTinhThanh;

            ViewBag.DacDiemFilter = dacDiem;
            //ViewBag.TinhThanhList = new SelectList(TinhThanhIEnumerable);

            // Sắp xếp và phân trang
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
            else
            {
                TempData["WarningMessage"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
                return Redirect("/Identity/Account/Login");
            }
            //ViewBag.DanhSachTinhThanh = TinhThanhIEnumerable;

            IEnumerable<TinhThanh> tinhThanhs = await db.TinhThanhs.ToListAsync();
            IEnumerable<QuanHuyen> quanHuyens = await db.QuanHuyens.ToListAsync();


            //ViewBag.DanhSachTinhThanh = new SelectList(tinhThanhs, "Id", "TenTinhThanh");
            //ViewBag.DanhSachQuanHuyen = new SelectList(quanHuyens, "Id", "TenQuanHuyen");
            ViewBag.DanhSachTinhThanh = tinhThanhs;
            ViewBag.DanhSachQuanHuyen = quanHuyens;

            return View();
        }

        private async Task LoadSelectListsAsync()
        {
            var tinhThanhs = await db.TinhThanhs.ToListAsync();
            var quanHuyens = await db.QuanHuyens.ToListAsync();
            ViewBag.DanhSachTinhThanh = new SelectList(tinhThanhs, "Id", "TenTinhThanh");
            ViewBag.DanhSachQuanHuyen = new SelectList(quanHuyens, "Id", "TenQuanHuyen");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemNguoiCanTim(
                                                                TimNguoi timNguoi,
                                                                List<IFormFile> DSHinhAnhCapNhat,
                                                                string? faceDescriptorsJson)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            var nguoiDung = await _userManager.GetUserAsync(User);
            if (nguoiDung == null || nguoiDung.Active == false)
            {
                TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                return RedirectToAction("Index", "LoiViPham", new { area = "" });
            }
            await LoadSelectListsAsync();


            if (!ModelState.IsValid)
            {
                IEnumerable<TinhThanh> tinhThanhs = await db.TinhThanhs.ToListAsync();
                IEnumerable<QuanHuyen> quanHuyens = await db.QuanHuyens.ToListAsync();



                //ViewBag.DanhSachTinhThanh = new SelectList(tinhThanhs, "Id", "TenTinhThanh");
                //ViewBag.DanhSachQuanHuyen = new SelectList(quanHuyens, "Id", "TenQuanHuyen");
                ViewBag.DanhSachTinhThanh = tinhThanhs;
                ViewBag.DanhSachQuanHuyen = quanHuyens;

                ViewBag.SelectedTinhThanh = timNguoi.IdTinhThanh;
                ViewBag.SelectedQuanHuyen = timNguoi.IdQuanHuyen;
                return View(timNguoi);
            }

            if (DSHinhAnhCapNhat == null || DSHinhAnhCapNhat.Count == 0)
            {
                ModelState.AddModelError("Lỗi", "Chưa Có Hình Ảnh");
                IEnumerable<TinhThanh> tinhThanhs = await db.TinhThanhs.ToListAsync();
                IEnumerable<QuanHuyen> quanHuyens = await db.QuanHuyens.ToListAsync();



                //ViewBag.DanhSachTinhThanh = new SelectList(tinhThanhs, "Id", "TenTinhThanh");
                //ViewBag.DanhSachQuanHuyen = new SelectList(quanHuyens, "Id", "TenQuanHuyen");
                ViewBag.DanhSachTinhThanh = tinhThanhs;
                ViewBag.DanhSachQuanHuyen = quanHuyens;

                ViewBag.SelectedTinhThanh = timNguoi.IdTinhThanh;
                ViewBag.SelectedQuanHuyen = timNguoi.IdQuanHuyen;
                string tt = db.TinhThanhs.FirstOrDefault(i => i.Id == timNguoi.IdTinhThanh).TenTinhThanh ?? "";
                ViewBag.SelectedNameTinhThanh = tt;
                return View(timNguoi);
            }

            // Xử lý descriptors từ client
            FaceDescriptorData? descriptorData = null;
            if (!string.IsNullOrEmpty(faceDescriptorsJson))
            {
                try
                {
                    descriptorData = JsonConvert.DeserializeObject<FaceDescriptorData>(faceDescriptorsJson);
                    _logger.LogInformation("Parsed faceDescriptorsJson: DescriptorsCount={DescriptorsCount}, AverageDescriptorLength={AvgLength}",
                        descriptorData?.Descriptors?.Count, descriptorData?.AverageDescriptor?.Length);

                    if (descriptorData?.AverageDescriptor != null && descriptorData.AverageDescriptor.Length == 128)
                    {
                        timNguoi.AverageDescriptor = descriptorData.AverageDescriptor;
                        _logger.LogInformation("Đã gán AverageDescriptor cho bài viết ID {Id}", timNguoi.Id);
                    }
                    else
                    {
                        _logger.LogWarning("AverageDescriptor không hợp lệ: {Length}", descriptorData?.AverageDescriptor?.Length);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý face descriptors: {Json}", faceDescriptorsJson);
                }
            }
            else
            {
                _logger.LogWarning("faceDescriptorsJson rỗng hoặc null");
            }

            timNguoi.active = false;

            db.Add(timNguoi);
            await db.SaveChangesAsync();


            TimNguoi z = db.TimNguois.FirstOrDefault(i => i.Id == timNguoi.Id);
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}/TimNguoi/ChiTietBaiTimNguoi/{z.Id}";
            z.QRCodeImage = QRCodeHelper.GenerateQRCode(baseUrl);
            await db.SaveChangesAsync();


            int primaryImageIndex = 0;
            for (int i = 0; i < DSHinhAnhCapNhat.Count; i++)
            {
                var file = DSHinhAnhCapNhat[i];
                var anhTimNguoi = new AnhTimNguoi
                {
                    IdNguoiCanTim = timNguoi.Id,
                    TrangThai = (i == primaryImageIndex) ? 1 : 0,
                    HinhAnh = await SaveImage(file, "AnhNguoiCanTim")
                };

                if (descriptorData?.Descriptors != null && i < descriptorData.Descriptors.Count)
                {
                    anhTimNguoi.FaceDescriptor = JsonConvert.SerializeObject(descriptorData.Descriptors[i]);
                    _logger.LogInformation("Saved FaceDescriptor for image {Index}: Length = {Length}", i, descriptorData.Descriptors[i].Length);
                }

                db.AnhTimNguois.Add(anhTimNguoi);
            }

            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm bài viết thành công. Bài viết đang chờ kiểm duyệt.";
            return Redirect("/Identity/Account/Manage/Index");
        }

        public class FaceDescriptorData
        {
            public List<float[]> Descriptors { get; set; }
            public float[] AverageDescriptor { get; set; }
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
                return;
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
            if (User.Identity.IsAuthenticated == false)
            {
                TempData["WarningMessage"] = "Bạn cần đăng nhập để thực hiện xem chi tiết để bảo vệ thông tin cá nhân chức năng này.";
                return Redirect("/Identity/Account/login");
            }

            ApplicationUser x = await _userManager.GetUserAsync(User);
            if (x != null)
            {
                if (x.Active == false)
                {
                    _logger.LogWarning($"Tài khoản {x.Email} đã bị vô hiệu hóa do vi phạm quy định.");
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return RedirectToAction("Index", "LoiViPham", new { area = "" });
                }

                var timNguoi = await db.TimNguois
                    .Include(u => u.AnhTimNguois)
                    .Include(u => u.TimThayNguoiThatLacs)
                    .Include(u => u.QuanHuyen)
                    .Include(u => u.TinhThanh)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (timNguoi == null)
                {
                    return NotFound();
                }

                ApplicationUser us = await _userManager.FindByIdAsync(timNguoi.IdNguoiDung);
                ViewBag.NguoiTim = us;
                ViewBag.DSHinhAnh = await db.AnhTimNguois
                                            .Where(i => i.IdNguoiCanTim == timNguoi.Id)
                                            .ToListAsync();

                var nguoiDung = await _userManager.GetUserAsync(User);
                if (timNguoi.active == false && timNguoi.ApplicationUser.Id != nguoiDung.Id)
                {
                    TempData["ErrorMessage"] = "Bài Viết Chưa Được Duyệt Hoặc Đã Bị Khóa, Nếu Có Thắc Mắc Xin Liên Hệ Với Admin";
                    return RedirectToAction("Index");
                }
                if (timNguoi.active == false)
                {
                    TempData["ErrorMessage"] = "Bài Viết Chưa Được Duyệt Hoặc Đã Bị Khóa, Nếu Có Thắc Mắc Xin Liên Hệ Với Admin";
                }

                List<BinhLuan> DSBinhLuan = await db.BinhLuans
                                                .Include(u => u.ApplicationUser)
                                                .Include(m => m.TraLois)
                                                .Where(i => i.IdBaiViet == id && i.Active == true && i.NguoiDangBaiXoa == false)
                                                .OrderByDescending(z => z.NgayBinhLuan)
                                                .ToListAsync();

                List<NhanChung> DSHANC = await db.NhanChungs
                                            .Where(i => i.TimNguoiId == id)
                                            .ToListAsync();

                if (idBinhLuan != 0)
                {
                    BinhLuan xBinhLuan = await db.BinhLuans.FirstOrDefaultAsync(i => i.Id == idBinhLuan);
                    if (xBinhLuan != null)
                    {
                        xBinhLuan.DaDoc = true;
                        await db.SaveChangesAsync();
                    }
                }

                ViewBag.KhuVuc = timNguoi.KhuVuc;
                ViewBag.QuanHuyen = timNguoi.QuanHuyen?.TenQuanHuyen;
                ViewBag.TinhThanh = timNguoi.QuanHuyen?.TinhThanh?.TenTinhThanh;
                ViewBag.DaTimThay = timNguoi.TimThayNguoiThatLacs != null;
                ViewBag.DSBinhLuan = DSBinhLuan;
                ViewBag.DSHANhanChung = DSHANC;
                ViewData["TimNguoiId"] = id;

                return View(timNguoi);
            }

            return RedirectToAction("ThongTinCaNhan", "NguoiDung");
        }

        [HttpPost]
        public async Task<IActionResult> ThemBinhLuan(int IdBaiViet, string UserId, string NoiDung, IFormFile? HinhAnh)
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

                if (baiViet == null || baiViet.ApplicationUser.Email == null)
                {
                    TempData["Warning"] = "Bài viết không tồn tại hoặc không có thông tin người dùng.";
                    return RedirectToAction("Index", "Home");
                }
                // Xử lý upload hình ảnh
                if (ModelState.IsValid)
                {

                    BinhLuan x = new BinhLuan();
                    if (HinhAnh != null)
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


        [HttpPost]
        public async Task<IActionResult> EditComment(int commentId, string noiDung, IFormFile hinhAnh, bool removeImage = false)
        {
            try
            {
                var comment = await db.BinhLuans.FirstOrDefaultAsync(i => i.Id == commentId);
                if (comment == null)
                {
                    return Json(new { success = false, message = "Bình luận không tồn tại" });
                }

                // Kiểm tra quyền chỉnh sửa
                var currentUserId = _userManager.GetUserId(User);
                if (comment.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa bình luận này" });
                }

                // Cập nhật nội dung
                comment.NoiDung = noiDung;
                //comment.NgayChinhSua = DateTime.Now;

                // Xử lý ảnh
                string imagePath = null;
                if (removeImage && !string.IsNullOrEmpty(comment.HinhAnh))
                {
                    // Xóa ảnh cũ
                    DeleteImage(comment.HinhAnh , "BinhLuan");
                    comment.HinhAnh = null;
                }
                else if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(comment.HinhAnh))
                    {
                        DeleteImage(comment.HinhAnh, "BinhLuan");
                    }

                    // Lưu ảnh mới
                   

                    comment.HinhAnh = await SaveImage(hinhAnh, "BinhLuan");
                    imagePath = comment.HinhAnh;
                }

                db.Update(comment);
                await db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    noiDung = comment.NoiDung,
                    hinhAnh = imagePath,
                    removeImage = removeImage && string.IsNullOrEmpty(imagePath)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyComment(ReplyViewModel model)
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = errors
                });
            }

            try
            {
                // Kiểm tra sự tồn tại của bài viết
                var postExists = await db.TimNguois.AnyAsync(p => p.Id == model.PostId);
                if (!postExists)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Bài viết không tồn tại"
                    });
                }

                // Kiểm tra comment cha nếu có ParentId
                if (model.ParentCommentId.HasValue)
                {
                    var parentCommentExists = await db.BinhLuans.AnyAsync(c => c.Id == model.ParentCommentId.Value);
                    if (!parentCommentExists)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Bình luận cha không tồn tại"
                        });
                    }
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Người dùng không hợp lệ"
                    });
                }

                var comment = new BinhLuan
                {
                    NoiDung = model.NoiDung,
                    ParentId = model.ParentCommentId,
                    IdBaiViet = model.PostId,
                    UserId = userId,
                    NgayBinhLuan = DateTime.Now
                };

                if (model.HinhAnh != null)
                {
                    try
                    {
                        // Xử lý upload ảnh với kích thước tối đa 5MB
                        if (model.HinhAnh.Length > 5 * 1024 * 1024)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Kích thước ảnh tối đa là 5MB"
                            });
                        }

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(model.HinhAnh.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)"
                            });
                        }

                        comment.HinhAnh = await SaveImage(model.HinhAnh, "BinhLuan");
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Lỗi khi upload ảnh",
                            error = ex.Message
                        });
                    }
                }

                db.BinhLuans.Add(comment);
                await db.SaveChangesAsync();

                // Trả về thông tin comment mới để cập nhật UI
                var newComment = await db.BinhLuans
                    .Include(c => c.ApplicationUser)
                    .FirstOrDefaultAsync(c => c.Id == comment.Id);

                return Json(new
                {
                    success = true,
                    comment = new
                    {
                        id = newComment.Id,
                        content = newComment.NoiDung,
                        image = newComment.HinhAnh,
                        date = newComment.NgayBinhLuan.ToString("dd/MM/yyyy HH:mm"),
                        userName = newComment.ApplicationUser?.FullName,
                        userImage = newComment.ApplicationUser?.HinhAnh,
                        isAuthor = newComment.ApplicationUser?.Id == newComment.TimNguoi?.IdNguoiDung,
                        parentId = newComment.ParentId // Thêm parentId để xác định vị trí thêm comment
                    }
                });
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây (sử dụng ILogger)
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi thêm bình luận",
                    error = ex.Message
                });
            }
        }

        

        public async Task<IActionResult> CapNhatBaiViet(int id)
        {

            if (User.Identity.IsAuthenticated)
            {
                //ViewBag.DanhSachTinhThanh = TinhThanhIEnumerable;
                await LoadSelectListsAsync();

                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var nguoiDung = await _userManager.GetUserAsync(User);
                if (nguoiDung == null || nguoiDung.Active == false)
                {
                    TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                    return Redirect("/Identity/Account/login");
                }
                TimNguoi x = db.TimNguois
                                    .Include(u => u.ApplicationUser)
                                    .Include(u => u.AnhTimNguois)
                                    .Include(u => u.BinhLuans)
                                        .ThenInclude(m => m.TraLois)
                                    .FirstOrDefault(i => i.Id == id);
                var roles = await _userManager.GetRolesAsync(nguoiDung);

                bool check = (roles.Contains(SD.Role_Admin) || roles.Contains(SD.Role_Moderator));

                IEnumerable<TinhThanh> tinhThanhs = await db.TinhThanhs.ToListAsync();
                IEnumerable<QuanHuyen> quanHuyens = await db.QuanHuyens.ToListAsync();



                ViewBag.DanhSachTinhThanh = tinhThanhs;
                ViewBag.DanhSachQuanHuyen = quanHuyens;
                string tt = db.TinhThanhs.FirstOrDefault(i => i.Id == x.IdTinhThanh).TenTinhThanh ?? "";
                ViewBag.SelectedNameTinhThanh = tt;

                ViewBag.SelectedTinhThanh = x.IdTinhThanh ?? 0;
                ViewBag.SelectedQuanHuyen = x.IdQuanHuyen ?? 0;
               

                if (x.IdNguoiDung == userid || check == true)
                {
                    ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == id).ToList();
                    return View(x);
                }
                else
                {
                    TempData["WarningMessage"] = "Cần phải đăng nhập đúng tài khoản để chỉnh sửa";
                    return Redirect("/Identity/Account/login");
                }
            }

            TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
            return Redirect("/Identity/Account/login");

        }

        

        [HttpPost]
        public async Task<IActionResult> CapNhatBaiViet(TimNguoi x, List<IFormFile>? DSHinhAnhCapNhat, string? faceDescriptorsJson)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["WarningMessage"] = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                return Redirect("/Identity/Account/login");
            }
            var nguoiDung = await _userManager.GetUserAsync(User);
            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            TimNguoi y = db.TimNguois
                                .Include(u => u.ApplicationUser)
                                .Include(u => u.AnhTimNguois)
                                .Include(u => u.BinhLuans)
                                .Include(u => u.TinhThanh)
                                .Include(u => u.QuanHuyen)
                                .FirstOrDefault(i => i.Id == x.Id);

            var roles = await _userManager.GetRolesAsync(nguoiDung);

            bool check = (roles.Contains(SD.Role_Admin) || roles.Contains(SD.Role_Moderator));

            if (y == null || (y.IdNguoiDung != userid && check == false) || nguoiDung.Active == false)
            {
                TempData["WarningMessage"] = "Cần phải đăng nhập đúng tài khoản để chỉnh sửa";
                return Redirect("/Identity/Account/login");
            }

            // Luôn thiết lập ViewBag trước khi trả về View
            IEnumerable<TinhThanh> tinhThanhs = await db.TinhThanhs.ToListAsync();
            IEnumerable<QuanHuyen> quanHuyens = await db.QuanHuyens.ToListAsync();



           
            ViewBag.DanhSachTinhThanh = tinhThanhs;
            ViewBag.DanhSachQuanHuyen = quanHuyens;

            ViewBag.SelectedTinhThanh = y.IdTinhThanh;
            ViewBag.SelectedQuanHuyen = y.IdQuanHuyen;
            string tt = db.TinhThanhs.FirstOrDefault(i => i.Id == x.IdTinhThanh).TenTinhThanh ?? "";
            ViewBag.SelectedNameTinhThanh = tt;

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
            y.NgaySinh = x.NgaySinh;
            y.NgayMatTich = x.NgayMatTich;
            y.HoTen = x.HoTen;
            y.MoiQuanHe = x.MoiQuanHe;
            y.IdQuanHuyen = x.IdQuanHuyen;
            y.IdTinhThanh = x.IdTinhThanh;

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

                        await db.SaveChangesAsync();
                        await transaction.CommitAsync(); // QUAN TRỌNG: Phải commit transaction


                    }
                    catch
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật hình ảnh");
                        return View(x);
                    }


                    // Xử lý descriptors từ client
                    FaceDescriptorData? descriptorData = null;
                    if (!string.IsNullOrEmpty(faceDescriptorsJson))
                    {
                        try
                        {
                            descriptorData = JsonConvert.DeserializeObject<FaceDescriptorData>(faceDescriptorsJson);
                            _logger.LogInformation("Parsed faceDescriptorsJson: DescriptorsCount={DescriptorsCount}, AverageDescriptorLength={AvgLength}",
                                descriptorData?.Descriptors?.Count, descriptorData?.AverageDescriptor?.Length);

                            if (descriptorData?.AverageDescriptor != null && descriptorData.AverageDescriptor.Length == 128)
                            {
                                y.AverageDescriptor = descriptorData.AverageDescriptor; // cập nhật
                                _logger.LogInformation("Đã gán AverageDescriptor cho bài viết ID {Id}", y.Id);
                            }
                            else
                            {
                                _logger.LogWarning("AverageDescriptor không hợp lệ: {Length}", descriptorData?.AverageDescriptor?.Length);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi xử lý face descriptors: {Json}", faceDescriptorsJson);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("faceDescriptorsJson rỗng hoặc null");
                    }

                    await db.SaveChangesAsync();

                    int primaryImageIndex = 0;
                    for (int i = 0; i < DSHinhAnhCapNhat.Count; i++)
                    {
                        var file = DSHinhAnhCapNhat[i];
                        var anhTimNguoi = new AnhTimNguoi
                        {
                            IdNguoiCanTim = y.Id,
                            TrangThai = (i == primaryImageIndex) ? 1 : 0,
                            HinhAnh = await SaveImage(file, "AnhNguoiCanTim")
                        };

                        if (descriptorData?.Descriptors != null && i < descriptorData.Descriptors.Count)
                        {
                            anhTimNguoi.FaceDescriptor = JsonConvert.SerializeObject(descriptorData.Descriptors[i]);
                            _logger.LogInformation("Saved FaceDescriptor for image {Index}: Length = {Length}", i, descriptorData.Descriptors[i].Length);
                        }

                        db.AnhTimNguois.Add(anhTimNguoi);
                    }

                    await db.SaveChangesAsync();
                }
            }

            // Lưu các thay đổi khác (luôn thực hiện)
            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
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
        public async Task<IActionResult> BaoCaoBaiViet(int MaBaiViet, string LyDo, string ChiTiet, IFormFile? HinhAnhBaoCaoBaiViet)
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
                if (HinhAnhBaoCaoBaiViet != null && HinhAnhBaoCaoBaiViet.Length > 0)
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
                            applicationUser.SoLanViPham++;
                            await db.SaveChangesAsync();

                            TimNguoi bv = db.TimNguois.FirstOrDefault(z => z.Id == comment.IdBaiViet);

                            HanhViDangNgo hanhVi = new HanhViDangNgo
                            {
                                NguoiDungId = applicationUser.Id,
                                HanhDong = "Bình luận bị báo cáo nhiều lần, Nghi Ngời vi phạm tiêu chuẩn cộng đồng",
                                ThoiGian = DateTime.Now,
                                IdLoiViPham = comment.Id,
                                LoaiViPham = "Bình Luận",
                                ChiTiet = "Bình luận bị báo cáo nhiều lần, Nghi Ngờ vi phạm tiêu chuẩn cộng đồng " +
                                "tại bình luận của bài viết : " + bv?.TieuDe

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
                                ChiTiet = $"bài viết {post.TieuDe} bị báo cáo nhiều lần nghi ngời vi phạm tiêu chuẩn cộng đồng"

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

                var nguoiDung = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(nguoiDung);

                bool check = (roles.Contains(SD.Role_Admin) || roles.Contains(SD.Role_Moderator));
                if (x.IdNguoiDung == userid || check == true)
                {
                    ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == id).ToList();
                    ViewBag.TimNguoi = x;
                    return View();
                }
                else
                {
                    TempData["WarningMessage"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
                    return Redirect("/Identity/Account/Login");
                }
            }

            TempData["WarningMessage"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
            return Redirect("/Identity/Account/Login");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuatFileBaiViet(int id)
        {
            try
            {
                // Get the blog post
                var baiViet = await db.TimNguois
                    .Include(b => b.ApplicationUser)
                    .FirstOrDefaultAsync(b => b.Id == id);


                if (baiViet == null)
                    return Json(new { success = false, message = "Không tìm thấy bài viết" });


                ApplicationUser nguoidung = db.Users.FirstOrDefault(i => i.Id == baiViet.IdNguoiDung);
                if (nguoidung == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin người dùng"
                    });
                }

                // Create Word document
                var wordService = new WordExportService();
                byte[] fileContents = wordService.CreateOfficialReport(baiViet, nguoidung);

                var fileName = $"DonTrinhBao_{baiViet.Id}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                // Save file permanently on server
                var folderPath = Path.Combine("wwwroot", "DangKiTimKiem");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, fileContents);

                // Update file path in database if needed
                baiViet.DonDangKiTrinhBao = Path.Combine("DangKiTimKiem", fileName);
                await db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    downloadUrl = Url.Action("DownloadBlogPost", new { id = baiViet.Id, fileName = fileName })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất file bài viết");
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xuất file",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBlogPost(int id, string fileName)
        {
            var filePath = Path.Combine("wwwroot", "DangKiTimKiem", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileContents = await System.IO.File.ReadAllBytesAsync(filePath);

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

        // Hàm đệ quy xóa bình luận và các reply
        private async Task DeleteCommentAndReplies(BinhLuan comment)
        {
            // Xóa hình ảnh nếu có
            //if (!string.IsNullOrEmpty(comment.HinhAnh))
            //{
            //    DeleteImage(comment.HinhAnh, "BinhLuan");
            //}

            // Xóa đệ quy các reply
            if (comment.TraLois != null && comment.TraLois.Any())
            {
                foreach (var reply in comment.TraLois.ToList())
                {
                    await DeleteCommentAndReplies(reply);
                }
            }

            //db.BinhLuans.Remove(comment);
            comment.NguoiDangBaiXoa = true;
        }

        public async Task<IActionResult> TimKiemNangCao(List<string> dacDiemDaChon) // Tìm kiếm nhân cao
        {
            var viewModel = new TimKiemNangCaoViewModel();

            // Loại bỏ các đặc điểm trùng lặp và rỗng
            dacDiemDaChon = dacDiemDaChon?
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .ToList() ?? new List<string>();

            // Lấy tất cả bài đăng kèm hình ảnh
            var allPosts = await db.TimNguois
                .Include(t => t.AnhTimNguois)
                .Where(t => t.active)
                .ToListAsync();

            // Xử lý đặc điểm nhận dạng
            var allDacDiem = new List<string>();
            var dacDiemRelations = new Dictionary<string, HashSet<string>>();

            foreach (var post in allPosts)
            {
                if (!string.IsNullOrEmpty(post.DaciemNhanDang))
                {
                    var dacDiems = post.DaciemNhanDang.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => d.Trim())
                        .Distinct()
                        .ToList();

                    allDacDiem.AddRange(dacDiems);

                    // Xây dựng mối quan hệ giữa các đặc điểm
                    foreach (var dacDiem in dacDiems)
                    {
                        if (!dacDiemRelations.ContainsKey(dacDiem))
                        {
                            dacDiemRelations[dacDiem] = new HashSet<string>();
                        }

                        foreach (var relatedDacDiem in dacDiems.Where(d => d != dacDiem))
                        {
                            dacDiemRelations[dacDiem].Add(relatedDacDiem);
                        }
                    }
                }
            }

            // Nhóm và đếm số lần xuất hiện của mỗi đặc điểm
            var groupedDacDiem = allDacDiem
                .GroupBy(d => d)
                .Select(g => new DacDiemNhanDangGroup
                {
                    DacDiem = g.Key,
                    SoLuong = g.Count(),
                    DaChon = dacDiemDaChon.Contains(g.Key)
                })
                .OrderByDescending(g => g.SoLuong)
                .ToList();

            // Thêm các đặc điểm liên quan
            foreach (var item in groupedDacDiem)
            {
                if (dacDiemRelations.TryGetValue(item.DacDiem, out var related))
                {
                    item.DacDiemLienQuan = groupedDacDiem
                        .Where(g => related.Contains(g.DacDiem))
                        .OrderByDescending(g => g.SoLuong)
                        .ToList();
                }
            }

            // Lọc kết quả tìm kiếm
            if (dacDiemDaChon.Any())
            {
                var filteredPosts = allPosts
                    .Where(p => !string.IsNullOrEmpty(p.DaciemNhanDang))
                    .Select(p => new
                    {
                        Post = p,
                        DacDiems = p.DaciemNhanDang.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => d.Trim())
                            .Distinct()
                            .ToList(),
                        MatchCount = p.DaciemNhanDang.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => d.Trim())
                            .Count(d => dacDiemDaChon.Contains(d))
                    })
                    .Where(x => x.MatchCount > 0)
                    .OrderByDescending(x => x.MatchCount)
                    .ToList();

                // Tính % khớp và tạo kết quả
                viewModel.KetQuaTimKiem = filteredPosts.Select(x => new KetQuaTimKiemItem
                {
                    BaiDang = x.Post,
                    PhanTramKhop = Math.Round((double)x.MatchCount / dacDiemDaChon.Count * 100, 1),
                    AnhDaiDien = x.Post.AnhTimNguois.Take(1).ToList() // Lấy 1 ảnh đại diện
                })
                    .ToList();

                // Lọc các đặc điểm có thể chọn (chỉ hiển thị các đặc điểm liên quan)
                var relatedDacDiem = filteredPosts
                    .SelectMany(x => x.DacDiems)
                    .Distinct()
                    .Where(d => !dacDiemDaChon.Contains(d))
                    .ToList();

                groupedDacDiem = groupedDacDiem
                    .Where(g => dacDiemDaChon.Contains(g.DacDiem) || relatedDacDiem.Contains(g.DacDiem))
                    .ToList();
            }
            else
            {
                // Nếu không có đặc điểm nào được chọn, hiển thị tất cả bài đăng
                viewModel.KetQuaTimKiem = allPosts.Select(p => new KetQuaTimKiemItem
                {
                    BaiDang = p,
                    PhanTramKhop = 0,
                    AnhDaiDien = p.AnhTimNguois.Take(1).ToList()
                })
                    .OrderByDescending(x => x.BaiDang.NgayDang)
                    .ToList();
            }

            viewModel.DacDiemDaChon = dacDiemDaChon;
            viewModel.DacDiemCoTheChon = groupedDacDiem;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> NguoiDungXoaBai(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {

                return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện xóa bài viết" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var baiViet = await db.TimNguois.FirstOrDefaultAsync(i => i.Id == id);

                if (currentUser == null || currentUser.Active == false)
                {
                    return Json(new { success = false, message = "Tài khoản không hợp lệ" });
                }

                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết" });
                }

                baiViet.NguoiDangBaiXoa = true;
                baiViet.active = false;

                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã Xóa Bài Viết Tìm " + baiViet.TieuDe + " Thành Công";
                return Json(new { success = true, message = "Đã Xóa Bài Viết Tìm " + baiViet.TieuDe + " Thành Công" });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa bài viết" });
            }
        }

        public async Task<IActionResult> TimKiemBangHinhAnh()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TimKiemBangHinhAnh([FromBody] FaceSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Bắt đầu tìm kiếm bằng hình ảnh, Descriptor length = {Length}", request?.Descriptor?.Length);
                if (request?.Descriptor == null || request.Descriptor.Length != 128)
                {
                    _logger.LogWarning("Dữ liệu khuôn mặt không hợp lệ: {DescriptorLength}", request?.Descriptor?.Length);
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Dữ liệu khuôn mặt không hợp lệ",
                        ErrorCode = "INVALID_DESCRIPTOR"
                    });
                }

                _logger.LogInformation("Truy vấn bài viết từ database");
                var posts = await db.TimNguois
                    .Include(p => p.AnhTimNguois)
                    .Where(p => p.active) // Chỉ lấy bài viết active
                    .Select(p => new
                    {
                        p.Id,
                        p.TieuDe,
                        p.MoTa,
                        p.NgayDang,
                        p.NgayMatTich,
                        p.HoTen,
                        p.DaciemNhanDang,
                        p.GioiTinh,
                        MainImage = p.AnhTimNguois.FirstOrDefault(a => a.TrangThai == 1).HinhAnh,
                        p.AverageDescriptorBytes
                    })
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Đã lấy được {PostCount} bài viết", posts.Count);
                if (!posts.Any())
                {
                    _logger.LogWarning("Không có bài viết nào trong database");
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Data = new
                        {
                            posts = new List<object>(),
                            metadata = new { totalCompared = 0, matchedCount = 0 }
                        },
                        Message = "Không có bài viết nào để so sánh"
                    });
                }

                var results = new List<SearchResult>();
                foreach (var post in posts)
                {
                    try
                    {
                        if (post.AverageDescriptorBytes == null || post.AverageDescriptorBytes.Length != 512)
                        {
                            _logger.LogWarning("Bài viết ID {PostId} có AverageDescriptorBytes null hoặc không hợp lệ, Length = {Length}", post.Id, post.AverageDescriptorBytes?.Length);
                            continue;
                        }

                        _logger.LogInformation("Xử lý bài viết ID {PostId}, DescriptorBytes length = {Length}", post.Id, post.AverageDescriptorBytes.Length);
                        var postDescriptor = WebTimNguoiThatLac.Helpers.FaceRecognitionHelper.ToFloatArray(post.AverageDescriptorBytes);
                        if (postDescriptor == null || postDescriptor.Length != 128)
                        {
                            _logger.LogWarning("Descriptor không hợp lệ cho bài viết ID {PostId}: Length = {Length}", post.Id, postDescriptor?.Length);
                            continue;
                        }

                        var similarity = 1 - FaceRecognitionHelper.CalculateEuclideanDistance(request.Descriptor, postDescriptor);
                        _logger.LogInformation("Bài viết ID {PostId}: Similarity = {Similarity}", post.Id, similarity);
                        if (similarity > 0.4) // Giảm ngưỡng xuống 0.4
                        {
                            results.Add(new SearchResult
                            {
                                PostId = post.Id,
                                Title = post.TieuDe,
                                Description = post.MoTa,
                                NhanDang = post.DaciemNhanDang,
                                NgayMatTich = post.NgayMatTich,
                                GioiTinh = post.GioiTinh,
                                Name = post.HoTen,
                                PostedDate = post.NgayDang,
                                ImageUrl = post.MainImage ?? "/images/default-avatar.jpg",
                                Similarity = similarity
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Lỗi khi xử lý bài viết ID {PostId}: {ErrorMessage}", post.Id, ex.Message);
                        continue;
                    }
                }

                _logger.LogInformation("Tìm thấy {ResultCount} kết quả", results.Count);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        posts = results.OrderByDescending(M => M.Similarity).Select(r => new {
                            PostId = r.PostId,
                            Title = r.Title,
                            Description = r.Description,
                            NhanDang = r.NhanDang,
                            NgayMatTich = r.NgayMatTich,
                            GioiTinh = r.GioiTinh,
                            Name =  r.Name,
                            PostedDate = r.PostedDate,
                            ImageUrl = r.ImageUrl ?? "/images/default-avatar.jpg",
                            Similarity = r.Similarity
                        }),
                        metadata = new
                        {
                            totalCompared = posts.Count,
                            matchedCount = results.Count
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi tìm kiếm bằng hình ảnh: {ErrorMessage}", ex.Message);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống: " + ex.Message,
                    ErrorCode = "SERVER_ERROR"
                });
            }
        }

       public async Task<IActionResult> DanhSachBaiVietTimThay(string ten, int? tinhThanhId, int? quanHuyenId, string khuVuc, string dacDiem, int page = 1)
        {
            int pageSize = 6; // Số bài viết mỗi trang

            var query = db.TimNguois
                .Include(u => u.ApplicationUser)
                .Include(u => u.AnhTimNguois)
                .Include(u => u.TinhThanh)
                .Include(u => u.QuanHuyen)
                .Where(i => i.active == true && i.TrangThai == "Đã Tìm Thấy");

            int d = 0;
            // Áp dụng bộ lọc tên
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

            // Áp dụng bộ lọc đặc điểm nhận dạng (nâng cấp)
            if (!string.IsNullOrEmpty(dacDiem))
            {
                // Tách các đặc điểm bằng dấu phẩy và loại bỏ khoảng trắng thừa
                var dacDiemList = dacDiem.Split(new[] { ',', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x => x.Trim())
                                      .Where(x => !string.IsNullOrWhiteSpace(x))
                                      .ToList();

                if (dacDiemList.Any())
                {
                    // Tìm kiếm theo OR - bài viết chứa bất kỳ đặc điểm nào trong danh sách
                    query = query.Where(x => dacDiemList.Any(d => x.DaciemNhanDang.Contains(d)));
                    d++;
                }
            }
            string tenTinhThanh = "";
            if (tinhThanhId.HasValue)
            {
                var tinh = await db.TinhThanhs.FindAsync(tinhThanhId.Value);
                if (tinh != null)
                    tenTinhThanh = tinh.TenTinhThanh;
            }
            if (d > 0)
            {
                string tenQuanHuyen = "";



                if (quanHuyenId.HasValue)
                {
                    var quan = await db.QuanHuyens.FindAsync(quanHuyenId.Value);
                    if (quan != null)
                        tenQuanHuyen = quan.TenQuanHuyen;
                }

                khuVuc = $"{tenQuanHuyen} {tenTinhThanh}".Trim();
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
                        TuKhoa = ten + khuVuc + dacDiem,
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

                            if (nguoiDungViPham.SoLanViPham >= 3)
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

            // Lưu các giá trị filter vào ViewBag
            ViewBag.TenFilter = ten;

            // XL KHU VỰC
            //ViewBag.KhuVucFilter = khuVuc;

            ViewBag.TinhThanhList = await db.TinhThanhs.ToListAsync();
            if (tinhThanhId.HasValue)
            {
                ViewBag.QuanHuyenList = await db.QuanHuyens
                    .Where(q => q.IdTinhThanh == tinhThanhId.Value)
                    .OrderBy(q => q.TenQuanHuyen)
                    .ToListAsync();
            }
            else
            {
                ViewBag.QuanHuyenList = new List<QuanHuyen>();
            }

            ViewBag.SelectedTinhThanh = tinhThanhId;
            ViewBag.SelectedQuanHuyen = quanHuyenId;
            ViewBag.SelectedNameTinhThanh = tenTinhThanh;

            ViewBag.DacDiemFilter = dacDiem;
            //ViewBag.TinhThanhList = new SelectList(TinhThanhIEnumerable);

            // Sắp xếp và phân trang
            var pagedList = query.OrderByDescending(x => x.Id)
                                .ToPagedList(page, pageSize);

            return View(pagedList);
       }

     

        public IActionResult DownloadQRCode(int id)
        {
            var post = db.TimNguois.FirstOrDefault(p => p.Id == id);
            if (post == null || post.QRCodeImage == null || post.QRCodeImage.Length == 0)
            {
                // Tạo QR code nếu chưa có
                var baseUrl = $"{Request.Scheme}://{Request.Host}/TimNguoi/ChiTietBaiTimNguoi/{post.Id}";
                post.GenerateQRCode(baseUrl);
                db.SaveChanges();

                if (post.QRCodeImage == null || post.QRCodeImage.Length == 0)
                {
                    return NotFound("Không thể tạo QR code");
                }
            }

            // Trả về file từ byte array
            return File(post.QRCodeImage, "image/png", $"`QRCode bài viết tìm người thất lạc -${id}.png`;");
        }

    }

}
