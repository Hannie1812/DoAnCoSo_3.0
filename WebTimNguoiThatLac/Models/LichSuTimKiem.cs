using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("LichSuTimKiem")]
    public class LichSuTimKiem // Ghi log người dùng
    {
        [Key]
        public int Id { get; set; }
        public string? IdNguoiDung { get; set; }
        [ForeignKey("IdNguoiDung")]
        public ApplicationUser? ApplicationUser { get; set; }
        public string? TuKhoa { get; set; }
        public DateTime ThoiGianTimKiem { get; set; } = DateTime.Now;
        public string? DiaChiIP { get; set; }
    }
}
