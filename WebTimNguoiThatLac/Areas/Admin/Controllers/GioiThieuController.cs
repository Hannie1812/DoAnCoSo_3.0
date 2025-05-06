using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTimNguoiThatLac.Areas.Admin.Models;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Moderator},{SD.Role_Admin}")]
    public class GioiThieuController : Controller
    {
        private ApplicationDbContext db;


        public GioiThieuController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IActionResult> Index(string TimKiem = "", int page = 1)
        {

            IEnumerable<GioiThieu> ds = await db.GioiThieus.ToListAsync();
            ViewBag.TimKiem = TimKiem;
            int sodongtren1trang = 5;

            if (TimKiem.IsNullOrEmpty())
            {
                var dsTrang = ds.ToPagedList(page, sodongtren1trang);
                return View(dsTrang);
            }
            else
            {
                TimKiem = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(TimKiem);
                List<GioiThieu> dsTimKiem = new List<GioiThieu>();
                foreach (GioiThieu i in ds)
                {
                    string r1 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.TieuDe);
                    if (r1.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    string r2 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.NoiDung);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }


                }
                var dsTrang = dsTimKiem.ToPagedList(page, sodongtren1trang);
                return View(dsTrang);
            }
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
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(GioiThieu t, IFormFile? HinhAnhCapNhat)
        {
            if (ModelState.IsValid)
            {
                if (HinhAnhCapNhat == null)
                {
                    ModelState.AddModelError("", "Vui lòng chọn hình ảnh");
                    return View(t);
                }
                t.HinhAnh = await SaveImage(HinhAnhCapNhat, "GioiThieu");

                db.GioiThieus.Add(t);

                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm thành công Giới Thiệu!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Vui lòng nhập đủ thông tin!";
            return View(t);
        }

        public async Task<IActionResult> Update(int id)
        {
            var t = await db.GioiThieus.FindAsync(id);
            if (t == null)
            {
                return NotFound();
            }
            return View(t);
        }

        [HttpPost]
        public async Task<IActionResult> Update(GioiThieu t, IFormFile? HinhAnhCapNhat)
        {
            if (ModelState.IsValid)
            {
                if (HinhAnhCapNhat != null)
                {
                    DeleteImage(t.HinhAnh, "GioiThieu");
                    t.HinhAnh = await SaveImage(HinhAnhCapNhat, "GioiThieu");
                }
                db.GioiThieus.Update(t);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập Nhật thành công Giới Thiệu!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Vui lòng nhập đủ thông tin!";
            return View(t);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                GioiThieu y = db.GioiThieus.FirstOrDefault(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Ko Có Id Cần Xóa" });
                }
                DeleteImage(y.HinhAnh, "GioiThieu");

                db.GioiThieus.Remove(y);

                db.SaveChanges();
                TempData["SuccessMessage"] = "Thành Công";
                return Json(new { success = true, message = "Thành Công" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Thất Bại";
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult HoatDong(int id)
        {
            try
            {
                GioiThieu y = db.GioiThieus.FirstOrDefault(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bản ghi" });
                }

                y.Active = !y.Active; 

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = y.Active ? "Đã kích hoạt thành công" : "Đã ngừng kích hoạt thành công",
                    newStatus = y.Active
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
