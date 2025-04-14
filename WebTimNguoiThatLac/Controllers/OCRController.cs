using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Tesseract;

namespace WebTimNguoiThatLac.Controllers
{
    public class OCRController : Controller
    {
        private readonly string _tessDataPath;

        public OCRController(IConfiguration configuration)
        {
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), configuration["Tesseract:TessDataPath"]);
        }

        [HttpPost]
        public IActionResult ExtractCCCD(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Vui lòng chọn một ảnh hợp lệ.");
            }

            try
            {
                // Kiểm tra định dạng file hợp lệ
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    return BadRequest("Chỉ chấp nhận file ảnh JPG, JPEG, PNG.");
                }

                // Tạo thư mục lưu ảnh nếu chưa có
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/CCCD_NguoiDung");
                Directory.CreateDirectory(uploadDir);

                // Lưu ảnh với tên file ngẫu nhiên (Guid) để tránh trùng lặp
                string fileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                // Khởi tạo OCR Engine
                using var ocrEngine = new TesseractEngine(_tessDataPath, "vie", EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = ocrEngine.Process(img);
                string extractedText = page.GetText();

                // Trích xuất số CCCD (12 số) và Họ Tên
                string cccd = Regex.Match(extractedText, @"\b\d{12}\b").Value;
                string fullName = Regex.Match(extractedText, @"(?:Họ và tên|Tên|Full Name)[:\s]*([A-ZÀ-Ỹ][A-Za-zÀ-ỹ\s]*)", RegexOptions.IgnoreCase).Groups[1].Value;

                // Xóa ảnh sau khi xử lý để tiết kiệm dung lượng
                System.IO.File.Delete(filePath);

                if (string.IsNullOrEmpty(cccd))
                {
                    return BadRequest("Không tìm thấy số CCCD. Vui lòng nhập thủ công.");
                }

                return Ok(new
                {
                    CCCD = cccd,
                    HoTen = string.IsNullOrEmpty(fullName) ? "Không tìm thấy" : fullName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi OCR: {ex.Message}");
            }
        }
    }
}
