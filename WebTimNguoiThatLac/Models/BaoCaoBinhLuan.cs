using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("BaoCaoBinhLuan") ]
    public class BaoCaoBinhLuan
    {
        [Key]
        public int Id { get; set; }
        public int? MaBinhLuan { get; set; }
        [ForeignKey("MaBinhLuan")]
        public virtual BinhLuan? BinhLuan { get; set; }
        public string? MaNguoiBaoCao { get; set; } // Người báo cáo
        [ForeignKey("MaNguoiBaoCao")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public string? LyDo { get; set; } // Lý do
        public DateTime NgayBaoCao { get; set; } = DateTime.Now;
        public bool check { get; set; } = false;
        public bool DaDoc { get; set; } = false; // Mặc định là chưa đọc
    }
}
