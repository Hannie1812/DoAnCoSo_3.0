using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTimNguoiThatLac.Areas.Admin.Models;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace PhongKhamThuCung.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class NguoiDungLienHeController : Controller
    {
        private ApplicationDbContext db;

        public NguoiDungLienHeController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IActionResult> Index(string TimKiem = "", string trangThai = "all", int page = 1)
        {
            ViewBag.TimKiem = TimKiem;
            ViewBag.TrangThai = trangThai;

            IQueryable<NguoiDungLienHe> query = db.NguoiDungLienHes;

            // Lọc theo trạng thái
            switch (trangThai)
            {
                case "read":
                    query = query.Where(x => x.isRead);
                    break;
                case "unread":
                    query = query.Where(x => !x.isRead);
                    break;
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(TimKiem))
            {
                query = query.Where(x =>
                    x.TenNguoiDungLienHe.Contains(TimKiem) ||
                    x.VanDeLienHe.Contains(TimKiem) ||
                    x.NgayLienHe.ToString().Contains(TimKiem) ||
                    x.PhoneNguoiDungLienHe.Contains(TimKiem) ||
                    x.EmailNguoiDungLienHe.Contains(TimKiem)
                );
            }

            // Phân trang
            int pageSize = 10;
            var model =  query.OrderByDescending(x => x.NgayLienHe)
                                  .ToPagedList(page, pageSize);

            // Truyền dữ liệu vào ViewBag để sử dụng trong view
            ViewBag.STT = (page - 1) * pageSize + 1;

            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            NguoiDungLienHe x = await db.NguoiDungLienHes.FirstOrDefaultAsync(i => i.MaLienHeNguoiDung == id);
            if (x == null)
            {
                return RedirectToAction("Index");
            }
            x.isRead = true;
            await db.SaveChangesAsync();
            return View(x);
            
        }

        

        // Action POST cho chức năng Xóa
        [HttpPost, ActionName("XoaLienHe")]
        [ValidateAntiForgeryToken]
        public IActionResult XoaLienHe(int id)
        {
            var item = db.NguoiDungLienHes.Find(id);
            if (item == null)
            {
                return NotFound();
            }

            db.NguoiDungLienHes.Remove(item);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // 1. Tìm dịch vụ cần xóa và bao gồm cả lịch hẹn liên quan
                var dichVu = await db.NguoiDungLienHes
                    .FirstOrDefaultAsync(d => d.MaLienHeNguoiDung == id);

                if (dichVu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ cần xóa" });
                }

                // 4. Xóa dịch vụ
                db.NguoiDungLienHes.Remove(dichVu);

                // 5. Lưu thay đổi và commit transaction
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Xóa dịch vụ và tất cả lịch hẹn liên quan thành công"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi xóa dịch vụ",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = db.NguoiDungLienHes.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return View("Detail", item); // Sử dụng lại view Details với modal
        }

        // Action POST cho chức năng Sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NguoiDungLienHe model)
        {
            if (ModelState.IsValid)
            {
                db.NguoiDungLienHes.Update(model);
                db.SaveChanges();
                return RedirectToAction("Detail", new { id = model.MaLienHeNguoiDung });
            }

            // Nếu có lỗi validation, trả về view với model để hiển thị lỗi
            return View("Detail", model);
        }

        // Action GET cho chức năng Xóa (hiển thị confirm)
       

        
    }
}
