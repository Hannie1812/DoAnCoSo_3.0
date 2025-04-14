using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("AnhTimNguoi")]
    public class AnhTimNguoi
    {
        [Key]
        public int Id { get; set; }
        public string HinhAnh { get; set; }
        public int? TrangThai { get; set; }

        public int IdNguoiCanTim { get; set; }
        [ForeignKey("IdNguoiCanTim")] 
        public virtual TimNguoi? TimNguoi { get; set; }



    }
}
