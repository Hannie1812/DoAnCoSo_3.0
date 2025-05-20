using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTimNguoiThatLac.Areas.Admin.Models;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Services;
using X.PagedList.Extensions;
using Microsoft.Build.Framework;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Moderator},{SD.Role_Admin}")]
    public class BaiVietController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private readonly ILogger<BaiVietController> _logger;

        public BaiVietController(ApplicationDbContext db,
                               UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               ILogger<BaiVietController> logger)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._logger = logger;
        }
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

        public async Task<IActionResult> Index(string TimKiem = "", int Page = 1)
        {
            IEnumerable<TimNguoi> ds = await db.TimNguois
                                                            .Include(u => u.ApplicationUser)
                                                            .Include(u => u.TinhThanh)
                                                            .Include(u => u.QuanHuyen)
                                                            .OrderByDescending(m => m.NgayDang)
                                                            .Include(u => u.TinhThanh)
                                                            .Include(u => u.QuanHuyen)
                                                            .ToListAsync();
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
                List<TimNguoi> dsTimKiem = new List<TimNguoi>();
                foreach (TimNguoi i in ds)
                {
                    string r1 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.HoTen);
                    if (r1.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    string r2 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.TieuDe);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    if (i.DaciemNhanDang != null)
                    {
                        string r3 = i.DaciemNhanDang;
                        if (r3.ToUpper().Contains(TimKiem.ToUpper()))
                        {
                            dsTimKiem.Add(i);
                            continue;
                        }
                    }
                    if (i.KhuVuc != null)
                    {
                        string r4 = i.KhuVuc;
                        if (r4.ToUpper().Contains(TimKiem.ToUpper()))
                        {
                            dsTimKiem.Add(i);
                            continue;
                        }
                    }

                }
                var dsTrang = dsTimKiem.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
        }

        private async Task LoadSelectListsAsync()
        {
            var tinhThanhs = await db.TinhThanhs.ToListAsync();
            var quanHuyens = await db.QuanHuyens.ToListAsync();
            ViewBag.DanhSachTinhThanh = new SelectList(tinhThanhs, "Id", "TenTinhThanh");
            ViewBag.DanhSachQuanHuyen = new SelectList(quanHuyens, "Id", "TenQuanHuyen");
        }
        public async Task<IActionResult> Create()
        {
            await LoadSelectListsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TimNguoi x, 
                                                List<IFormFile>? DSHinhAnhCapNhat, 
                                                string EmailNguoiDung,
                                                string faceDescriptorsJson)
        {
            await LoadSelectListsAsync();

            if (ModelState.IsValid)
            {
                ApplicationUser us = await userManager.FindByEmailAsync(EmailNguoiDung);
                if (us == null)
                {
                    ModelState.AddModelError("", "Lỗi email chưa đăng kí tài khoản ");
                    return View(x);
                }


                x.IdNguoiDung = us.Id;
                x.NgayDang = DateTime.Now;
                x.active = false;
                x.NguoiDangBaiXoa = false;



                if (DSHinhAnhCapNhat == null || DSHinhAnhCapNhat.Count == 0)
                {
                    ModelState.AddModelError("Lỗi", "Chưa Có Hình Ảnh");
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
                            x.AverageDescriptor = descriptorData.AverageDescriptor;
                            _logger.LogInformation("Đã gán AverageDescriptor cho bài viết ID {Id}", x.Id);
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

                db.TimNguois.Add(x);
                await db.SaveChangesAsync();

                int primaryImageIndex = 0;
                for (int i = 0; i < DSHinhAnhCapNhat.Count; i++)
                {
                    var file = DSHinhAnhCapNhat[i];
                    var anhTimNguoi = new AnhTimNguoi
                    {
                        IdNguoiCanTim = x.Id,
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
                ModelState.AddModelError("", "Vui Lòng Nhập Đủ Thông Tin ");
                return RedirectToAction("Index");
            }

            return View();
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
                return; // Không làm gì nếu không có ảnh
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
        public async Task<IActionResult> Update(int id)
        {
            TimNguoi x = await db.TimNguois
                                             .Include(u => u.ApplicationUser)
                                             .Include(u => u.BinhLuans)
                                             .FirstOrDefaultAsync(i => i.Id == id);
            if (x == null)
            {
                return RedirectToAction("Index");
            }
            await LoadSelectListsAsync();
            ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == id).ToList();

            return View(x);
        }
        [HttpPost]
        public async Task<IActionResult> Update(TimNguoi x, List<IFormFile>? DSHinhAnhCapNhat, string? faceDescriptorsJson)
        {
            try
            {
                // Luôn thiết lập ViewBag trước khi trả về View
                IEnumerable<TinhThanh> tinhThanhs = await db.TinhThanhs.ToListAsync();
                IEnumerable<QuanHuyen> quanHuyens = await db.QuanHuyens.ToListAsync();


                ViewBag.DanhSachTinhThanh = tinhThanhs;
                ViewBag.DanhSachQuanHuyen = quanHuyens;

                ViewBag.SelectedTinhThanh = x.IdTinhThanh;
                ViewBag.SelectedQuanHuyen = x.IdQuanHuyen;
                string tt = db.TinhThanhs.FirstOrDefault(i => i.Id == x.IdTinhThanh).TenTinhThanh ?? "";
                ViewBag.SelectedNameTinhThanh = tt;

                if (ModelState.IsValid)
                {
                    TimNguoi y = await db.TimNguois.FirstOrDefaultAsync(i => i.Id == x.Id);
                    if (y == null)
                    {
                        TempData["ErrorMessage"] = " Lỗi Khi Thay Đổi";
                        return RedirectToAction("Index");
                    }

                    // Luôn thiết lập ViewBag trước khi trả về View
                    await LoadSelectListsAsync();
                    ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == x.Id).ToList();

                    if (!ModelState.IsValid)
                    {

                        ViewData["ErrorMessage"] = " Lỗi Khi Thay Đổi";
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
                    y.IdTinhThanh = x.IdTinhThanh;
                    y.IdQuanHuyen = x.IdQuanHuyen;

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
                                await transaction.CommitAsync(); // QUAN TRỌNG: Phải commit transaction


                            }
                            catch
                            {
                                transaction.Rollback();
                                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật hình ảnh");
                                ViewData["ErrorMessage"] = " Lỗi Khi Thay Đổi";
                                return View(x);
                            }
                        }
                    }

                    // Lưu các thay đổi khác (luôn thực hiện)
                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = " Lưu Thay Đỗi Bài Viết Tành Công";
                    return RedirectToAction("Index");
                }
                await LoadSelectListsAsync();
                ViewBag.DanhSachHinhAnh = db.AnhTimNguois.Where(i => i.IdNguoiCanTim == x.Id).ToList();
                ViewData["ErrorMessage"] = " Nhập Đầy Đủ Thông Tin";
                return View(x);
            }
            catch (Exception ex)
            {
                return View(x);
            }


        }


        [HttpPost]
        public async Task<IActionResult> HoatDong(int id)
        {

            try
            {
                TimNguoi y = await db.TimNguois.FirstOrDefaultAsync(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Ko Có Id Cần chỉnh sửa" });
                }
                if (y.active == false)
                {
                    y.NguoiDangBaiXoa = false;
                }
                y.active = !y.active;

                await db.SaveChangesAsync();
                return Json(new { success = true, message = "Thành Công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                TimNguoi tn = db.TimNguois
                    .Include(u => u.ApplicationUser)
                    .Include(u => u.BaoCaoBaiViets)
                    .Include(u => u.AnhTimNguois)
                    .Include(u => u.NhanChungs)
                    .Include(u => u.BinhLuans)
                        .ThenInclude(v => v.ApplicationUser)
                        .ThenInclude(v => v.BaoCaoBinhLuans)
                    .Include(u => u.BaoCaoBaiViets)
                        .ThenInclude(v => v.ApplicationUser)
                    .FirstOrDefault(i => i.Id == id);


                // bình luận
                List<BinhLuan> dsBinhLuan = db.BinhLuans
                    .Include(u => u.BaoCaoBinhLuans)
                    .Where(i => i.IdBaiViet == tn.Id).ToList();
                // xóa các báo cáo bình luận trong dsBinhLuan
                foreach (BinhLuan i in dsBinhLuan)
                {
                    List<BaoCaoBinhLuan> bc = db.BaoCaoBinhLuans.Where(m => m.MaBinhLuan == i.Id).ToList();
                    if (bc.Count() > 0)
                    {
                        db.BaoCaoBinhLuans.RemoveRange(bc);
                    }
                    DeleteImage(i.HinhAnh, "BinhLuan");
                }


                db.BinhLuans.RemoveRange(dsBinhLuan);

                // tìm thấy thất lạc
                List<TimThayNguoiThatLac> dsThayThayLac = db.TimThayNguoiThatLacs
                    .Include(u => u.ApplicationUser)
                    .Where(i => i.TimNguoiId == tn.Id).ToList();

                if (dsThayThayLac.Count() > 0)
                {
                    db.TimThayNguoiThatLacs.RemoveRange(dsThayThayLac);
                }

                // ảnh tìm người
                List<AnhTimNguoi> dsAnhTimNguoi = db.AnhTimNguois
                   .Where(i => i.IdNguoiCanTim == tn.Id).ToList();

                foreach (AnhTimNguoi i in dsAnhTimNguoi)
                {
                    DeleteImage(i.HinhAnh, "AnhNguoiCanTim");
                    db.AnhTimNguois.Remove(i);
                }


                // báo cáo bài viết

                List<BaoCaoBaiViet> dsBaoCaoBaiViet = db.BaoCaoBaiViets
                    .Include(u => u.ApplicationUser)
                    .Where(i => i.MaBaiViet == tn.Id).ToList();


                foreach (BaoCaoBaiViet i in dsBaoCaoBaiViet)
                {
                    DeleteImage(i.HinhAnh, "AnhMinhTrungBaoCaoBaiViet");
                    db.BaoCaoBaiViets.Remove(i);
                }

                db.TimNguois.Remove(tn);

                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Thành Công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ExportToWord(int id)
        {
            var timNguoi = await db.TimNguois
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (timNguoi == null)
            {
                return NotFound();
            }

            try
            {
                var wordService = new WordExportService();
                byte[] fileContents = wordService.CreateOfficialReport(timNguoi, timNguoi.ApplicationUser);

                // Tạo tên file duy nhất
                var fileName = $"DonTrinhBao_{timNguoi.Id}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                // Đường dẫn thư mục lưu trữ (tương đối)
                var relativeFolderPath = Path.Combine("DangKiTimKiem", fileName);

                // Đường dẫn vật lý đầy đủ để lưu file
                var fullFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DangKiTimKiem");
                var fullFilePath = Path.Combine(fullFolderPath, fileName);

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }

                // Lưu file vào thư mục
                await System.IO.File.WriteAllBytesAsync(fullFilePath, fileContents);

                // Cập nhật đường dẫn file vào trường DonDangKiTrinhBao
                timNguoi.DonDangKiTrinhBao = $"/DangKiTimKiem/{fileName}";
                db.TimNguois.Update(timNguoi);
                await db.SaveChangesAsync();

                // Trả về file cho người dùng
                return File(fileContents,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất file Word");
                return StatusCode(500, "Lỗi khi xuất file: " + ex.Message);
            }
        }

    }
}
