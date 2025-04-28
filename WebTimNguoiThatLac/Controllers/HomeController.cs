using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            this.db = db;
        }

        public IActionResult Index(string? TimKiem, string? KhuVuc)
        {
            ViewBag.TimKiem = TimKiem;
            if (TimKiem.IsNullOrEmpty())
            {
                IEnumerable<TimNguoi> ds = db.TimNguois
                                                .Include(u => u.ApplicationUser)
                                                .Include(u2 => u2.AnhTimNguois)
                                                .Where(i => i.active == true)
                                                .ToList().OrderByDescending(x => x.Id);
                return View(ds);
            }
            else
            {
                IEnumerable<TimNguoi> ds = db.TimNguois
                                        .Include(u => u.ApplicationUser)
                                        .Include(u2 => u2.AnhTimNguois)
                                        .Where(i => i.active == true)
                                        .ToList().OrderByDescending(x => x.Id);
                List<TimNguoi> DSTimkiem = new List<TimNguoi>();
                foreach (TimNguoi i in ds)
                {
                    TimKiem = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(TimKiem);
                    string r1 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.HoTen);
                    if (r1.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        DSTimkiem.Add(i);
                        continue;
                    }
                    string r2 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.MoTa);
                    if (r2.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        DSTimkiem.Add(i);
                        continue;
                    }
                    string r3 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.KhuVuc);
                    if (r3.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        DSTimkiem.Add(i);
                        continue;
                    }
                    string r4 = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(i.DaciemNhanDang);
                    if (r4.ToUpper().Contains(TimKiem.ToUpper()))
                    {
                        DSTimkiem.Add(i);
                        continue;
                    }
                }
                return View(DSTimkiem);
            }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> GioiThieu()
        {
            GioiThieu ds = await db.GioiThieus.FirstOrDefaultAsync(i => i.Active == true);
            return View(ds);
        }
        public async Task<IActionResult> LienHe()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LienHe(NguoiDungLienHe lh)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    lh.NgayLienHe = DateTime.Now;
                    db.NguoiDungLienHes.Add(lh);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Hãy Nhập Đủ Thông Tin");
                    TempData["ErrorMessage"] = "Vui Lòng Nhập Đủ Thông Tin";
                }
                return View(lh);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại sau"+ ex.Message);
                return View(lh);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error()
        {
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(errorModel);
        }
    }
}
