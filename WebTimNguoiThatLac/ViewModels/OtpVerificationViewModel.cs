using System.ComponentModel.DataAnnotations;

namespace WebTimNguoiThatLac.ViewModels
{
    public class OtpVerificationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; }

        public bool IsVerified { get; set; }
    }
}