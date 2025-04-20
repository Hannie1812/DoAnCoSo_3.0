using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Models;
using System.IO;
using System.Threading.Tasks;
using WebTimNguoiThatLac.Data;
using DocumentFormat.OpenXml.Office2010.Excel;
using WebTimNguoiThatLac.Repositories;
using Microsoft.AspNetCore.Authorization;
using WebTimNguoiThatLac.Services;
using DocumentFormat.OpenXml.ExtendedProperties;
using X.PagedList.Extensions;

public class NhanChungController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITimNguoiRepository _timNguoiRepository;
    private readonly ILogger<NhanChungController> _logger;
    private readonly EmailService _emailService;

    public NhanChungController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITimNguoiRepository timNguoiRepository, ILogger<NhanChungController> logger, EmailService _emailService)
    {
        _context = context;
        _userManager = userManager;
        _timNguoiRepository = timNguoiRepository;
        _logger = logger;
        this._emailService = _emailService;
    }

    public async Task<IActionResult> Index(int id, int page = 1,
             string searchHoTen = "", string searchEmail = "", string searchPhone = "")
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục";
            return RedirectToAction("Login", "Account");
        }

        ApplicationUser us = await _userManager.GetUserAsync(User);
        if (us == null)
        {
            TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục";
            return RedirectToAction("Login", "Account");
        }

        int pageSize = 5; // Số lượng item trên mỗi trang
        ViewBag.TimNguoiId = id; // Lưu id để sử dụng trong view

        // Base query
        var query = _context.NhanChungs
            .Include(u => u.TimNguoi)
                .ThenInclude(u => u.ApplicationUser)
            .Where(i => i.TimNguoiId == id && i.TimNguoi.IdNguoiDung == us.Id)
            .AsQueryable();

        // Apply search filters
        if (!string.IsNullOrEmpty(searchHoTen))
        {
            query = query.Where(n => n.HoTen.Contains(searchHoTen));
        }

        if (!string.IsNullOrEmpty(searchEmail))
        {
            query = query.Where(n => n.Email.Contains(searchEmail));
        }

        if (!string.IsNullOrEmpty(searchPhone))
        {
            query = query.Where(n => n.SoDienThoai.Contains(searchPhone));
        }

        // Order and paginate
        var ds =  query
            .OrderByDescending(x => x.NgayBaoTin)
            .ToPagedList(page, pageSize);

        // Pass search parameters to view to maintain them in pagination
        ViewBag.SearchHoTen = searchHoTen;
        ViewBag.SearchEmail = searchEmail;
        ViewBag.SearchPhone = searchPhone;

        return View(ds);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        var nhanChung = await _context.NhanChungs.FindAsync(id);
        if (nhanChung == null)
        {
            return Json(new { success = false, message = "Nhân chứng không tồn tại" });
        }

        // Verify ownership
        var userId = _userManager.GetUserId(User);
        var timNguoi = await _context.TimNguois
            .FirstOrDefaultAsync(t => t.Id == nhanChung.TimNguoiId && t.IdNguoiDung == userId);

        if (timNguoi == null)
        {
            return Json(new { success = false, message = "Bạn không có quyền xóa nhân chứng này" });
        }
<<<<<<< Updated upstream
=======

        // Lưu thông tin vào ViewBag
        ViewBag.TenNguoiMatTich = timNguoi.HoTen;
        ViewBag.KhuVucMatTich = timNguoi.KhuVuc;
        ViewBag.NgaySinh = timNguoi.NgaySinh?.ToString("dd/MM/yyyy") ?? "Không có thông tin";


        
>>>>>>> Stashed changes

        _context.NhanChungs.Remove(nhanChung);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã xóa nhân chứng thành công" });
    }

    public async Task<IActionResult> GetDetails(int id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        var nhanChung = await _context.NhanChungs
            .Include(n => n.TimNguoi)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nhanChung == null)
        {
            return Json(new { success = false, message = "Nhân chứng không tồn tại" });
        }

        // Verify ownership
        var userId = _userManager.GetUserId(User);
        if (nhanChung.TimNguoi.IdNguoiDung != userId)
        {
            return Json(new { success = false, message = "Bạn không có quyền xem thông tin này" });
        }

        return Json(new
        {
            success = true,
            hoTen = nhanChung.HoTen,
            email = nhanChung.Email,
            soDienThoai = nhanChung.SoDienThoai,
            thoiGian = nhanChung.ThoiGian?.ToString("dd/MM/yyyy HH:mm"),
            diaDiem = nhanChung.DiaDiem,
            moTa = nhanChung.MoTaManhMoi,
            fileDinhKem = nhanChung.FileDinhKem,
            ngayBaoTin = nhanChung.NgayBaoTin.ToString("dd/MM/yyyy HH:mm")
        });
    }


    public async Task<string> SaveImage(IFormFile ImageURL, string subFolder)
    {

        if (ImageURL == null || ImageURL.Length == 0)
        {
            throw new ArgumentException("File không hợp lệ!");
        }

        // Đường dẫn thư mục lưu ảnh trong wwwroot/uploads/tin-tuc
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Tạo tên file duy nhất để tránh trùng lặp
        string fileExtension = Path.GetExtension(ImageURL.FileName);
        string fileName = Path.GetFileNameWithoutExtension(ImageURL.FileName);
        string uniqueFileName = fileName + "_" + Guid.NewGuid().ToString("N") + fileExtension;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Lưu file vào thư mục
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await ImageURL.CopyToAsync(fileStream);
        }

        // Trả về đường dẫn tương đối để hiển thị ảnh trên web
        return $"/uploads/{subFolder}/{uniqueFileName}";
    }


    public void DeleteImage(string ImageURL, string subFolder)
    {
        if (string.IsNullOrEmpty(ImageURL))
        {
            throw new ArgumentException("Đường dẫn ảnh không hợp lệ!");
        }

        // Lấy đường dẫn tuyệt đối của ảnh trong thư mục wwwroot/uploads/
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
        string filePath = Path.Combine(uploadsFolder, Path.GetFileName(ImageURL));

        // Kiểm tra nếu file tồn tại thì xóa
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }


    [HttpGet]
    public async Task<IActionResult> Create(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục";
            return RedirectToAction("Login", "Account");
        }

        var timNguoi = await _context.TimNguois
            .Include(t => t.ApplicationUser)
            .Include(t => t.AnhTimNguois)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (timNguoi == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy bài viết về người mất tích";
            return RedirectToAction("Index", "Home");
        }

        //Tự động điền thông tin người dùng nếu có
       var model = new NhanChung
       {
           TimNguoiId = timNguoi.Id,
           HoTen = currentUser.FullName,
           Email = currentUser.Email,
           SoDienThoai = currentUser.PhoneNumber
       };

        ViewBag.BaiTimNguoi = timNguoi;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NhanChung model, IFormFile file)
    {
        var timNguoi = await _context.TimNguois
            .Include(t => t.ApplicationUser)
            .Include(t => t.AnhTimNguois)
            .FirstOrDefaultAsync(t => t.Id == model.TimNguoiId);

        if (timNguoi == null)
        {
            ViewData["ErrorMessage"] = "Không tìm thấy bài viết về người mất tích";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.BaiTimNguoi = timNguoi;

        if (!ModelState.IsValid)
        {
            ViewData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin đã nhập";
            return View(model);
        }

        // Xử lý file đính kèm
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Vui lòng tải lên hình ảnh minh chứng");
            return View(model);
        }

        // Kiểm tra kích thước file
        if (file.Length > 5 * 1024 * 1024) // 5MB
        {
            ModelState.AddModelError("", "File đính kèm không được vượt quá 5MB");
            ViewData["ErrorMessage"] = "File đính kèm không được vượt quá 5MB";
            return View(model);
        }

        // Kiểm tra loại file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".mp4" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (JPEG, PNG) hoặc video (MP4)");
            ViewData["ErrorMessage"] = "Chỉ chấp nhận file ảnh (JPEG, PNG) hoặc video (MP4)";
            return View(model);
        }

        try
        {

            NhanChung x = new NhanChung();
            x.HoTen = model.HoTen;
            x.Email = model.Email;
            x.SoDienThoai = model.SoDienThoai;
            x.ThoiGian = model.ThoiGian;
            x.DiaDiem = model.DiaDiem;
            x.MoTaManhMoi = model.MoTaManhMoi;
            x.TimNguoiId = model.TimNguoiId;
            x.NgayBaoTin = DateTime.Now;
            
      

            // Lưu file
            x.FileDinhKem = await SaveImage(file, "AnhNhanChung");

            _context.NhanChungs.Add(x);
            await _context.SaveChangesAsync();


            // Gửi email thông báo cho người dùng
            var user = await _userManager.FindByIdAsync(timNguoi.IdNguoiDung);
            if (user != null)
            {
                var emailContent = $"BẠN CÓ 1 BÀI BÁO CÁO NHÂN CHỨNG VỀ BÀI VIẾT" + timNguoi.TieuDe;
                await _emailService.SendEmailAsync(user.Email, "Thông báo về thông tin nhân chứng", emailContent);
            }



            TempData["GuiNhanChungThanhCong"] = "Thông tin nhân chứng đã được gửi thành công!";
            return RedirectToAction("ChiTietBaiTimNguoi", "TimNguoi", new { id = model.TimNguoiId });


            //ViewData["SuccessMessage"] = "Thông tin nhân chứng đã được gửi thành công!";
            //return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu thông tin nhân chứng");
            ViewData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu thông tin. Vui lòng thử lại.";
            ModelState.AddModelError("", "ex: "+ ex);
            return View(model);
        }
    }
}