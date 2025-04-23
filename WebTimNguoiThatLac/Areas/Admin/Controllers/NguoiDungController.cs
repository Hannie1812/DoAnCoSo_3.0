using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using WebTimNguoiThatLac.Areas.Admin.Models;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Moderator},{SD.Role_Admin}")]
    public class NguoiDungController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public  NguoiDungController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
           
        }
       
        public async Task<IActionResult> Index(string TimKiem = "", string Role = "", int Page = 1)
        {
            IEnumerable<ApplicationUser> ds = await db.Users.ToListAsync();
            ViewBag.TimKiem = TimKiem;
            ViewBag.Role = Role;
            int sodongtren1trang = 5;

            // Lọc theo role nếu được chọn
            if (!string.IsNullOrEmpty(Role))
            {
                var usersInRole = await userManager.GetUsersInRoleAsync(Role);
                ds = ds.Where(u => usersInRole.Contains(u));
            }

            if (TimKiem.IsNullOrEmpty())
            {
                var dsTrang = ds.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
            else
            {
                TimKiem = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(TimKiem);
                List<ApplicationUser> dsTimKiem = new List<ApplicationUser>();
                foreach (ApplicationUser i in ds)
                {
                    string r1 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.FullName);
                    if (r1.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    string r2 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.UserName);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    if(i.PhoneNumber != null )
                    {
                        string r3 = i.PhoneNumber;
                        if (r3.ToUpper().Contains(TimKiem.ToUpper()))
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
            //return RedirectToAction("Register", "Account", new { area = "Identity" });
            return RedirectToPage("/Identity/Account/Manage/Email");
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
        public async Task<IActionResult> Update(string id)
        {
            // Kiểm tra quyền của người dùng hiện tại
            var currentUser = await userManager.GetUserAsync(User);
            var currentUserRoles = await userManager.GetRolesAsync(currentUser);
            var isModerator = currentUserRoles.Contains("Moderator");

            ApplicationUser x = await db.Users.FirstOrDefaultAsync(i => i.Id == id);
            if (x == null)
            {
                return RedirectToAction("Index");
            }

            // Kiểm tra nếu là Moderator và người được chỉnh sửa là Admin
            if (isModerator)
            {
                var userRoles = await userManager.GetRolesAsync(x);
                if (userRoles.Contains("Admin"))
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin của Admin";
                    return RedirectToAction("Index");
                }
            }
            List<IdentityRole> ds = await db.Roles.ToListAsync();
            if (isModerator)
            {
                ds = ds.Where(r => r.Name != "Admin").ToList();
            }
            var dsLTT = new SelectList(ds, "Name", "Name");
            ViewBag.DanhSachRole = dsLTT;
            var roles = await userManager.GetRolesAsync(x);
            ViewBag.RoleHienTai = roles.Any() ? roles[0] : "";
            return View(x);
        }
      
        [HttpPost]
        public async Task<IActionResult> Update(ApplicationUser t, string? LoaiTaiKhoan)
        {
            // Kiểm tra quyền của người dùng hiện tại
            var currentUser = await userManager.GetUserAsync(User);
            var currentUserRoles = await userManager.GetRolesAsync(currentUser);
            var isModerator = currentUserRoles.Contains("Moderator");

            if (ModelState.IsValid)
            {
                ApplicationUser x = await db.Users.FirstOrDefaultAsync(i => i.Id == t.Id);
                if (x == null)
                {
                    return RedirectToAction("Index");
                }

                // Kiểm tra nếu là Moderator
                if (isModerator)
                {
                    // Kiểm tra xem người được chỉnh sửa có phải là Admin không
                    var userRoles = await userManager.GetRolesAsync(x);
                    if (userRoles.Contains("Admin"))
                    {
                        TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin của Admin";
                        return RedirectToAction("Index");
                    }

                    // Giới hạn việc nâng cấp quyền
                    if (LoaiTaiKhoan == "Admin")
                    {
                        TempData["Error"] = "Bạn không có quyền nâng cấp tài khoản lên Admin";
                        return RedirectToAction("Index");
                    }
                }

                if (LoaiTaiKhoan != null)
                {
                    var dsrole = await userManager.GetRolesAsync(x);
                    await userManager.RemoveFromRolesAsync(x, dsrole);
                    await userManager.AddToRoleAsync(x, LoaiTaiKhoan);
                }

                x.FullName = t.FullName;
                x.Address = t.Address;
                x.NgaySinh = t.NgaySinh;
                x.CCCD = t.CCCD;
                x.PhoneNumber = t.PhoneNumber;
                x.Active = t.Active;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }


            List<IdentityRole> ds = await db.Roles.ToListAsync();
            var dsLTT = new SelectList(ds, "Id", "Name");
            ViewBag.DanhSachRole = dsLTT;
            var roles = await userManager.GetRolesAsync(t);
            ViewBag.RoleHienTai = roles.Any() ? roles[0] : "";
            return View(t);

        }


        [HttpPost]
        public async Task<IActionResult> HoatDong(string id)
        {

            try
            {
                ApplicationUser y = await db.Users.FirstOrDefaultAsync(i => i.Id == id);
                if (y == null)
                {
                    return Json(new { success = false, message = "Ko Có Id Cần chỉnh sửa" });
                }

                y.Active = !y.Active;

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


        //[HttpPost]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    try
        //    {
        //        ApplicationUser y = await db.Users.FirstOrDefaultAsync(i => i.Id == id);
        //        if (y == null)
        //        {
        //            return Json(new { success = false, message = "Ko Có Id Cần Xóa" });
        //        }

        //        // Xóa các bài tìm người liên quan
        //        List<TimNguoi> ds = await db.TimNguois
        //                                    .Include(u => u.AnhTimNguois)
        //                                    .Where(i => i.IdNguoiDung == id)
        //                                    .ToListAsync();

        //        foreach (TimNguoi i in ds)
        //        {
        //            // Xóa ảnh liên quan đến bài tìm người
        //            var dsHA = await db.AnhTimNguois
        //                               .Where(m => m.IdNguoiCanTim.HasValue && m.IdNguoiCanTim.Value == i.Id)
        //                               .ToListAsync();
        //            db.AnhTimNguois.RemoveRange(dsHA);

        //            // Xóa bình luận của bài viết
        //            var dsBLBV = await db.BinhLuans
        //                               .Where(m => m.IdBaiViet == i.Id)
        //                               .ToListAsync();
        //            db.BinhLuans.RemoveRange(dsBLBV);

        //            // Xóa bình luận của us
        //            var dsBLus = await db.BinhLuans
        //                               .Where(m => m.UserId == y.Id)
        //                               .ToListAsync();
        //            db.BinhLuans.RemoveRange(dsBLus);

        //            // Xóa bài tìm người
        //            db.TimNguois.Remove(i);
        //        }

        //        // Xóa vai trò của user
        //        var usrole = await userManager.GetRolesAsync(y);
        //        if (usrole != null && usrole.Any())
        //        {
        //            await userManager.RemoveFromRolesAsync(y, usrole);
        //        }

        //        // Xóa user
        //        db.Users.Remove(y);

        //        // Lưu thay đổi chỉ một lần
        //        await db.SaveChangesAsync();

        //        return Json(new { success = true, message = "Xóa thành công" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi: " + ex.InnerException?.Message ?? ex.Message });
        //    }

        //}


    }
}