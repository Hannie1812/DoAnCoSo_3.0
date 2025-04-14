using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("GioiThieu")]
    public class GioiThieu
    {
        [Key]
        public int Id { get; set; }
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }
        public string? HinhAnh { get; set; }
        public bool Active { get; set; } = false;
    }
}
