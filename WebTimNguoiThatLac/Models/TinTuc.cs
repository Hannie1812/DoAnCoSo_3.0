using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DocumentFormat.OpenXml.Spreadsheet;

namespace WebTimNguoiThatLac.Models
{
    [Table("TinTuc")]
    public class TinTuc
    {
        [Key]
        public int Id { get; set; }
        public string? TieuDe { get; set; }

        [MaxLength(100, ErrorMessage ="KHÔNG ĐƯỢC QUÁ 100 KÍ TỰ")]
        public string? MoTaNgan { get; set; }
        public string? NoiDung { get; set; }
        public string? HinhAnh { get; set; }
        public bool Active { get; set; } = false;
        public string? TacGia { get; set; }
        public DateTime NgayDang { get; set; } = DateTime.Now;
    }
}
