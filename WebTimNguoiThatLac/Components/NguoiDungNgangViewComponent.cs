using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Components
{
    public class NguoiDungNgangViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext db;

        public NguoiDungNgangViewComponent(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<ApplicationUser> ds = await db.Users.Where(i=>i.Active == true).ToListAsync();
            return View("DanhSachNgang", ds);
        }
    }
}
