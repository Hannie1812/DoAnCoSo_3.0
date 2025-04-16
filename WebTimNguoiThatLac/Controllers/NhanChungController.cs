using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Models;
using System.IO;
using System.Threading.Tasks;
using WebTimNguoiThatLac.Data;

public class NhanChungController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NhanChungController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Lấy thông tin người dùng hiện tại
        var currentUser = await _userManager.GetUserAsync(User);

        // Tạo đối tượng model với thông tin tự động điền
        var model = new NhanChung
        {
            HoTen = currentUser?.FullName, // Kiểm tra currentUser không null
            Email = currentUser?.Email,
            SoDienThoai = currentUser?.PhoneNumber
        };

        // Luôn gán ViewBag, kể cả khi không có người mất tích
        var nguoiMatTiches = _context.TimNguois.ToList();
        ViewBag.NguoiMatTiches = nguoiMatTiches ?? new List<TimNguoi>();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(NhanChung model)
    {
        if (ModelState.IsValid)
        {
            string filePath = null;

            // Lưu file upload, nếu có
            var files = Request.Form.Files;
            if (files.Count > 0)
            {
                var file = files[0];
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                model.FileDinhKem = file.FileName; // Lưu tên file vào model
            }

            // Lưu dữ liệu nhân chứng
            _context.NhanChungs.Add(model);
            await _context.SaveChangesAsync();

            //return RedirectToAction("Success");

            TempData["SuccessMessage"] = "Thông tin đã được gửi thành công!";
            return RedirectToAction("ChiTietBaiViet", "TimNguoi", new { id = model.Id });
        }
        /*
                ViewBag.NguoiMatTiches = _context.TimNguois.ToList(); // Load lại danh sách người mất tích nếu có lỗi
                return View(model);*/

        TempData["ErrorMessage"] = "Đã xảy ra lỗi. Vui lòng thử lại.";
        return RedirectToAction("ChiTietBaiViet", "TimNguoi", new { id = model.Id });
    }

    public IActionResult Success()
    {
        return View(); // Hiển thị thông báo thành công
    }
}