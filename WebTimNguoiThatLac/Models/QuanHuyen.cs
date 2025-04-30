using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("QuanHuyen")]
    public class QuanHuyen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenQuanHuyen { get; set; }

        [Required]
        public int IdTinhThanh { get; set; }

        [ForeignKey("IdTinhThanh")]
        public TinhThanh TinhThanh { get; set; }

        public ICollection<TimNguoi> TimNguois { get; set; } = new HashSet<TimNguoi>();
    }
}
