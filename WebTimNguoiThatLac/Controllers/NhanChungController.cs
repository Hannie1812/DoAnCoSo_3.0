using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Models;
using System.IO;
using System.Threading.Tasks;
using WebTimNguoiThatLac.Data;
using DocumentFormat.OpenXml.Office2010.Excel;
using WebTimNguoiThatLac.Repositories;

public class NhanChungController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITimNguoiRepository _timNguoiRepository;
    private readonly ILogger<NhanChungController> _logger;

    public NhanChungController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITimNguoiRepository timNguoiRepository, ILogger<NhanChungController> logger)
    {
        _context = context;
        _userManager = userManager;
        _timNguoiRepository = timNguoiRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int id) // id của bài viết tìm người
    {
        // Lấy thông tin người mất tích từ database
        var timNguoi = await _context.TimNguois
            .Where(t => t.Id == id)
            .Select(t => new {
                t.HoTen,
                t.KhuVuc,
                t.NgaySinh
            })
            .FirstOrDefaultAsync();

        if (timNguoi == null)
        {
            return NotFound();
        }

        // Lưu thông tin vào ViewBag
        ViewBag.TenNguoiMatTich = timNguoi.HoTen;
        ViewBag.KhuVucMatTich = timNguoi.KhuVuc;
        ViewBag.NgaySinh = timNguoi.NgaySinh?.ToString("dd/MM/yyyy") ?? "Không có thông tin";

        // Lấy thông tin người dùng hiện tại
        var currentUser = await _userManager.GetUserAsync(User);

        // Tạo model với thông tin tự động điền
        var model = new NhanChung
        {
            TimNguoiId = id,
            HoTen = currentUser?.FullName,
            Email = currentUser?.Email,
            SoDienThoai = currentUser?.PhoneNumber
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(NhanChung model, IFormFile file)
    {
        if (!ModelState.IsValid)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogError($"Lỗi ở {state.Key}: {error.ErrorMessage}");
                }
            }
        }
        if (ModelState.IsValid)
        {
            try
            {
                // Tự động gán ngày báo tin
                model.NgayBaoTin = DateTime.Now;
                // Xử lý file đính kèm nếu có
                if (file != null && file.Length > 0)
                {
                    // Tạo thư mục upload nếu chưa tồn tại
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file unique
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    model.FileDinhKem = uniqueFileName;
                }

                // Lưu thông tin nhân chứng
                _context.NhanChungs.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thông tin nhân chứng đã được gửi thành công!";
                return RedirectToAction("ChiTietBaiViet", "TimNguoi", new { id = model.TimNguoiId });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(ex, "Lỗi khi lưu thông tin nhân chứng");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu thông tin. Vui lòng thử lại.");
            }
        }

        // Nếu có lỗi, hiển thị lại form với thông báo
        TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập";
        return View(model);
    }

    public IActionResult Success()
    {
        return View(); // Hiển thị thông báo thành công
    }
}