using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Controllers
{
    public class LoiViPhamController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> usManager;

        public LoiViPhamController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<ApplicationUser> us)
        {
            _logger = logger;
            this.db = db;
            this.usManager = us;
        }
        public async Task<IActionResult> Index(int Page = 1)
        {
            if(User.Identity.IsAuthenticated)
            {
                ApplicationUser us = await usManager.GetUserAsync(User);

                int sodongtren1trang = 5;
                List<HanhViDangNgo> ds = db.HanhViDangNgos
                                                            .Include(u => u.ApplicationUser)
                                                            .Where(i => i.NguoiDungId == us.Id)
                                                            .ToList()
                                                            .OrderByDescending(x => x.ThoiGian).ToList();
                var dsTrang = ds.ToPagedList(Page, sodongtren1trang);
                return View(dsTrang);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
    }
}
