using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Components
{
    public class TrungBayNgangViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public TrungBayNgangViewComponent(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<TrungBayHinhAnh> ds = await db.TrungBayHinhAnhs
                                                .Where(i => i.Active == true).Take(8).ToListAsync();

            return View("DanhSachNgang", ds);
        }
    }
}
