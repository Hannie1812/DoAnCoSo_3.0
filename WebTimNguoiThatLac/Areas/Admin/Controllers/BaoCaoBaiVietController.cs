using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Services;
using X.PagedList;
using X.PagedList.Extensions;
using WebTimNguoiThatLac.BoTro;
using WebTimNguoiThatLac.Areas.Admin.Models;
namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Moderator},{SD.Role_Admin}")]
    public class BaoCaoBaiVietController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;
        private const int ReportThreshold = 3; // Ngưỡng báo cáo để tự động vô hiệu hóa

        public BaoCaoBaiVietController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string TiemKiem = "", int page = 1, string status = "all")
        {
            var query = _context.BaoCaoBaiViets
                .Include(b => b.TimNguoi)
                .Include(b => b.ApplicationUser)
                .AsQueryable();
            
            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(TiemKiem))
            {
                query = query.Where(b =>
                    b.TimNguoi.TieuDe.Contains(TiemKiem) ||
                    b.ApplicationUser.FullName.Contains(TiemKiem) ||
                    b.LyDo.Contains(TiemKiem));
            }

            // Lọc theo trạng thái
            switch (status)
            {
                case "read":
                    query = query.Where(b => b.DaDoc);
                    break;
                case "unread":
                    query = query.Where(b => !b.DaDoc);
                    break;
            }

            // Sắp xếp và phân trang
            var reports = query
                .OrderByDescending(b => b.NgayBaoCao)
                .ToPagedList(page, PageSize);

            // Kiểm tra và xử lý bài viết có nhiều báo cáo
            //await ProcessReportedPosts();

            ViewBag.TiemKiem = TiemKiem;
            ViewBag.Status = status;

            return View(reports);
        }

        private async Task ProcessReportedPosts()
        {
            // Lấy danh sách bài viết có số báo cáo >= ngưỡng
            var problematicPosts = await _context.BaoCaoBaiViets
                .Where(b => !b.check)
                .GroupBy(b => b.TimNguoi.Id)
                .Where(g => g.Count() >= ReportThreshold)
                .Select(g => g.Key)
                .ToListAsync();

            // Vô hiệu hóa các bài viết này
            foreach (var postId in problematicPosts)
            {
                var post = await _context.TimNguois.Include(z => z.ApplicationUser).FirstOrDefaultAsync(i => i.Id == postId);
                if (post != null && post.active)
                {
                    post.active = false;
                    _context.Update(post);

                    // Gửi email thông báo cho người dùng
                    var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == post.ApplicationUser.Id);
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

                            };


                            _context.HanhViDangNgos.Add(hanhVi);
                            await _context.SaveChangesAsync();
                        }
                    }


                    // Cập nhật số lần vi phạm

                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.BaoCaoBaiViets
                .Include(b => b.TimNguoi.ApplicationUser)
                .Include(b => b.ApplicationUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            // Đánh dấu đã đọc
            if (!report.DaDoc)
            {
                report.DaDoc = true;
                await _context.SaveChangesAsync();
            }

            return View(report);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.BaoCaoBaiViets.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            if(report.HinhAnh != null)
            {
                DeleteImage(report.HinhAnh, "AnhMinhTrungBaoCaoBaiViet");
            }
            _context.BaoCaoBaiViets.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var report = await _context.BaoCaoBaiViets.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            report.DaDoc = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostStatus(int postId, int ReportId)
        {
            try
            {
                var post = await _context.TimNguois
                                                    .Include(u => u.ApplicationUser)
                                                    .FirstOrDefaultAsync(i => i.Id == postId);
                
                var baocao = await _context.BaoCaoBaiViets
                                                    .Include(u => u.ApplicationUser)
                                                    .FirstOrDefaultAsync(i => i.Id == ReportId);
                if (post == null)
                {
                    return NotFound();
                }

                if (post.active == true)
                {
                    // Gửi thông báo cho Người Dùng
                    await _emailService.SendEmailAsync(
                        post.ApplicationUser.Email,
                        BoTro.ThongTinEmail.TieuDeDangBaiThanhCong,
                        BoTro.ThongTinEmail.NoiDungDangBaiThanhCong(post)
                    );
                }
                else
                {
                    // Gửi thông báo cho Người Dùng
                    await _emailService.SendEmailAsync(
                        post.ApplicationUser.Email,
                        BoTro.ThongTinEmail.TieuDeDangBaiThatBai,
                        BoTro.ThongTinEmail.NoiDungDangBaiThatBai(post)
                    );
                }

                
                post.active = !post.active;
                _context.Update(post);
                await _context.SaveChangesAsync();


                if(post.active == true)
                {
                    
                    var reports = await _context.BaoCaoBaiViets
                        .Where(b => b.MaBaiViet == postId)
                        .ToListAsync();
                    _context.BaoCaoBaiViets.RemoveRange(reports);

                    foreach (var report in reports)
                    {
                        report.check = true; // Đánh dấu là đã xử lý
                    }
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = $"Đã {(post.active ? "kích hoạt" : "vô hiệu hóa")} bài viết thành công";
                return RedirectToAction("Details", new { id = ReportId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể gửi email thông báo cho người dùng " + ex.Message;
                return RedirectToAction("Details", new { id=ReportId });
            }
        }
    }
}