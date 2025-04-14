using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Controllers
{
    public class NguoiDungController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private ApplicationDbContext db;
        public NguoiDungController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            this.userManager = userManager;
            this.db = db;
        }
        public async Task<IActionResult> Index(string TimKiem = "", int Page = 1)
        {

            IEnumerable<ApplicationUser> ds = await db.Users.ToListAsync();
            ViewBag.TimKiem = TimKiem;
            int sodongtren1trang = 5;

            if (TimKiem.IsNullOrEmpty())
            {
                var dsTrang = ds.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
            else
            {
                TimKiem = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(TimKiem);
                List<ApplicationUser> dsTimKiem = new List<ApplicationUser>();
                foreach (ApplicationUser i in ds)
                {
                    string r1 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.FullName);
                    if (r1.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    string r2 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.Email);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    if (i.PhoneNumber != null)
                    {
                        string r3 = i.PhoneNumber;
                        if (r3.ToUpper().Contains(TimKiem.ToUpper()))
                        {
                            dsTimKiem.Add(i);
                            continue;
                        }
                    }

                }
                var dsTrang = dsTimKiem.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
        }
        public async Task<IActionResult> ThongTinCaNhan()
        {
            ApplicationUser x = await userManager.GetUserAsync(User);
            List<TimNguoi> timNguois = await db.TimNguois.Include(u => u.AnhTimNguois).Where(i => i.IdNguoiDung == x.Id).ToListAsync();
            ViewBag.CacBaiDang = timNguois;
            return View(x);

        }
        public async Task<IActionResult> EditTaiKhoan()
        {
            ApplicationUser x = await userManager.GetUserAsync(User);
            return View(x);
        }


        
    }
}
