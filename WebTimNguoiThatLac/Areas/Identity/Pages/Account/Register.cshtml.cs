// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WebTimNguoiThatLac.Areas.Admin.Models;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager; //Admins
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            RoleManager<IdentityRole> roleManager,//Admins
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _roleManager = roleManager;//Admins
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }
        [BindProperty]
        public IFormFile CccdImage { get; set; }

        [BindProperty]
        public string CapturedImageBase64 { get; set; }
        [BindProperty]
        public bool IsFaceVerified { get; set; }

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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            public string? Address { get; set; }

            public string? CCCD { get; set; }
            [Required]
            public string FullName { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                // Nếu chưa tồn tại role Role_Customer thì tạo ra all role
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Moderator)).GetAwaiter().GetResult();

            }
            if ((await _userManager.GetUsersInRoleAsync(SD.Role_Admin)).Count == 0)
            {
                // Tạo tài khoản admin mặc định
                var adminUser = new ApplicationUser
                {
                    UserName = "Admin9999@gmail.com",
                    Email = "Admin9999@gmail.com",
                    FullName = "Quản trị viên",
                    EmailConfirmed = true, // Bỏ qua xác thực email cho admin
                    Active = true,
                    IsAdmin = true,

                };

                var result = await _userManager.CreateAsync(adminUser, "Admin9999@gmail.com"); // Mật khẩu mạnh mặc định
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
                    _logger.LogInformation("Đã tạo tài khoản admin mặc định.");
                }
                else
                {
                    _logger.LogError("Không thể tạo tài khoản admin mặc định: {Errors}", result.Errors);
                }
            }
            Input = new()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        private async Task<bool> VerifyFaceAsync(IFormFile cccdImage, string capturedImageBase64)
        {
            try
            {
                if (cccdImage == null || string.IsNullOrEmpty(capturedImageBase64))
                {
                    _logger.LogWarning("VerifyFaceAsync: Ảnh CCCD hoặc ảnh khuôn mặt không được cung cấp");
                    return false;
                }

                // Loại bỏ prefix của base64 image nếu có
                string base64Data = capturedImageBase64;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                }

                var apiUrl = "https://api.fpt.ai/vision/face/verifications";
                var apiKey = "S6X5dLj4vaMzn52cEWshCUrXLgkb7lJi";

                byte[] cccdBytes;
                using (var ms = new MemoryStream())
                {
                    await cccdImage.CopyToAsync(ms);
                    cccdBytes = ms.ToArray();
                }

                byte[] faceBytes;
                try
                {
                    faceBytes = Convert.FromBase64String(base64Data);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Lỗi chuyển đổi ảnh khuôn mặt từ base64");
                    return false;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", apiKey);

                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(cccdBytes), "image1", "cccd.jpg");
                content.Add(new ByteArrayContent(faceBytes), "image2", "face.jpg");

                var response = await client.PostAsync(apiUrl, content);
                var json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"API Response: {json}");

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                double similarity = result?.data?.similarity ?? 0;

                return similarity >= 0.75;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình xác thực khuôn mặt");
                return false;
            }
        }

        public async Task<IActionResult> OnPostAsync(string action, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Để đảm bảo RoleList luôn có sẵn trong mọi trường hợp
            Input.RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            // Xử lý xác thực khuôn mặt
            if (action == "verify")
            {
                // Kiểm tra xem đã có hình ảnh chưa
                if (CccdImage == null)
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng tải lên ảnh CCCD.");
                    return Page();
                }

                if (string.IsNullOrEmpty(CapturedImageBase64))
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng chụp ảnh khuôn mặt.");
                    return Page();
                }

                try
                {
                    _logger.LogInformation("Bắt đầu xác thực khuôn mặt");
                    var isVerified = await VerifyFaceAsync(CccdImage, CapturedImageBase64);

                    if (isVerified)
                    {
                        IsFaceVerified = true;
                        ViewData["FaceVerifyMessage"] = "Xác thực khuôn mặt thành công!";
                        _logger.LogInformation("Xác thực khuôn mặt thành công");
                    }
                    else
                    {
                        IsFaceVerified = false;
                        ModelState.AddModelError(string.Empty, "Xác thực khuôn mặt không thành công. Vui lòng thử lại.");
                        _logger.LogWarning("Xác thực khuôn mặt không thành công");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong quá trình xác thực khuôn mặt");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình xác thực. Vui lòng thử lại.");
                }

                return Page();
            }

            // Xử lý đăng ký tài khoản
            if (action == "register")
            {
                if (!IsFaceVerified)
                {
                    ModelState.AddModelError(string.Empty, "Bạn cần xác thực khuôn mặt trước khi đăng ký.");
                    return Page();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var user = CreateUser();

                        user.FullName = Input.FullName;
                        user.CCCD = Input.CCCD;
                        user.Address = Input.Address;
                        user.HinhAnh = "/uploads/avatars/default-avatar.png";

                        if (!String.IsNullOrEmpty(Input.Role) && Input.Role == SD.Role_Admin)
                        {
                            user.IsAdmin = true;
                        }

                        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                        await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                        var result = await _userManager.CreateAsync(user, Input.Password);

                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User created a new account with password.");
                            if (!String.IsNullOrEmpty(Input.Role))
                            {
                                await _userManager.AddToRoleAsync(user, Input.Role);
                            }
                            else
                            {
                                await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                            }

                            var userId = await _userManager.GetUserIdAsync(user);
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                                protocol: Request.Scheme);

                            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                            if (_userManager.Options.SignIn.RequireConfirmedAccount)
                            {
                                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                            }
                            else
                            {
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return LocalRedirect(returnUrl);
                            }
                        }

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi trong quá trình đăng ký");
                        ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
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
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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