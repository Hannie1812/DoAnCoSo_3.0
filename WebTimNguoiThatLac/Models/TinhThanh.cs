using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("TinhThanh")]
    public class TinhThanh
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenTinhThanh { get; set; }

        public ICollection<QuanHuyen> QuanHuyens { get; set; } = new HashSet<QuanHuyen>();
    }
}
