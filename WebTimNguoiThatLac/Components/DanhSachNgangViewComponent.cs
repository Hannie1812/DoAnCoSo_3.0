using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Components
{
    public class DanhSachNgangViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public DanhSachNgangViewComponent(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<TimNguoi> ds = await db .TimNguois
                                                .Include(u => u.ApplicationUser)
                                                .Include(u => u.AnhTimNguois)
                                                .Where(i => i.active == true).ToListAsync();

            return View("DanhSachNgang", ds);
        }
    }
}
