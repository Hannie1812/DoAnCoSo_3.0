using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTimNguoiThatLac.Models
{
    [Table("TimNguoi")]
    public class TimNguoi
    {
        
        public  TimNguoi()
        {
            this.AnhTimNguois = new HashSet<AnhTimNguoi>();
            this.BinhLuans = new HashSet<BinhLuan>();
            this.BaoCaoBaiViets = new HashSet<BaoCaoBaiViet>();
            this.TimThayNguoiThatLacs = new HashSet<TimThayNguoiThatLac>();
            this.NhanChungs = new HashSet<NhanChung>();
        }

        [Key]
        public int Id { get; set; }
        public string? HoTen { get; set; }
        public string? TieuDe { get; set; }
        [DataType(DataType.Html)]
        public string? MoTa { get; set; }
        public string DaciemNhanDang { get; set; } 
        public int? GioiTinh { get; set; }
        public bool active { get; set; } = false;
        public string? TrangThai { get; set; } = "Đang Tìm Kiếm";// Cần hỗ trợ từ cộng đồng , Đã tìm thấy
        public string? KhuVuc { get; set; }

        public int? IdTinhThanh { get; set; }
        [ForeignKey("IdTinhThanh")]
        public TinhThanh? TinhThanh { get; set; }

        public int? IdQuanHuyen { get; set; }
        [ForeignKey("IdQuanHuyen")]
        public QuanHuyen? QuanHuyen { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.Now;
        public string? MoiQuanHe { get; set; } // Mối quan hệ với người mất tích
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NgayMatTich { get; set; }  // Ngày mất tích
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NgaySinh { get; set; }  // Ngày sinh
        public bool NguoiDangBaiXoa { get; set; } = false; // Người đăng bài Đã Xóa bài viết
        public  ICollection<AnhTimNguoi>? AnhTimNguois { get; set; }

        public string? IdNguoiDung { get; set; }
        [ForeignKey("IdNguoiDung")]
        public  ApplicationUser? ApplicationUser { get; set; }

        public  ICollection<BinhLuan>? BinhLuans { get; set; }

        public string? DonDangKiTrinhBao { get; set; }
        public ICollection<BaoCaoBaiViet> BaoCaoBaiViets { get; set; }
        public ICollection<TimThayNguoiThatLac> TimThayNguoiThatLacs { get; set; }
        public ICollection<NhanChung> NhanChungs { get; set; }





    }
}
