using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("TimThayNguoiThatLac")]
    public class TimThayNguoiThatLac
    {
        [Key]
        public int Id { get; set; }

        // Thông tin liên kết với đơn báo mất tích ban đầu
        public int? TimNguoiId { get; set; }
        [ForeignKey("TimNguoiId")]
        public virtual TimNguoi? TimNguoi { get; set; }

        // Thông tin người làm đơn báo đã tìm thấy
        public string? IdNguoiLamDon { get; set; }
        [ForeignKey("IdNguoiLamDon")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        // Thông tin về việc tìm thấy
        public DateTime? NgayTimThay { get; set; } 

        public TimeSpan? GioTimThay { get; set; }

        public string? DiaDiemTimThay { get; set; }

        public string? NguoiTimThay { get; set; } // Có thể là cá nhân/tổ chức nào tìm thấy

        public string? TinhTrangSucKhoe { get; set; } // Ví dụ: "Ổn định", "Bị thương", "Bệnh nặng"

        public bool DaDuaVeGiaDinh { get; set; } = true;

        public string? HienDangO { get; set; } // Nếu chưa đưa về gia đình
        [DataType(DataType.Html)]
        public string? MoTaChiTiet { get; set; } // Mô tả quá trình tìm thấy, tình trạng...
        public bool HoanTatHoSoPhapLy { get; set; } = false;
        public string? AnhMinhChung { get; set; } // Đường dẫn ảnh

        public string? NguoiXacNhan { get; set; } // Cán bộ/cơ quan xác nhận

        public string? SoDienThoaiNguoiXacNhan { get; set; }

        public string? GhiChu { get; set; }

        // Các thông tin hệ thống
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
        public string? HoTen { get; set; }
        public string? DonXacNhanTimThay { get; set; }


    }
}