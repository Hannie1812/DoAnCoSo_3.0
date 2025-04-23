using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Middlewares;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Repositories;
using WebTimNguoiThatLac.Services;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ xác thực
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies"; // Sử dụng cookie để lưu trữ thông tin đăng nhập
    options.DefaultChallengeScheme = "Google"; // Sử dụng Google làm phương thức đăng nhập mặc định
})
    .AddCookie("Cookies") // Sử dụng cookie để lưu trữ thông tin đăng nhập
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    });

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<WordExportService>();// Xuất File Đăng Kí

builder.Services.AddTransient<IEmailSender, EmailService>(); // email
builder.Services.AddScoped<EmailService>(); // Đăng ký EmailService
builder.Services.AddScoped<OtpService>(); // Đăng ký OtpService

// Trong ConfigureServices (Startup.cs) hoặc Program.cs
builder.Services.AddScoped<ITimNguoiRepository, EFTimNguoiRepository>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddDefaultTokenProviders()
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.LogoutPath = $"/Identity/Account/AccessDenied";

});

// Add services to the container.
builder.Services.AddControllersWithViews();



// Đăng ký Session (cần cho SessionId) -> Ghi log
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); //n�y 

app.UseAuthorization();

app.MapRazorPages(); // n�y



// Sử dụng Session (cần cho SessionId)
app.UseSession();

app.UseMiddleware<GhiLogNguoiDungMiddleware>(); // Ghi log người dùng

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();