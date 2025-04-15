using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class TrungBayController : Controller
    {
        private ApplicationDbContext db;
        public TrungBayController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IActionResult> Index(string TimKiem = "", int Page = 1)
        {
            ViewBag.TimKiem = TimKiem;
            IEnumerable<TrungBayHinhAnh> ds = await db.TrungBayHinhAnhs.ToListAsync();

            int sodongtren1trang = 5;
            if (TimKiem.IsNullOrEmpty())
            {
                var dsTrang = ds.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
            else
            {
                TimKiem = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(TimKiem);
                List<TrungBayHinhAnh> dsTimKiem = new List<TrungBayHinhAnh>();
                foreach (TrungBayHinhAnh i in ds)
                {
                    if ((i.Id+"").Contains(TimKiem))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    
                }
                var dsTrang = dsTimKiem.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(TrungBayHinhAnh t, IFormFile? HinhAnhCapNhat)
        {
            if (ModelState.IsValid)
            {

                if (HinhAnhCapNhat != null)
                {
                    t.HinhAnh = await SaveImage(HinhAnhCapNhat, "TrungBayHinhAnh");
                    db.TrungBayHinhAnhs.Add(t);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError("", "Vui lòng chọn hình ảnh.");
            }
            return View(t);
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
            TinTuc x = await db.TinTucs.FindAsync(id);
            if (x == null)
            {
                return RedirectToAction("Index");
            }
            return View(x);
        }

        [HttpPost]
        public async Task<IActionResult> Update(TrungBayHinhAnh t, IFormFile? HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                TrungBayHinhAnh x = await db.TrungBayHinhAnhs.FindAsync(t.Id);
                if (x == null)
                {
                    return RedirectToAction("Index");
                }

                if (HinhAnhFile != null)
                {
                   
                    DeleteImage(x.HinhAnh, "TrungBayHinhAnh");

                    x.HinhAnh = await SaveImage(HinhAnhFile, "TrungBayHinhAnh");

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                x.Active = t.Active;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(t);

        }


        [HttpPost]
        public IActionResult HoatDong(int id)
        {

            try
            {
                TrungBayHinhAnh y = db.TrungBayHinhAnhs.FirstOrDefault(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Ko Có Id Cần Xóa" });
                }

                y.Active = !y.Active;

                db.SaveChanges();
                return Json(new { success = true, message = "Thành Công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                TrungBayHinhAnh y = db.TrungBayHinhAnhs.FirstOrDefault(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Ko Có Id Cần Xóa" });
                }
                DeleteImage(y.HinhAnh, "TrungBayHinhAnh");
                db.TrungBayHinhAnhs.Remove(y);

                db.SaveChanges();

                return Json(new { success = true, message = "Thành Công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
