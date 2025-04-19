using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Services
{
    public class OtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public OtpService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<string> GenerateAndSendOtpAsync(string email)
        {
            // Tạo mã OTP ngẫu nhiên 6 chữ số
            var otp = GenerateOtp();

            // Lưu thông tin OTP vào database
            var otpVerification = new OtpVerification
            {
                Email = email,
                OtpCode = otp
            };

            _context.OtpVerifications.Add(otpVerification);
            await _context.SaveChangesAsync();

            // Gửi email chứa mã OTP
            var subject = "Mã xác thực OTP - Hệ thống Tìm người thất lạc";
            var message = $"<p>Mã xác thực OTP của bạn là: <strong>{otp}</strong></p>" +
                         "<p>Mã này sẽ hết hạn sau 5 phút.</p>" +
                         "<p>Vui lòng không chia sẻ mã này với bất kỳ ai.</p>";

            await _emailService.SendEmailAsync(email, subject, message);

            return otp;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var verification = await _context.OtpVerifications
                .Where(v => v.Email == email && v.OtpCode == otp && !v.IsUsed)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verification == null)
                return false;

            if (DateTime.UtcNow > verification.ExpiresAt)
                return false;

            // Đánh dấu OTP đã được sử dụng
            verification.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateOtp()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var random = BitConverter.ToUInt32(bytes, 0);
                return (random % 1000000).ToString("D6"); // Tạo mã 6 chữ số
            }
        }
    }
}