using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebTimNguoiThatLac.Models
{
    [Table("NguoiDungLienHe")]
    public class NguoiDungLienHe
    {
        [Key]
        public int MaLienHeNguoiDung { get; set; }
        public string? TenNguoiDungLienHe { get; set; }
        public string? EmailNguoiDungLienHe { get; set; }
        public string? VanDeLienHe { get; set; }
        [DataType(DataType.Html)]
        public string? NoiBungLienHe { get; set; }
        public string? PhoneNguoiDungLienHe { get; set; }
        public DateTime NgayLienHe { get; set; } = DateTime.Now;
        public bool isRead { get; set; } = false;
    }
}
