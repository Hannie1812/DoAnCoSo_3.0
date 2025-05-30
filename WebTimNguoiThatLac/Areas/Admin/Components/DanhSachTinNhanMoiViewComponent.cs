using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Areas.Admin.Components
{
    public class DanhSachTinNhanMoiViewComponent : ViewComponent 
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _us;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DanhSachTinNhanMoiViewComponent(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> us)
        {
            _db = db;
            _roleManager = roleManager;
            _us = us;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            ApplicationUser user = _us.GetUserAsync((System.Security.Claims.ClaimsPrincipal)User).Result;
            var comments = new List<HopThoaiTinNhan>();
            if (user == null)
            {
                return View("_DanhSachTinTucFooter", comments);
            }


            var conversations = await _db.NguoiThamGias
               .Include(ng => ng.HopThoai)
                   .ThenInclude(h => h.TinNhans.OrderByDescending(t => t.NgayGui).Take(1))
               .Include(ng => ng.HopThoai)
                   .ThenInclude(h => h.NguoiThamGias)
                        .ThenInclude(tv => tv.ApplicationUser)
               .Where(ng => ng.MaNguoiThamGia == user.Id && ng.HopThoai.TinNhans.Any(tn => tn.IsRead == false && tn.MaNguoiGui != user.Id))
               .ToListAsync();

             comments = conversations.Select(ng => ng.HopThoai).ToList() ?? new List<HopThoaiTinNhan>();


            return View("_TinNhanChuaDocAdmin", comments);
        }
    }
}
