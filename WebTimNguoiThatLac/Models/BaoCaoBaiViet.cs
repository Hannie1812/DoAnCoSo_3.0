using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("BaoCaoBaiViet")]
    public class BaoCaoBaiViet
    {
        [Key]
        public int Id { get; set; }

        public int? MaBaiViet { get; set; } // Bài đăng bị báo cáo
        [ForeignKey("MaBaiViet")]
        public virtual TimNguoi? TimNguoi { get; set; }
        public string? MaNguoiBaoCao { get; set; } // Người báo cáo
        [ForeignKey("MaNguoiBaoCao")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public string? LyDo { get; set; } // Lý do
        public string? ChiTiet { get; set; } // Chi tiết
        public string? HinhAnh { get; set; } // Hình Ảhh
        public DateTime NgayBaoCao { get; set; } = DateTime.Now;
        public bool check { get; set; } = false;
        public bool DaDoc { get; set; } = false; // Mặc định là chưa đọc

    }
}
