﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

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
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [EmailAddress]
            public string Email { get; set; }
        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Kiểm tra xem user đã có tài khoản chưa
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                ErrorMessage = "Email not received from external provider.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Không cho phép đăng nhập ngoài nếu email chưa đăng ký thủ công
                TempData["ErrorMessage"] = "Email này chưa được đăng ký. Vui lòng đăng ký tài khoản thủ công trước.";
                return RedirectToPage("./Register", new { ReturnUrl = returnUrl });
                // Lấy thông tin tên từ claims
                // var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ??
                //               info.Principal.FindFirstValue("name") ??
                //               email;

                // Lấy URL avatar từ claims
                // var picture = info.Principal.Claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value ?? 
                //                 info.Principal.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

                // Tạo user mới
                // user = new ApplicationUser
                // {
                //     UserName = email,
                //     Email = email,
                //     EmailConfirmed = true,
                //     FullName = fullName,
                //     Active = true,
                //     Address = "Chưa cập nhật",
                //     PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone) ?? "",
                //     HinhAnh = picture // Lưu URL avatar
                // };

                // var createResult = await _userManager.CreateAsync(user);
                // if (!createResult.Succeeded)
                // {
                //     _logger.LogError($"User creation failed: {string.Join(", ", createResult.Errors)}");
                //     ErrorMessage = "Lỗi khi tạo tài khoản mới.";
                //     return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                // }
            }
            
            // Đăng nhập người dùng
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Kiểm tra trạng thái Active của tài khoản
            if (!user.Active)
            {
     //           ErrorMessage = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";
                //return RedirectToPage("./Login", new { ReturnUrl = returnUrl });// test
                TempData["WarningMessage"] = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.";

                return RedirectToAction("Index", "LoiViPham", new { area = "" });
            }

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            
            ErrorMessage = "Chức năng đăng ký bằng tài khoản ngoài đã bị tắt. Vui lòng đăng ký thủ công.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            // returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            // var info = await _signInManager.GetExternalLoginInfoAsync();
            // if (info == null)
            // {
            //     ErrorMessage = "Error loading external login information during confirmation.";
            //     return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            // }

            // if (ModelState.IsValid)
            // {
            //     var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };

            //     var result = await _userManager.CreateAsync(user);
            //     if (result.Succeeded)
            //     {
            //         result = await _userManager.AddLoginAsync(user, info);
            //         if (result.Succeeded)
            //         {
            //             _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

            //             var userId = await _userManager.GetUserIdAsync(user);
            //             var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //             code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //             var callbackUrl = Url.Page(
            //                 "/Account/ConfirmEmail",
            //                 pageHandler: null,
            //                 values: new { area = "Identity", userId = userId, code = code },
            //                 protocol: Request.Scheme);

            //             await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
            //                 $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            //             // If account confirmation is required, we need to show the link if we don't have a real email sender
            //             if (_userManager.Options.SignIn.RequireConfirmedAccount)
            //             {
            //                 return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
            //             }

            //             await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

            //             return LocalRedirect(returnUrl);
            //         }
            //     }
            //     foreach (var error in result.Errors)
            //     {
            //         ModelState.AddModelError(string.Empty, error.Description);
            //     }
            // }

            // ProviderDisplayName = info.ProviderDisplayName;
            // ReturnUrl = returnUrl;
            // return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}