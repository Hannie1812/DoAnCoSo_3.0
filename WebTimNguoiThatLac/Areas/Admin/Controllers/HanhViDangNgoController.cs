
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Areas.Admin.Models;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Moderator},{SD.Role_Admin}")]
    public class HanhViDangNgoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HanhViDangNgoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? page, string searchString, string status)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            IQueryable<HanhViDangNgo> query = _context.HanhViDangNgos
                .Include(h => h.ApplicationUser)
                .OrderByDescending(h => h.ThoiGian);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h =>
                    h.NguoiDungId.Contains(searchString) ||
                    (h.ApplicationUser != null &&
                     (h.ApplicationUser.Email.Contains(searchString) ||
                      h.ApplicationUser.UserName.Contains(searchString) ||
                      h.ApplicationUser.PhoneNumber.Contains(searchString))
                ));
            }

            switch (status)
            {
                case "KhangNghi":
                    query = query.Where(h => h.KhangNghi);
                    break;
                case "ChuaXuLy":
                    query = query.Where(h => !h.DaXuLy);
                    break;
                case "all":
                default:
                    break;
            }

            ViewBag.SearchString = searchString;
            ViewBag.Status = status;

            var model = query.ToPagedList(pageNumber, pageSize);
            return View(model);
        }

        public IActionResult ChiTiet(int id)
        {
            var hanhVi = _context.HanhViDangNgos
                .Include(h => h.ApplicationUser)
                .FirstOrDefault(h => h.Id == id);

            if (hanhVi == null)
            {
                return NotFound();
            }

            if (!hanhVi.DaXem)
            {
                hanhVi.DaXem = true;
                _context.SaveChanges();
            }

            return PartialView("_ChiTietModal", hanhVi);
        }

        [HttpPost]
        public IActionResult XuLy(int id)
        {
            try
            {
                var hanhVi = _context.HanhViDangNgos.Find(id);
                if (hanhVi == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hành vi" });
                }

                hanhVi.DaXuLy = true;
                _context.SaveChanges();

                return Json(new { success = true, message = "Đã đánh dấu là đã xử lý" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult XuLyKhangNghi(int id, string trangThaiKhangNghi)
        {
            try
            {
                var hanhVi = _context.HanhViDangNgos.Find(id);
                if (hanhVi == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hành vi" });
                }

                hanhVi.TrangThaiKhangNghi = trangThaiKhangNghi;
                hanhVi.DaXuLy = true;

                

                if (trangThaiKhangNghi == "Kháng Nghị Thành Công")
                {
                    var applicationUser = _context.Users.FirstOrDefault(u => u.Id == hanhVi.NguoiDungId);
                    if (applicationUser != null)
                    {
                        applicationUser.SoLanViPham--;
                        if (applicationUser.SoLanViPham < 0)
                        {
                            applicationUser.SoLanViPham = 0;
                        }
                    }

                    if(hanhVi.LoaiViPham == "Bình Luận" && hanhVi.IdLoiViPham >0)
                    {
                        BinhLuan binhLuan = _context.BinhLuans.Find(hanhVi.IdLoiViPham);
                        if (binhLuan != null)
                        {
                            binhLuan.Active = true;
                            // các báo cáo bình luận liên quan
                            var baoCaoBinhLuans = _context.BaoCaoBinhLuans
                                .Where(b => b.MaBinhLuan == binhLuan.Id)
                                .ToList();

                            foreach (var baoCao in baoCaoBinhLuans)
                            {
                                baoCao.check = true;
                                baoCao.DaDoc = true;
                            }
                                _context.SaveChanges();
                        }
                    }

                    if (hanhVi.LoaiViPham == "Bài Viết" && hanhVi.IdLoiViPham >0)
                    {
                        TimNguoi timNguoi = _context.TimNguois.FirstOrDefault(i => i.Id == hanhVi.IdLoiViPham);
                        if (timNguoi != null)
                        {
                            timNguoi.active = true;
                            // các báo cáo bài viết liên quan
                            var baoCaoBaiViets = _context.BaoCaoBaiViets
                                .Where(b => b.MaBaiViet == timNguoi.Id)
                                .ToList();
                            foreach (var baoCao in baoCaoBaiViets)
                            {
                                baoCao.check = true;
                                baoCao.DaDoc = true;
                            }
                            _context.SaveChanges();
                        }
                    }
                }

                _context.SaveChanges();

                return Json(new { success = true, message = $"Đã xử lý kháng nghị: {trangThaiKhangNghi}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}