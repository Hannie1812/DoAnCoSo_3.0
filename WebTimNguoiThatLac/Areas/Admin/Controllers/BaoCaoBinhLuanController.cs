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
using System.IO;
using X.PagedList.Extensions;
using WebTimNguoiThatLac.Areas.Admin.Models;

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Moderator},{SD.Role_Admin}")]
    public class BaoCaoBinhLuanController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;
        private const int ReportThreshold = 3;

        public BaoCaoBinhLuanController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string TiemKiem = "", int page = 1, string status = "all")
        {
            var query = _context.BaoCaoBinhLuans
                .Include(b => b.BinhLuan)
                    .ThenInclude(bl => bl.TimNguoi)
                .Include(b => b.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(TiemKiem))
            {
                query = query.Where(b =>
                    b.BinhLuan.NoiDung.Contains(TiemKiem) ||
                    b.ApplicationUser.FullName.Contains(TiemKiem) ||
                    b.LyDo.Contains(TiemKiem));
            }

            switch (status)
            {
                case "read": query = query.Where(b => b.DaDoc); break;
                case "unread": query = query.Where(b => !b.DaDoc); break;
                case "processed": query = query.Where(b => b.check); break;
                case "unprocessed": query = query.Where(b => !b.check); break;
            }

            var reports = query
                .OrderByDescending(b => b.NgayBaoCao)
                .ToPagedList(page, PageSize);

            //await ProcessReportedComments();

            ViewBag.TiemKiem = TiemKiem;
            ViewBag.Status = status;

            return View(reports);
        }

        private async Task ProcessReportedComments() // Xử lý bình luận bị báo cáo
        {
            var problematicComments = await _context.BaoCaoBinhLuans
                .Where(b => !b.check)
                .GroupBy(b => b.MaBinhLuan)
                .Where(g => g.Count() >= ReportThreshold)
                .Select(g => new {
                    CommentId = g.Key,
                    ReportCount = g.Count()
                })
                .ToListAsync();

            foreach (var item in problematicComments)
            {
                var comment = await _context.BinhLuans
                    .Include(c => c.ApplicationUser)
                    .Include(c => c.TimNguoi)
                    .FirstOrDefaultAsync(c => c.Id == item.CommentId);

                if (comment != null)
                {
                    comment.Active = false;
                    _context.Update(comment);

                    var relatedReports = await _context.BaoCaoBinhLuans
                        .Where(b => b.MaBinhLuan == item.CommentId)
                        .ToListAsync();

                    foreach (var report in relatedReports)
                    {
                        report.check = true;
                        _context.Update(report);
                    }

                    if (comment.ApplicationUser != null)
                    {
                        await _emailService.SendEmailAsync(
                            comment.ApplicationUser.Email,
                            "Bình luận của bạn đã bị ẩn tự động",
                            $"Bình luận của bạn trong bài viết \"{comment.TimNguoi?.TieuDe}\" " +
                            $"đã bị ẩn tự động do nhận được {item.ReportCount} báo cáo vi phạm."
                        );

                        ApplicationUser applicationUser = await _context.Users
                            .FirstOrDefaultAsync(u => u.Id == comment.ApplicationUser.Id);
                        if (applicationUser != null)
                        {
                            applicationUser.SoLanViPham += 1;
                            await _context.SaveChangesAsync();

                          

                            HanhViDangNgo hanhVi = new HanhViDangNgo
                            {
                                NguoiDungId = applicationUser.Id,
                                HanhDong = "Bình luận bị báo cáo nhiều lần",
                                ThoiGian = DateTime.Now,
                                IdLoiViPham = comment.Id,
                                LoaiViPham = "Bình Luận",

                            };
                            _context.HanhViDangNgos.Add(hanhVi);
                            await _context.SaveChangesAsync();

                            if (applicationUser.SoLanViPham >= 3)
                            {
                                applicationUser.Active = false;

                                await _context.SaveChangesAsync();

                                await _emailService.SendEmailAsync(
                                    applicationUser.Email,
                                    "Tài khoản của bạn đã bị khóa",
                                    "Tài khoản của bạn đã bị khóa do vi phạm quy định nhiều lần. " +
                                    "Vui lòng liên hệ với quản trị viên để biết thêm chi tiết."
                                );

                            }

                            await _context.SaveChangesAsync();

                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.BaoCaoBinhLuans
                .Include(b => b.BinhLuan)
                    .ThenInclude(bl => bl.ApplicationUser)
                .Include(b => b.BinhLuan)
                    .ThenInclude(bl => bl.TimNguoi)
                .Include(b => b.ApplicationUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (report == null) return NotFound();

            if (!report.DaDoc)
            {
                report.DaDoc = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.ReportCount = await _context.BaoCaoBinhLuans
                .CountAsync(b => b.MaBinhLuan == report.MaBinhLuan);

            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) // xóa báo cáo
        {
            var report = await _context.BaoCaoBinhLuans.FindAsync(id);
            if (report == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy báo cáo";
                return RedirectToAction(nameof(Index));
            }

            _context.BaoCaoBinhLuans.Remove(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa báo cáo thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id) //  Đã đánh dấu đã đọc
        {
            var report = await _context.BaoCaoBinhLuans.FindAsync(id);
            if (report == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy báo cáo";
                return RedirectToAction(nameof(Index));
            }

            report.DaDoc = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh dấu đã đọc";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCommentStatus(int commentId, int reportId) // Ẩn/hiện bình luận
        {
            try
            {
                var comment = await _context.BinhLuans
                    .Include(c => c.ApplicationUser)
                    .Include(c => c.TimNguoi)
                    .FirstOrDefaultAsync(c => c.Id == commentId);

                if (comment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bình luận";
                    return RedirectToAction("Details", new { id = reportId });
                }

                comment.Active = !comment.Active;
                _context.Update(comment);

                var report = await _context.BaoCaoBinhLuans.FindAsync(reportId);
                if (report != null)
                {
                    report.check = true;
                    _context.Update(report);
                }
                
                if(comment.Active)
                {
                    // danh sách report liên quan bình luận
                    var relatedReports = await _context.BaoCaoBinhLuans
                        .Where(b => b.MaBinhLuan == commentId)
                        .ToListAsync();
                    foreach (var relatedReport in relatedReports)
                    {
                        relatedReport.check = true;

                    }
                    await _context.SaveChangesAsync();
                }


                await _context.SaveChangesAsync();

                if (comment.ApplicationUser != null)
                {
                    var emailSubject = comment.Active
                        ? "Bình luận của bạn đã được phê duyệt"
                        : "Bình luận của bạn đã bị ẩn";

                    await _emailService.SendEmailAsync(
                        comment.ApplicationUser.Email,
                        emailSubject,
                        $"Bình luận của bạn trong bài viết \"{comment.TimNguoi?.TieuDe}\" " +
                        $"đã được {(comment.Active ? "hiển thị lại" : "ẩn bình luận")}."
                    );
                }

                

                TempData["SuccessMessage"] = $"Đã {(comment.Active ? "Đã cho phép hiển thị bình luận" : "Đã ẩn bình luận")} bình luận thành công";
                return RedirectToAction("Details", new { id = reportId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Details", new { id = reportId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int reportId) // Xóa bình luận
        {
            try
            {
                var comment = await _context.BinhLuans
                    .Include(c => c.ApplicationUser)
                    .Include(c => c.TimNguoi)
                    .FirstOrDefaultAsync(c => c.Id == commentId);

                if (comment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bình luận";
                    return RedirectToAction("Details", new { id = reportId });
                }

                // Xóa hình ảnh nếu có
                if (!string.IsNullOrEmpty(comment.HinhAnh))
                {
                    DeleteImage(comment.HinhAnh, "BinhLuan");
                }

                // Xóa tất cả báo cáo liên quan
                var relatedReports = await _context.BaoCaoBinhLuans
                    .Where(b => b.MaBinhLuan == commentId)
                    .ToListAsync();

                _context.BaoCaoBinhLuans.RemoveRange(relatedReports);
                _context.BinhLuans.Remove(comment);
                await _context.SaveChangesAsync();

                // Gửi email thông báo
                if (comment.ApplicationUser != null)
                {
                    await _emailService.SendEmailAsync(
                        comment.ApplicationUser.Email,
                        "Bình luận của bạn đã bị xóa",
                        $"Bình luận của bạn trong bài viết \"{comment.TimNguoi?.TieuDe}\" " +
                        "đã bị xóa do vi phạm quy định."
                    );
                }

                TempData["SuccessMessage"] = "Đã xóa bình luận và các báo cáo liên quan";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa: " + ex.Message;
                return RedirectToAction("Details", new { id = reportId });
            }
        }

        private void DeleteImage(string imageUrl, string subFolder) // Xóa hình ảnh
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
                string filePath = Path.Combine(uploadsFolder, Path.GetFileName(imageUrl));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi xóa ảnh: " + ex.Message);
            }
        }
    }
}