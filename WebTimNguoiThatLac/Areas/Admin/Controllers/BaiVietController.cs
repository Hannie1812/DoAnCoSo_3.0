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

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]// chỉ cho phép admin
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
        private static readonly IEnumerable<string> TinhThanhIEnumerable = new List<string>
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
    };

        public async Task<IActionResult> Index(string TimKiem = "", int Page = 1)
        {
            IEnumerable<TimNguoi> ds = await db.TimNguois
                                                            .Include(u => u.ApplicationUser)                                            
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

        public async Task<IActionResult> Create()
        {
            IEnumerable<string> DanhSachTinhThanh = TinhThanhIEnumerable;
            ViewBag.DanhSachTinhThanh = DanhSachTinhThanh;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TimNguoi x, List<IFormFile>? DanhSachHinhAnh, string EmailNguoiDung)
        {
            IEnumerable<string> DanhSachTinhThanh = TinhThanhIEnumerable;
            ViewBag.DanhSachTinhThanh = DanhSachTinhThanh;

            if (ModelState.IsValid)
            {
                ApplicationUser us = await userManager.FindByEmailAsync(EmailNguoiDung);
                if(us == null)
                {
                    ModelState.AddModelError("", "Lỗi email chưa đăng kí tài khoản ");
                    return View();
                }


                x.IdNguoiDung = us.Id;
                x.NgayDang = DateTime.Now;
                x.active = false;

                db.Add(x);
                await db.SaveChangesAsync();

                if (DanhSachHinhAnh != null)
                {
                    int d = 0;
                    foreach(IFormFile i in DanhSachHinhAnh)
                    {

                        AnhTimNguoi z = new AnhTimNguoi(); 
                        z.HinhAnh  = await SaveImage(i, "AnhNguoiCanTim");
                        if(d==0)
                        {
                            z.TrangThai = 1;
                            d++;
                        }
                        else
                        {
                            z.TrangThai = 0;
                            d++;
                        }
                        z.IdNguoiCanTim = x.Id;

                        db.AnhTimNguois.Add(z);
                        
                    }
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View();
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

            IEnumerable<string> DanhSachTinhThanh = TinhThanhIEnumerable;
            ViewBag.DanhSachTinhThanh = DanhSachTinhThanh;

            return View(x);
        }

        [HttpPost]
        public async Task<IActionResult> Update(TimNguoi t, List<IFormFile>? DSHinhAnhCapNhat)
        {
            if (ModelState.IsValid)
            {
                TimNguoi x = await db.TimNguois.FirstOrDefaultAsync(i => i.Id == t.Id);
                if (x == null)
                {
                    return RedirectToAction("Index");
                }


                x.HoTen = t.HoTen;
                x.TieuDe = t.TieuDe;
                x.MoTa = t.MoTa;
                x.DaciemNhanDang = t.DaciemNhanDang;
                x.GioiTinh = t.GioiTinh;
                x.TrangThai = t.TrangThai;
                x.KhuVuc = t.KhuVuc;
                x.active = t.active;

                await db.SaveChangesAsync();

                if(DSHinhAnhCapNhat != null)
                {
                    List<AnhTimNguoi> dsha = await db.AnhTimNguois
                                            .Where(i => i.IdNguoiCanTim == t.Id)
                                            .ToListAsync();
                    foreach(AnhTimNguoi i in dsha)
                    {
                        db.AnhTimNguois.Remove(i);
                        DeleteImage(i.HinhAnh, "AnhNguoiCanTim");
                    }
                    int d = 0;
                    foreach(IFormFile i in DSHinhAnhCapNhat)
                    {
                        AnhTimNguoi a = new AnhTimNguoi();
                        a.HinhAnh = await SaveImage(i, "AnhNguoiCanTim");
                        if(d==0)
                        {
                            a.TrangThai = 1;
                        }
                        else
                        {
                            a.TrangThai = 0;
                        }
                        a.IdNguoiCanTim = x.Id;
                        await db.SaveChangesAsync();
                    }
                }


                return RedirectToAction("Index");
            }


            IEnumerable<string> DanhSachTinhThanh = TinhThanhIEnumerable;
            ViewBag.DanhSachTinhThanh = DanhSachTinhThanh;

            return View(t);

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
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                ApplicationUser y = await db.Users.FirstOrDefaultAsync(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Ko Có Id Cần Xóa" });
                }


                List<TimNguoi> ds = await db.TimNguois
                                                        .Include(u => u.ApplicationUser)
                                                        .Include(u => u.AnhTimNguois)
                                                        .Where(i => i.IdNguoiDung == id)
                                                        .ToListAsync();
                foreach (TimNguoi i in ds)
                {
                    // Xóa ảnh tìm người
                    List<AnhTimNguoi> dsHA = await db.AnhTimNguois.Where(m => m.IdNguoiCanTim == i.Id).ToListAsync();
                    foreach (AnhTimNguoi j in dsHA)
                    {
                        DeleteImage(j.HinhAnh, "AnhNguoiCanTim");
                        db.AnhTimNguois.Remove(j);
                        //await db.SaveChangesAsync();
                    }

                    // Xóa Bình luôanj trong bài viết
                    List<BinhLuan> dsBL = await db.BinhLuans.Where(m => m.IdBaiViet == i.Id).ToListAsync();
                    foreach (BinhLuan z in dsBL)
                    {
                        if (z.HinhAnh != null)
                        {
                            DeleteImage(z.HinhAnh, "BinhLuan");
                        }
                        db.BinhLuans.Remove(z);
                        //await db.SaveChangesAsync();
                    }

                    db.TimNguois.Remove(i);
                    //await db.SaveChangesAsync();
                }

                // Xóa toàm bộ bình luận người dùng
                List<BinhLuan> BLUS = await db.BinhLuans
                                                     .Include(u => u.ApplicationUser)
                                                     .Where(i => i.UserId == y.Id)
                                                     .ToListAsync();

                foreach (BinhLuan j in BLUS)
                {
                    if (j.HinhAnh != null)
                    {
                        DeleteImage(j.HinhAnh, "BinhLuan");
                    }
                    db.BinhLuans.Remove(j);
                }



                var usrole = await userManager.GetRolesAsync(y);

                if (usrole != null)
                {
                    await userManager.RemoveFromRolesAsync(y, usrole);
                }

                db.Users.Remove(y);


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
