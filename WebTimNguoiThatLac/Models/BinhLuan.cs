using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("BinhLuan")]
    public class BinhLuan
    {
        public BinhLuan()
        {
            this.BaoCaoBinhLuans = new HashSet<BaoCaoBinhLuan>();
        }
        [Key]
        public int Id { get; set; }

        public string NoiDung  { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayBinhLuan { get; set; } = DateTime.Now;
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        // Thêm trường cho bình luận cha (nếu là reply)
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual BinhLuan? ParentBinhLuan { get; set; }

        public int? IdBaiViet { get; set; }
        [ForeignKey("IdBaiViet")]
        public virtual TimNguoi? TimNguoi { get; set; }
        public bool Active { get; set; } = true; // Mặc định là hirnt thị
        public bool DaDoc { get; set; } = false; // Mặc định là chưa đọc
        public bool NguoiDangBaiXoa { get; set; } = false; // Mặc định là chuaxoa
        public ICollection<BaoCaoBinhLuan> BaoCaoBinhLuans { get; set; }
        public ICollection<BinhLuan> TraLois { get; set; } // Danh sách các reply
    }
}
