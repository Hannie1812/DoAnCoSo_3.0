using System;
using System.ComponentModel.DataAnnotations;

namespace WebTimNguoiThatLac.Models
{
    public class OtpVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OtpCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public OtpVerification()
        {
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = CreatedAt.AddMinutes(5); // OTP hết hạn sau 5 phút
            IsUsed = false;
        }
    }
}