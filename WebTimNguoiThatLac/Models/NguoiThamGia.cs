using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebTimNguoiThatLac.Models
{
    [Table("NguoiThamGia")]
    public class NguoiThamGia
    {
        [Key]
        public int Id { get; set; }

        public int? MaHopThoaiTinNhan { get; set; }
        [ForeignKey("MaHopThoaiTinNhan")]
        public virtual HopThoaiTinNhan? HopThoai { get; set; }

        public string? MaNguoiThamGia { get; set; }
        [ForeignKey("MaNguoiThamGia")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public DateTime NgayThamGia { get; set; } = DateTime.Now;

        public bool IsAdmin { get; set; } = false; // Thêm để phân quyền
    }
}
