using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    public class NhanChung
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; } // Tên nhân chứng

        [EmailAddress]
        public string? Email { get; set; } // Email của nhân chứng (tùy chọn)

        [Phone]
        public string? SoDienThoai { get; set; } // Số điện thoại của nhân chứng (tùy chọn)

        [Required]
        public DateTime ThoiGian { get; set; } // Thời gian nhân chứng nhìn thấy

        [Required]
        [MaxLength(200)]
        public string DiaDiem { get; set; } // Địa điểm nhìn thấy

        [Required]
        public string MoTaManhMoi { get; set; } // Nội dung manh mối

        public string? FileDinhKem { get; set; } // Đường dẫn tới file ảnh/video

        [Required]
        public int TimNguoiId { get; set; } // Liên kết với bảng NguoiMatTich

        [ForeignKey("Id")]
        public TimNguoi TimNguoi { get; set; } // Thông tin người mất tích liên quan
    }
}