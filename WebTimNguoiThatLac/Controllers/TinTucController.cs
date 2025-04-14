using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Controllers
{
    public class TinTucController : Controller
    {
        private ApplicationDbContext db;
        public TinTucController(ApplicationDbContext db)
        {

            this.db = db;
        }
        public async Task<IActionResult> Index(string TimKiem = "", int Page = 1)
        {

            IEnumerable<TinTuc> ds = await db.TinTucs.ToListAsync();
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
                List<TinTuc> dsTimKiem = new List<TinTuc>();
                foreach (TinTuc i in ds)
                {
                    string r1 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.TieuDe);
                    if (r1.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }
                    string r2 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.TacGia);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        dsTimKiem.Add(i);
                        continue;
                    }

                }
                var dsTrang = dsTimKiem.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
        }

        public async Task<IActionResult> Details(int id)
        {

            TinTuc y = db.TinTucs.FirstOrDefault(i => i.Id == id);
            if (y == null)
            {
                return RedirectToAction("Index");
            }
            List<TinTuc> dsMoi = await db.TinTucs.OrderByDescending(i => i.NgayDang).Take(10).ToListAsync();
            ViewBag.DSTinTucMoi = dsMoi;
            return View(y);

        }
    }
}
