using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("HanhViDangNgo")]
    public class HanhViDangNgo // Ghi log người dùng
    {
        [Key]
        public int Id { get; set; }
        public string? NguoiDungId { get; set; }
        [ForeignKey("NguoiDungId")]
        public ApplicationUser? ApplicationUser { get; set; }
        public string? HanhDong { get; set; }
        public DateTime? ThoiGian { get; set; }
        public string? ChiTiet { get; set; }
        public bool KhangNghi { get; set; } = false; // Mặc định là không kháng nghị
        public bool DaXem { get; set; } = false; // Mặc định là chưa xem
        public bool DaXuLy { get; set; } = false; // Mặc định là chưa xử lý
        public string? TrangThaiKhangNghi { get; set; }  // Kháng Nghị Thất Bại, Kháng Nghị Thành Công
        public string LoaiViPham { get; set; } = "Người Dùng"; // Loại hành vi: Bình Luận , Bài Viết , người dùng
        public int IdLoiViPham { get; set; } = -1; // id của mấy cái bài viết , Bình luận

    }

}
