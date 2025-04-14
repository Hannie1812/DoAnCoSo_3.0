using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("TrungBayHinhAnh")]
    public class TrungBayHinhAnh
    {
        [Key]
        public int Id { get; set; }
        public string? HinhAnh { get; set; }
        public bool Active { get; set; } = false;
    }
}
