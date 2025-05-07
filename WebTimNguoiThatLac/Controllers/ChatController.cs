using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTimNguoiThatLac.BoTro;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.ViewModels;

namespace WebTimNguoiThatLac.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        //private const string AdminId = "19dbf6fb-5ed5-4393-831d-640b5e30d6c1"; // Thay bằng ID thực của admin

        private RoleManager<IdentityRole> _roleManager;

       
        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Trang chat chính
        public async Task<IActionResult> Index(int? hopThoaiId)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                TempData["WarningMessage"] = "Vui lòng đăng nhập để sử dụng chức năng này.";
                return RedirectToAction("Login", "Account", new {area = "Identity"});
            }
            var nguoiDung = await _userManager.GetUserAsync(User);
            // kiểm tra hộp thoại có đang nhắn với admin không 
            



            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách hộp thoại - SỬA LẠI CÁC ĐIỀU KIỆN
            var conversations = await _context.NguoiThamGias
                .Include(ng => ng.HopThoai)
                    .ThenInclude(h => h.TinNhans.OrderByDescending(t => t.NgayGui).Take(1))
                .Include(ng => ng.HopThoai)
                    .ThenInclude(h => h.NguoiThamGias)
                        .ThenInclude(tv => tv.ApplicationUser)
                .Where(ng => ng.MaNguoiThamGia == userId) // Sửa điều kiện Where
                .OrderByDescending(ng => ng.HopThoai.LastMessageTime ?? ng.HopThoai.NgayTao)
                .ToListAsync();

            if(hopThoaiId != null)
            {
                // Lấy danh sách tin nhắn chưa đọc
                List<TinNhan> unreadMessages = await _context.TinNhans
                    .Include(t => t.HopThoaiTinNhan)
                    .Where(t => t.IsRead == false && t.MaNguoiGui != userId)
                    .ToListAsync();

                
             

                foreach (TinNhan i in unreadMessages)
                {
                    i.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
            


            // Lấy hộp thoại hiện tại
            HopThoaiTinNhan currentConversation = null;
            if (hopThoaiId.HasValue)
            {
                currentConversation = await _context.HopThoaiTinNhans
                   .Include(h => h.TinNhans.OrderByDescending(t => t.NgayGui))
                        .ThenInclude(t => t.NguoiGuiTinNhan)
                    .Include(h => h.NguoiThamGias)
                        .ThenInclude(tv => tv.ApplicationUser)
                    .FirstOrDefaultAsync(h => h.Id == hopThoaiId &&
                                           h.NguoiThamGias.Any(ng => ng.MaNguoiThamGia == userId));
            }

            CacAdmin cacAdmin = new CacAdmin(_context);

            var viewModel = new ChatViewModel
            {
                HopThoais = conversations.Select(ng => ng.HopThoai).ToList(),
                HopThoaiHienTai = currentConversation,
                AdminId = cacAdmin.IdAdmin(),
            };

            if (nguoiDung.Active == false)
            {

                if (hopThoaiId == null)
                {
                    TempData["WarningMessage"] = "Vui lòng đăng nhập để sử dụng chức năng này.";
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }
                else
                {
                    var isAdmin = await _context.NguoiThamGias
                                            .FirstOrDefaultAsync(i => i.MaHopThoaiTinNhan == hopThoaiId && i.IsAdmin == true);

                    if (isAdmin == null)
                    {
                        TempData["WarningMessage"] = "Chỉ được phép nhắn với admin";
                        return RedirectToAction("Login", "Account", new { area = "Identity" });
                    }
                }

            }

            return View(viewModel);
        }

        // Tạo hộp thoại mới
        public async Task<IActionResult> BatDauChat(string nguoiNhanId)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                TempData["WarningMessage"] = "Vui lòng đăng nhập để sử dụng chức năng này.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            var nguoiDung = await _userManager.GetUserAsync(User);
            //if (nguoiDung.Active == false)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Identity" });
            //}
            var nguoiGuiId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra đã có hộp thoại nào chưa (thêm AsNoTracking)
            var existingConversation = await _context.HopThoaiTinNhans
                .AsNoTracking()
                .Include(h => h.NguoiThamGias)
                .Where(h => !h.IsGroup && h.NguoiThamGias.Count == 2)
                .FirstOrDefaultAsync(h => h.NguoiThamGias.Any(ng => ng.MaNguoiThamGia == nguoiGuiId) &&
                                       h.NguoiThamGias.Any(ng => ng.MaNguoiThamGia == nguoiNhanId));

            if (existingConversation != null)
            {
                return RedirectToAction("Index", new { hopThoaiId = existingConversation.Id });
            }

            // Tạo hộp thoại mới
            var hopThoai = new HopThoaiTinNhan
            {
                TieuDeChat = "Chat riêng",
                NgayTao = DateTime.Now,
                LastMessageTime = DateTime.Now
            };

            _context.HopThoaiTinNhans.Add(hopThoai);
            await _context.SaveChangesAsync();


            CacAdmin cacAdmin = new CacAdmin(_context);
            // Thêm thành viên - SỬA LẠI CÁCH TẠO MỚI Ở ĐÂY
            var thanhVien = new List<NguoiThamGia>
            {
                new NguoiThamGia {
                    MaHopThoaiTinNhan = hopThoai.Id, // Sửa thành MaHopThoaiTinNhan
                    MaNguoiThamGia = nguoiGuiId,
                    NgayThamGia = DateTime.Now
                },
                new NguoiThamGia {
                    MaHopThoaiTinNhan = hopThoai.Id, // Sửa thành MaHopThoaiTinNhan
                    MaNguoiThamGia = nguoiNhanId,
                    NgayThamGia = DateTime.Now,
                    IsAdmin = nguoiNhanId == cacAdmin.IdAdmin() // Kiểm tra nếu người nhận là admin
                }
            };

            // Thêm từng thành viên một để tránh lỗi tracking
            foreach (var tv in thanhVien)
            {
                _context.NguoiThamGias.Add(tv);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { hopThoaiId = hopThoai.Id });
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

        // Gửi tin nhắn
        [HttpPost]
        public async Task<IActionResult> GuiTinNhan(int hopThoaiId, string noiDung, IFormFile? file)
        {
            var nguoiGuiId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            // Kiểm tra người dùng có trong hộp thoại không - SỬA ĐIỀU KIỆN
            var isValid = await _context.NguoiThamGias
                .AnyAsync(ng => ng.MaHopThoaiTinNhan == hopThoaiId && ng.MaNguoiThamGia == nguoiGuiId);

            if (!isValid) return Forbid();

            var tinNhan = new TinNhan
            {
                MaHopThoaiTinNhan = hopThoaiId,
                MaNguoiGui = nguoiGuiId,
                NoiDung = noiDung,
                NgayGui = DateTime.Now
            };

            // Xử lý file đính kèm
            if (file != null && file.Length > 0)
            {
                string url_hinhanh = await SaveImage(file, "AnhTinNhan");
                tinNhan.HinhAnh = url_hinhanh;
            }

            _context.TinNhans.Add(tinNhan);

            // Cập nhật thời gian cập nhật cuối
            var hopThoai = await _context.HopThoaiTinNhans.FindAsync(hopThoaiId);
            hopThoai.LastMessageTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { hopThoaiId });
        }
    }

    
}
