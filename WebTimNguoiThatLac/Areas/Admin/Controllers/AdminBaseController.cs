using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Areas.Admin.Controllers
{
    public abstract class AdminBaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public AdminBaseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "Admin" });
                return;
            }

            var conversations = await _context.NguoiThamGias
               .Include(ng => ng.HopThoai)
                   .ThenInclude(h => h.TinNhans.OrderByDescending(t => t.NgayGui).Take(1))
               .Include(ng => ng.HopThoai)
                   .ThenInclude(h => h.NguoiThamGias)
                        .ThenInclude(tv => tv.ApplicationUser)
               .Where(ng => ng.MaNguoiThamGia == user.Id && ng.HopThoai.TinNhans.Any(tn => tn.IsRead == false && tn.MaNguoiGui != user.Id))
               .ToListAsync();

            ViewBag.DSTinNhanMoi = conversations.Select(ng => ng.HopThoai).ToList() ?? new List<HopThoaiTinNhan>();

            ViewBag.DemBaoCaoBinhLuan = await _context.BaoCaoBinhLuans
                .Include(b => b.BinhLuan)
                .Where(b => b.DaDoc == false)
                .CountAsync();


            ViewBag.DemBaoCaoBaiViet = await _context.BaoCaoBaiViets
                .Include(b => b.TimNguoi)
                .Where(b => b.DaDoc == false)
                .CountAsync();            ViewBag.DemLienHeNguoiDung = await _context.NguoiDungLienHes
                .Where(b => b.isRead == false).CountAsync();

            // Thực thi action chính
            await next();
        }
    }
}
