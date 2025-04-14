using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Controllers
{
    public class TrungBayController : Controller
    {
        private ApplicationDbContext db;
        public TrungBayController(ApplicationDbContext db)
        {

            this.db = db;
        }
        public async Task<IActionResult> Index(int Page = 1)
        {

            IEnumerable<TrungBayHinhAnh> ds = await db.TrungBayHinhAnhs.ToListAsync();
            
            int sodongtren1trang = 8;

            var dsTrang = ds.ToPagedList(Page, sodongtren1trang);
            return View(dsTrang);
           
            
        }
    }
}
