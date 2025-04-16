using Microsoft.AspNetCore.Authorization;
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
    public class TinTucController : Controller
    {
        private ApplicationDbContext db;
        public TinTucController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index(string TimKiem = "", int Page = 1)
        {
            ViewBag.TimKiem = TimKiem;
            IEnumerable<TinTuc> ds = await db.TinTucs.ToListAsync();

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
                    string r3 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.MoTaNgan);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
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
        public async Task<IActionResult> Create(TinTuc t, IFormFile? HinhAnhCapNhat)
        {
            if (ModelState.IsValid)
            {

                if (HinhAnhCapNhat != null)
                {

                    t.HinhAnh = await SaveImage(HinhAnhCapNhat, "TinTuc");
                }
                db.TinTucs.Add(t);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");

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
        public async Task<IActionResult> Update(TinTuc t, IFormFile? HinhAnhCapNhat)
        {
            if (ModelState.IsValid)
            {
                TinTuc x = await db.TinTucs.FindAsync(t.Id);
                if (x == null)
                {
                    return RedirectToAction("Index");
                }

                if (HinhAnhCapNhat != null)
                {
                    x.TieuDe = t.TieuDe;
                    x.NoiDung = t.NoiDung;
                    x.TacGia = t.TacGia;
                    x.Active = t.Active;
                    x.MoTaNgan = t.MoTaNgan;

                    DeleteImage(x.HinhAnh, "TinTuc");

                    x.HinhAnh = await SaveImage(HinhAnhCapNhat, "TinTuc");

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                x.TieuDe = t.TieuDe;
                x.NoiDung = t.NoiDung;
                x.TacGia = t.TacGia;
                x.Active = t.Active;
                x.MoTaNgan = t.MoTaNgan;
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
                TinTuc y = db.TinTucs.FirstOrDefault(i => i.Id == id);
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
                TinTuc y = db.TinTucs.FirstOrDefault(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Ko Có Id Cần Xóa" });
                }
                DeleteImage(y.HinhAnh, "TinTuc");
                db.TinTucs.Remove(y);

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
