using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;

        public NguoiDungController(IWebHostEnvironment env, ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _env = env;

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
                    if (i.PhoneNumber != null)
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
        //public async Task<IActionResult> Update(string id)
        //{
        //    // Kiểm tra quyền của người dùng hiện tại
        //    var currentUser = await userManager.GetUserAsync(User);
        //    var currentUserRoles = await userManager.GetRolesAsync(currentUser);
        //    var isModerator = currentUserRoles.Contains("Moderator");

        //    ApplicationUser x = await db.Users.FirstOrDefaultAsync(i => i.Id == id);
        //    if (x == null)
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    // Kiểm tra nếu là Moderator và người được chỉnh sửa là Admin
        //    if (isModerator)
        //    {
        //        var userRoles = await userManager.GetRolesAsync(x);
        //        if (userRoles.Contains("Admin"))
        //        {
        //            TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin của Admin";
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    List<IdentityRole> ds = await db.Roles.ToListAsync();
        //    if (isModerator)
        //    {
        //        ds = ds.Where(r => r.Name != "Admin").ToList();
        //    }
        //    var dsLTT = new SelectList(ds, "Name", "Name");
        //    ViewBag.DanhSachRole = dsLTT;
        //    var roles = await userManager.GetRolesAsync(x);
        //    ViewBag.RoleHienTai = roles.Any() ? roles[0] : "";
        //    return View(x);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Update(ApplicationUser t, string? LoaiTaiKhoan)
        //{
        //    // Kiểm tra quyền của người dùng hiện tại
        //    var currentUser = await userManager.GetUserAsync(User);
        //    var currentUserRoles = await userManager.GetRolesAsync(currentUser);
        //    var isModerator = currentUserRoles.Contains("Moderator");

        //    if (ModelState.IsValid)
        //    {
        //        ApplicationUser x = await db.Users.FirstOrDefaultAsync(i => i.Id == t.Id);
        //        if (x == null)
        //        {
        //            return RedirectToAction("Index");
        //        }

        //        // Kiểm tra nếu là Moderator
        //        if (isModerator)
        //        {
        //            // Kiểm tra xem người được chỉnh sửa có phải là Admin không
        //            var userRoles = await userManager.GetRolesAsync(x);
        //            if (userRoles.Contains("Admin"))
        //            {
        //                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông tin của Admin";
        //                return RedirectToAction("Index");
        //            }

        //            // Giới hạn việc nâng cấp quyền
        //            if (LoaiTaiKhoan == "Admin")
        //            {
        //                TempData["Error"] = "Bạn không có quyền nâng cấp tài khoản lên Admin";
        //                return RedirectToAction("Index");
        //            }
        //        }

        //        if (LoaiTaiKhoan != null)
        //        {
        //            var dsrole = await userManager.GetRolesAsync(x);
        //            await userManager.RemoveFromRolesAsync(x, dsrole);
        //            await userManager.AddToRoleAsync(x, LoaiTaiKhoan);
        //        }

        //        x.FullName = t.FullName;
        //        x.Address = t.Address;
        //        x.NgaySinh = t.NgaySinh;
        //        x.CCCD = t.CCCD;
        //        x.PhoneNumber = t.PhoneNumber;
        //        x.Active = t.Active;

        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }


        //    List<IdentityRole> ds = await db.Roles.ToListAsync();
        //    var dsLTT = new SelectList(ds, "Id", "Name");
        //    ViewBag.DanhSachRole = dsLTT;
        //    var roles = await userManager.GetRolesAsync(t);
        //    ViewBag.RoleHienTai = roles.Any() ? roles[0] : "";
        //    return View(t);

        //}
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get current roles
            var currentRoles = await userManager.GetRolesAsync(user);

            // Prepare roles for dropdown
            var allRoles = await roleManager.Roles.ToListAsync();
            ViewBag.DanhSachRole = new SelectList(allRoles, "Name", "Name");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ApplicationUser model, IFormFile? HinhAnhCapNhat, string? LoaiTaiKhoan)
        {
            if (!ModelState.IsValid)
            {
                var allRoles = await roleManager.Roles.ToListAsync();
                ViewBag.DanhSachRole = new SelectList(allRoles, "Name", "Name");
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập";
                return View(model);
            }

            try
            {
                var user = await userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update basic info
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.CCCD = model.CCCD;
                user.NgaySinh = model.NgaySinh;
                user.Address = model.Address;
                user.Active = model.Active;
                if (user.Active == true)
                {

                    user.SoLanViPham = 0;
                }


                // Handle image upload
                if (HinhAnhCapNhat != null && HinhAnhCapNhat.Length > 0)
                {
                    // Validate image
                    if (HinhAnhCapNhat.Length > 5 * 1024 * 1024) // 5MB
                    {
                        TempData["ErrorMessage"] = "Kích thước ảnh tối đa là 5MB";
                        return RedirectToAction("Update", new { id = model.Id });
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(HinhAnhCapNhat.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh JPG, JPEG hoặc PNG";
                        return RedirectToAction("Update", new { id = model.Id });
                    }

                    // Save new image
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhCapNhat.CopyToAsync(fileStream);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(user.HinhAnh))
                    {
                        var oldImagePath = Path.Combine(_env.WebRootPath, user.HinhAnh.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    user.HinhAnh = $"/uploads/avatars/{uniqueFileName}";
                }

                // Update user
                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // Handle role change
                if (!string.IsNullOrEmpty(LoaiTaiKhoan))
                {
                    var currentRoles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    await userManager.AddToRoleAsync(user, LoaiTaiKhoan);
                }

                TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
                return RedirectToAction("Update", new { id = model.Id });
            }
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

                string s = "";
                if (y.Active == false)
                {
                    s = "Tài khoản đã được mở";
                    y.SoLanViPham = 0;
                }
                else
                {
                    s = "Tài khoản đã khóa thành công";
                }
                y.Active = !y.Active;

                await db.SaveChangesAsync();
                return Json(new { success = true, message = s });
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

                ApplicationUser hientai = await userManager.GetUserAsync(User);
                if (hientai == null || y.Id == hientai.Id)
                {
                    return Json(new { success = false, message = "Ko xóa Bản Thân Mình" });
                }

                // Xóa các bài tìm người liên quan
                List<TimNguoi> ds = await db.TimNguois
                                            .Include(u => u.AnhTimNguois)
                                            .Include(u => u.ApplicationUser)
                                            .Include(u => u.BaoCaoBaiViets)
                                                .ThenInclude(v => v.ApplicationUser)
                                            .Include(u => u.NhanChungs)
                                            .Include(u => u.BinhLuans)
                                                .ThenInclude(v => v.ApplicationUser)
                                            .Include(u => u.TimThayNguoiThatLacs)
                                            .Where(i => i.IdNguoiDung == id)
                                            .ToListAsync();


                // Xóa Báo Cáo Bình Luận
                var dsBCBLUS = await db.BaoCaoBinhLuans
                                 .Where(m => m.MaNguoiBaoCao == y.Id)
                                 .ToListAsync();
                if (dsBCBLUS != null && dsBCBLUS.Any())
                {
                    db.BaoCaoBinhLuans.RemoveRange(dsBCBLUS);
                }


                Console.WriteLine("Xóa Báo Cáo Bình Luận");

                // xóa người dùng báo cáo bài viết
                var dsBCBVUS = await db.BaoCaoBaiViets
                                 .Where(m => m.MaNguoiBaoCao == y.Id)
                                 .ToListAsync();
                foreach (var d in dsBCBVUS)
                {
                    DeleteImage(d.HinhAnh, "AnhMinhTrungBaoCaoBaiViet");
                }

                if (dsBCBVUS != null && dsBCBVUS.Any())
                {
                    db.BaoCaoBaiViets.RemoveRange(dsBCBVUS);
                }
                Console.WriteLine("xóa người dùng báo cáo bài viết");


                //// Xóa bình luận của us
                var dsBLus = await db.BinhLuans
                                   .Include(v => v.ParentBinhLuan)
                                   .Where(m => m.UserId == y.Id)
                                   .ToListAsync();
                foreach (var d in dsBLus)
                {
                    await DeleteCommentAndReplies(d);
                }
                Console.WriteLine("Xóa bình luận của us");


                // Xóa Tìm Thấy Thất Lạc
                var dsTimThayUS = await db.TimThayNguoiThatLacs
                                  .Where(m => m.IdNguoiLamDon == y.Id)
                                  .ToListAsync();
                foreach (var d in dsTimThayUS)
                {
                    DeleteImage(d.AnhMinhChung, "AnhMinhChungTimThay");
                }
                if (dsTimThayUS != null && dsTimThayUS.Any())
                {
                    db.TimThayNguoiThatLacs.RemoveRange(dsTimThayUS);
                }
                Console.WriteLine("Xóa Tìm Thấy Thất Lạc");


                foreach (TimNguoi i in ds)
                {
                    // Xóa ảnh liên quan đến bài tìm người
                    var dsHA = await db.AnhTimNguois
                                       .Where(m => m.IdNguoiCanTim == i.Id)
                                       .ToListAsync();
                    foreach (var d in dsHA)
                    {
                        DeleteImage(d.HinhAnh, "AnhNguoiCanTim");
                    }
                    if (dsHA != null && dsHA.Any())
                    {
                        db.AnhTimNguois.RemoveRange(dsHA);
                    }
                    Console.WriteLine("Xóa ảnh liên quan đến bài tìm người : id -> " + i.Id);

                    // Xóa bình luận của bài viết
                    var dsBLBV = await db.BinhLuans
                                       .Where(m => m.IdBaiViet == i.Id)
                                       .ToListAsync();
                    foreach (var d in dsBLBV)
                    {
                        await DeleteCommentAndReplies(d);
                    }
                    Console.WriteLine("Xóa bình luận của bài viết : id -> " + i.Id);


                    // Xóa Nhân Chứng
                    var dsNhanChung = await db.NhanChungs
                                      .Where(m => m.TimNguoiId == i.Id)
                                      .ToListAsync();
                    foreach (var d in dsNhanChung)
                    {
                        DeleteImage(d.FileDinhKem, "AnhNhanChung");
                    }
                    if (dsNhanChung != null && dsNhanChung.Any())
                    {
                        db.NhanChungs.RemoveRange(dsNhanChung);
                    }
                    Console.WriteLine("Xóa Nhân Chứng : id -> " + i.Id);


                    // Xóa Báo Cáo Bài viết

                    var dsBCBV = await db.BaoCaoBaiViets
                                      .Where(m => m.MaBaiViet == i.Id)
                                      .ToListAsync();
                    foreach (var d in dsBCBV)
                    {
                        DeleteImage(d.HinhAnh, "AnhMinhTrungBaoCaoBaiViet");
                    }
                    if (dsBCBV != null && dsBCBV.Any())
                    {
                        db.BaoCaoBaiViets.RemoveRange(dsBCBV);
                    }
                    Console.WriteLine("Xóa Báo Cáo Bài viết : id -> " + i.Id);


                    // Xóa Tìm Thấy Thất Lạc
                    var dsTimThay = await db.TimThayNguoiThatLacs
                                      .Where(m => m.TimNguoiId == i.Id)
                                      .ToListAsync();
                    foreach (var d in dsTimThay)
                    {
                        DeleteImage(d.AnhMinhChung, "AnhMinhChungTimThay");
                    }
                    if (dsTimThay != null && dsTimThay.Any())
                    {
                        db.TimThayNguoiThatLacs.RemoveRange(dsTimThay);
                    }
                    Console.WriteLine("Xóa Tìm Thấy Thất Lạc : id -> " + i.Id);

                    // Xóa bài tìm người
                    db.TimNguois.Remove(i);
                    Console.WriteLine("Xóa bài tìm người : id -> " + i.Id);
                }

                // xóa tin nhắn

                List<NguoiThamGia> dsNguoiThamGia = db.NguoiThamGias
                        .Where(m => m.MaNguoiThamGia == y.Id)
                        .ToList();

                List<HopThoaiTinNhan> hopThoaiTinNhans = new List<HopThoaiTinNhan>();
                foreach (var d in dsNguoiThamGia)
                {
                    HopThoaiTinNhan ht = db.HopThoaiTinNhans.FirstOrDefault(u => u.Id == d.MaHopThoaiTinNhan);
                    if (ht != null)
                    {
                        hopThoaiTinNhans.Add(ht);
                    }

                }

                foreach (var d in hopThoaiTinNhans)
                {
                    // xóa tin nhắn trong họp thoại
                    List<TinNhan> tinnhan = db.TinNhans.Where(z => z.MaHopThoaiTinNhan == d.Id).ToList();
                    foreach (var t in tinnhan)
                    {
                        DeleteImage(t.HinhAnh, "AnhTinNhan");
                    }

                    if (tinnhan != null && tinnhan.Any())
                    {
                        db.TinNhans.RemoveRange(tinnhan);
                    }
                    Console.WriteLine("xóa tin nhắn trong họp thoại : id -> " + d.Id);



                    // xóa nguòi tham gia 
                    List<NguoiThamGia> nguoiThamGias = db.NguoiThamGias.Where(u => u.MaHopThoaiTinNhan == d.Id).ToList();
                    if (nguoiThamGias != null && nguoiThamGias.Any())
                    {
                        db.NguoiThamGias.RemoveRange(nguoiThamGias);
                    }
                    Console.WriteLine("xóa nguòi tham gia  : id -> " + d.Id);

                }
                // xóa họp thoại
                if (hopThoaiTinNhans.Any()) // ✅ tránh null khi gọi RemoveRange
                {
                    db.HopThoaiTinNhans.RemoveRange(hopThoaiTinNhans);
                }
                Console.WriteLine("xóa họp thoại ");

                // Xóa Ls Tim Kiếm
                var lstimkiem = db.LichSuTimKiems.Where(i => i.IdNguoiDung == y.Id).ToList();
                if (lstimkiem != null && lstimkiem.Any())
                {
                    db.LichSuTimKiems.RemoveRange(lstimkiem);
                }
                Console.WriteLine("Xóa Ls Tim Kiếm");

                // xóa hành vi đáng ngờ
                var hanhvidangngo = db.HanhViDangNgos.Where(i => i.NguoiDungId == y.Id).ToList();
                if (hanhvidangngo != null && hanhvidangngo.Any())
                {
                    db.HanhViDangNgos.RemoveRange(hanhvidangngo);
                }

                Console.WriteLine("xóa hành vi đáng ngờ");


                // Xóa vai trò của user
                var usrole = await userManager.GetRolesAsync(y);
                if (usrole != null && usrole.Any())
                {
                    await userManager.RemoveFromRolesAsync(y, usrole);
                }

                Console.WriteLine("Xóa vai trò của user");

                if (y.HinhAnh != "/uploads/avatars/default-avatar.png" && y.HinhAnh != null && y.HinhAnh != "/uploads/avatars/default-avatar.svg")
                {
                    DeleteImage(y.HinhAnh, "avatars");
                }

                // Xóa user
                db.Users.Remove(y);
                Console.WriteLine("Xóa user");


                // Lưu thay đổi chỉ một lần
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                string innerMessage = ex.InnerException?.Message ?? "No inner exception";
                return Json(new { success = false, message = $"Lỗi: {ex.Message} | Inner: {innerMessage}" });
            }

        }

        // Hàm đệ quy xóa bình luận và các reply
        private async Task DeleteCommentAndReplies(BinhLuan comment)
        {
            // Xóa hình ảnh nếu có
            if (!string.IsNullOrEmpty(comment.HinhAnh))
            {
                DeleteImage(comment.HinhAnh, "BinhLuan");
            }
            List<BinhLuan> dsTL = db.BinhLuans.Where(x => x.ParentId == comment.Id).ToList();

            // Xóa đệ quy các reply
            if (dsTL != null)
            {
                foreach (BinhLuan reply in dsTL)
                {
                    await DeleteCommentAndReplies(reply);
                }
            }
            List<BaoCaoBinhLuan> ds = db.BaoCaoBinhLuans.Where(x => x.MaBinhLuan == comment.Id).ToList();
            if (ds != null && ds.Any())
            {
                db.BaoCaoBinhLuans.RemoveRange(ds);
            }


            db.BinhLuans.Remove(comment);
        }

       
    }

}
