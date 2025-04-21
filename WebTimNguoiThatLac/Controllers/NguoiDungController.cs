using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using X.PagedList.Extensions;

namespace WebTimNguoiThatLac.Controllers
{
    public class NguoiDungController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ApplicationDbContext db;
        public NguoiDungController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
        
        public async Task<IActionResult> BaiVietCuaToi()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var baiViet = await db.TimNguois
                    .Include(t => t.AnhTimNguois)
                    .Where(t => t.IdNguoiDung == user.Id)
                    .OrderByDescending(t => t.NgayDang)
                    .ToListAsync();
                return View(baiViet);
            }
            return RedirectToAction("Index", "Home");
        }

        // Đăng nhập bằng Google/Facebook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "TaiKhoanNguoiDung");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect("~/");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return View("ExternalLoginFailure");
            }

            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = info.Principal.FindFirstValue(ClaimTypes.Name),
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return View("ExternalLoginFailure");
                }
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect("~/");
        }


    }
}
