using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Components
{
    public class TinTucNgangViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public TinTucNgangViewComponent(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<TinTuc> ds = await db.TinTucs.Where(i => i.Active == true).ToListAsync();

            return View("DanhSachNgang", ds);
        }
    }
}
