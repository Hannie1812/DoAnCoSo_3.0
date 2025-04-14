using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using System.Security.Claims;
using WebTimNguoiThatLac.Models;
using Microsoft.AspNetCore.Identity;

namespace WebTimNguoiThatLac.ViewComponents
{
    public class BinhLuanChuaDocViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> userManager;
        public BinhLuanChuaDocViewComponent(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            this.userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("_DanhSachThongBao", new List<BinhLuan>());
            }

            var userId = (User as ClaimsPrincipal)?.FindFirstValue(ClaimTypes.NameIdentifier);
            var comments = await _db.BinhLuans
                .Include(b => b.ApplicationUser)
                .Include(b => b.TimNguoi)
                .Where(b => b.TimNguoi.IdNguoiDung == userId && !b.DaDoc)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Take(10)
                .ToListAsync();

            return View("_DanhSachThongBao", comments);
        }
    }
}