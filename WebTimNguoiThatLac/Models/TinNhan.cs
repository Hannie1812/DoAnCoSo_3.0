using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebTimNguoiThatLac.Models
{
    [Table("TinNhan")]
    public class TinNhan
    {
        [Key]
        public int Id { get; set; }

        public int? MaHopThoaiTinNhan { get; set; }
        [ForeignKey("MaHopThoaiTinNhan")]
        public virtual HopThoaiTinNhan? HopThoaiTinNhan { get; set; }

        public string? MaNguoiGui { get; set; }
        [ForeignKey("MaNguoiGui")]
        public virtual ApplicationUser? NguoiGuiTinNhan { get; set; }

        public string? NoiDung { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        // Cho phép đính kèm hình ảnh
        public string? HinhAnh { get; set; }
        
    }
}
