using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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
        private UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            this.db = db;
            _userManager = userManager;
        }


        public IActionResult Index()
        {

            int sl = db.TimNguois.ToList().Count;
            ViewBag.TongBaiVietTrong = sl;
            return View();


        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> GioiThieu()
        {
            int sl = db.TimNguois.ToList().Count;
            ViewBag.TongBaiVietTrong = sl;
            GioiThieu ds = await db.GioiThieus.FirstOrDefaultAsync(i => i.Active == true);

            


            List<ApplicationUser> dsAdmin = db.Users.Where(i => i.IsAdmin == true).ToList();
            ViewBag.DsAdmin = dsAdmin;
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
                    if(lh.TenNguoiDungLienHe == null)
                    {
                        ModelState.AddModelError("", "Hãy Nhập Đủ Thông Tin");
                        TempData["ErrorMessage"] = "Vui Lòng Nhập Đủ Thông Tin";
                        return View(lh);
                    }
                    if(lh.VanDeLienHe == null)
                    {
                        ModelState.AddModelError("", "Hãy Nhập Đủ Thông Tin");
                        TempData["ErrorMessage"] = "Vui Lòng Nhập Đủ Thông Tin";
                        return View(lh);
                    }
                    if(lh.EmailNguoiDungLienHe == null)
                    {
                        ModelState.AddModelError("", "Hãy Nhập Đủ Thông Tin");
                        TempData["ErrorMessage"] = "Vui Lòng Nhập Đủ Thông Tin";
                        return View(lh);
                    }
                    if (lh.PhoneNguoiDungLienHe == null)
                    {
                        ModelState.AddModelError("", "Hãy Nhập Đủ Thông Tin");
                        TempData["ErrorMessage"] = "Vui Lòng Nhập Đủ Thông Tin";
                        return View(lh);
                    }

                    lh.NgayLienHe = DateTime.Now;
                    db.NguoiDungLienHes.Add(lh);
                    await db.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đã Gửi Thành Công";
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
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại sau" + ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra, vui lòng thử lại sau: "+ ex.Message;
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
