using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Components
{
    public class BaiVietMoiViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        public BaiVietMoiViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            var comments = await _db.TinTucs
                .OrderByDescending(b => b.NgayDang)
                .Take(3)
                .ToListAsync();

            return View("_DanhSachTinTucFooter", comments);
        }
    }
}
