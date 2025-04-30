using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("NhanChung")]
    public class NhanChung
    {
        [Key]
        public int Id { get; set; }

        public string? HoTen { get; set; } // Tên nhân chứng
        public string? Email { get; set; } // Email của nhân chứng (tùy chọn)
        public string? SoDienThoai { get; set; } // Số điện thoại của nhân chứng (tùy chọn)

        public DateTime? ThoiGian { get; set; } // Thời gian nhân chứng nhìn thấy

        public string? DiaDiem { get; set; } // Địa điểm nhìn thấy

        public string? MoTaManhMoi { get; set; } // Nội dung manh mối

        public string? FileDinhKem { get; set; } // Đường dẫn tới file ảnh/video
        public int? TimNguoiId { get; set; } // Liên kết với bảng TimNguoi (missing person)

        [ForeignKey("TimNguoiId")]
        public TimNguoi? TimNguoi { get; set; } // Thông tin người mất tích liên quan
        public DateTime NgayBaoTin { get; set; } = DateTime.Now;

        // so sách cơ sở dữ liệu
        public string? NguoiDungChon  { get; set; }
        public int PhanTramDanhGia { get; set; } = 0;
    }
}