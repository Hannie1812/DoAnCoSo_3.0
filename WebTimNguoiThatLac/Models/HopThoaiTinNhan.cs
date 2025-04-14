using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("HopThoaiTinNhan")]
    public class HopThoaiTinNhan
    {
        public HopThoaiTinNhan()
        {
            this.TinNhans = new HashSet<TinNhan>();
            this.NguoiThamGias = new HashSet<NguoiThamGia>();
        }

        [Key]
        public int Id { get; set; }
        public string? TieuDeChat { get; set; } // Tiêu đề nhóm chat
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool IsGroup { get; set; } = false;
        public DateTime? LastMessageTime { get; set; } // Thêm để sắp xếp
        public virtual ICollection<TinNhan> TinNhans { get; set; }
        public virtual ICollection<NguoiThamGia> NguoiThamGias { get; set; }
    }
}
