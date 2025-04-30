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

        /*public IActionResult Index(string? TimKiem, int? tinhThanhId, int? quanHuyenId)
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

        }*/
        public IActionResult Index(string? TimKiem, int? tinhThanhId, int? quanHuyenId)
        {
            ViewBag.TimKiem = TimKiem;
            ViewBag.TinhThanhList = db.TinhThanhs.OrderBy(t => t.TenTinhThanh).ToList();
            ViewBag.QuanHuyenList = db.QuanHuyens.OrderBy(q => q.TenQuanHuyen).ToList();

            // Base query
            var query = db.TimNguois
                          .Include(u => u.ApplicationUser)
                          .Include(u2 => u2.AnhTimNguois)
                          .Include(q => q.QuanHuyen) // Nếu cần hiển thị tên quận huyện
                          .Include(t => t.TinhThanh)  // Nếu cần hiển thị tên tỉnh thành
                          .Where(i => i.active == true);

            // Filter by Tỉnh Thành (sử dụng trực tiếp IdTinhThanh trong TimNguoi)
            if (tinhThanhId.HasValue)
            {
                query = query.Where(x => x.IdTinhThanh == tinhThanhId.Value);
            }

            // Filter by Quận Huyện
            if (quanHuyenId.HasValue)
            {
                query = query.Where(x => x.IdQuanHuyen == quanHuyenId.Value);
            }

            // Filter by TimKiem
            if (!string.IsNullOrEmpty(TimKiem))
            {
                TimKiem = WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(TimKiem);
                query = query.Where(x =>
                    WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(x.HoTen).Contains(TimKiem) ||
                    WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(x.MoTa).Contains(TimKiem) ||
                    WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(x.DaciemNhanDang).Contains(TimKiem) ||
                    WebTimNguoiThatLac.BoTro.Filter.ChuyenCoDauThanhKhongDau(x.KhuVuc).Contains(TimKiem));
            }

            var ds = query.OrderByDescending(x => x.Id).ToList();
            return View(ds);
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
                    TempData["SuccessMessage"] = "Thông tin liên hệ của bạn đã được gửi thành công.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông tin liên hệ.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi, vui lòng thử lại sau.";
            }

            return View(lh);
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
        [HttpGet]
        public async Task<IActionResult> GetQuanHuyenByTinhThanh(int tinhThanhId)
        {
            var quanHuyens = await db.QuanHuyens
                .Where(q => q.IdTinhThanh == tinhThanhId)
                .Select(q => new { id = q.Id, tenQuanHuyen = q.TenQuanHuyen })
                .ToListAsync();

            return Json(quanHuyens);
        }

    }
}
