// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Data;
using Microsoft.EntityFrameworkCore;

namespace WebTimNguoiThatLac.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }
        public string Email { get; set; }
        public string HinhAnh { get; set; }
        public IList<TimNguoi> UserPosts { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

            [Required]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Địa chỉ")]
            public string Address { get; set; }

            [Display(Name = "Ngày sinh")]
            [DataType(DataType.Date)]
            public DateTime? NgaySinh { get; set; }

            [Display(Name = "Ảnh đại diện")]
            public IFormFile AvatarFile { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            Username = userName;
            Email = email;
            HinhAnh = user.HinhAnh;
            UserPosts = await _context.TimNguois
                .Include(t => t.AnhTimNguois)
                .Where(t => t.IdNguoiDung == user.Id)
                .ToListAsync();

            Input = new InputModel
            {
                FullName = user.FullName,
                PhoneNumber = phoneNumber,
                Address = user.Address,
                NgaySinh = user.NgaySinh
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            user.FullName = Input.FullName;
            user.Address = Input.Address;
            user.NgaySinh = Input.NgaySinh;

            if (Input.AvatarFile != null && Input.AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Xóa ảnh avatar cũ nếu tồn tại và không phải là ảnh mặc định
                if (!string.IsNullOrEmpty(user.HinhAnh) && !user.HinhAnh.EndsWith("default-avatar.svg"))
                {
                    var oldAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.HinhAnh.TrimStart('/'));
                    if (System.IO.File.Exists(oldAvatarPath))
                    {
                        System.IO.File.Delete(oldAvatarPath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.AvatarFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.AvatarFile.CopyToAsync(fileStream);
                }

                user.HinhAnh = "/uploads/avatars/" + uniqueFileName;
            }

            await _userManager.UpdateAsync(user);

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Cập nhật thông tin thành công.";
            return RedirectToPage();
        }
    }
}
