using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
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
        private const int MinimumWidth = 800;
        private const int MinimumHeight = 600;

        public OCRController(IConfiguration configuration)
        {
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), configuration["Tesseract:TessDataPath"]);
        }

        [HttpPost]
        public IActionResult ExtractCCCD(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Vui lòng chọn một ảnh hợp lệ.");

            try
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest("Chỉ chấp nhận file ảnh JPG, JPEG, PNG.");

                // Đọc ảnh từ stream
                using var inputStream = imageFile.OpenReadStream();
                using var originalImage = new Bitmap(inputStream);

                // Tiền xử lý ảnh trực tiếp trên memory
                using var preprocessedImage = PreprocessImage(originalImage);

                // Lưu ảnh đã xử lý vào memory stream để OCR
                using var ms = new MemoryStream();
                preprocessedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                ms.Position = 0;

                string extractedText;
                try
                {
                    using var ocrEngine = new TesseractEngine(_tessDataPath, "vie", EngineMode.Default);
                    using var img = Pix.LoadFromMemory(ms.ToArray());
                    using var page = ocrEngine.Process(img);
                    extractedText = page.GetText();
                }
                catch (Exception ocrEx)
                {
                    return StatusCode(500, $"Lỗi OCR: {ocrEx.Message}");
                }

                // Xử lý văn bản trích xuất
                if (string.IsNullOrWhiteSpace(extractedText))
                    return BadRequest("Không thể đọc nội dung từ ảnh. Vui lòng thử với ảnh rõ nét hơn.");

                string cleanedText = Regex.Replace(extractedText, @"\s+", "");
                string cccd = Regex.Match(cleanedText, @"\d{12}").Value;

                if (string.IsNullOrEmpty(cccd))
                {
                    var digitMatches = Regex.Matches(cleanedText, @"\d{11,13}");
                    cccd = digitMatches.Cast<Match>()
                        .Select(m => m.Value)
                        .FirstOrDefault(v => v.Length == 12);
                }

                if (string.IsNullOrEmpty(cccd))
                    return BadRequest("Không thể nhận dạng thông tin từ ảnh.");

                return Ok(new { CCCD = cccd });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }


        private Bitmap PreprocessImage(Bitmap originalImage)
        {
            // Resize nếu ảnh quá nhỏ
            var resizedImage = originalImage.Width < MinimumWidth || originalImage.Height < MinimumHeight
                ? new Bitmap(originalImage, new Size(
                    Math.Max(originalImage.Width * 2, MinimumWidth),
                    Math.Max(originalImage.Height * 2, MinimumHeight)))
                : new Bitmap(originalImage);

            // Chuyển sang grayscale
            using var grayImage = ToGrayscale(resizedImage);

            // Tăng contrast
            using var highContrast = AdjustContrast(grayImage, 1.5f);

            // Nhị phân hoá
            var finalImage = ApplyThreshold(highContrast, 180);

            return finalImage;
        }

        private Bitmap ToGrayscale(Bitmap original)
        {
            var grayscale = new Bitmap(original.Width, original.Height);
            using (var g = Graphics.FromImage(grayscale))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                    new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                    new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original,
                        new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height,
                        GraphicsUnit.Pixel, attributes);
                }
            }
            return grayscale;
        }

        private Bitmap AdjustContrast(Bitmap image, float contrast)
        {
            var adjustedImage = new Bitmap(image.Width, image.Height);
            contrast = Math.Max(0, contrast); // Đảm bảo contrast không âm

            using (var g = Graphics.FromImage(adjustedImage))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {contrast, 0, 0, 0, 0},
                    new float[] {0, contrast, 0, 0, 0},
                    new float[] {0, 0, contrast, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0.001f, 0.001f, 0.001f, 0, 1}
                });

                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(image,
                        new Rectangle(0, 0, image.Width, image.Height),
                        0, 0, image.Width, image.Height,
                        GraphicsUnit.Pixel, attributes);
                }
            }
            return adjustedImage;
        }

        private Bitmap ApplyThreshold(Bitmap image, int threshold)
        {
            var binaryImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int intensity = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    Color newColor = intensity >= threshold ? Color.White : Color.Black;
                    binaryImage.SetPixel(x, y, newColor);
                }
            }

            return binaryImage;
        }

        /*private string ExtractFullName(string text)
        {
            // Giữ nguyên logic trích xuất họ tên như trước
            var nameMatch = Regex.Match(text,
                @"(?:H[oòọõó]?[ ]*v[àa]?[ ]*t[êeèéẹẻẽ]?[nơ]?|T[eêèéẹẻẽ]?n|Full[ ]*Name)[:\s]*([^\n]+)",
                RegexOptions.IgnoreCase);

            if (nameMatch.Success)
            {
                string candidate = nameMatch.Groups[1].Value.Trim();
                if (candidate.Count(c => char.IsUpper(c)) >= 2)
                    return candidate;
            }

            return text.Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length >= 5 && line.Length <= 50)
                .OrderByDescending(line => line.Count(c => char.IsUpper(c)))
                .ThenByDescending(line => line.Length)
                .FirstOrDefault() ?? "Không tìm thấy";
        }*/
    }
}