using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Extensions;

namespace WebTimNguoiThatLac.Models
{
    [Table("TimNguoi")]
    public class TimNguoi
    {

        public TimNguoi()
        {
            this.AnhTimNguois = new HashSet<AnhTimNguoi>();
            this.BinhLuans = new HashSet<BinhLuan>();
            this.BaoCaoBaiViets = new HashSet<BaoCaoBaiViet>();
            this.TimThayNguoiThatLacs = new HashSet<TimThayNguoiThatLac>();
            this.NhanChungs = new HashSet<NhanChung>();
        }

        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string? HoTen { get; set; }
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string? TieuDe { get; set; }
        [DataType(DataType.Html)]
        [Required(ErrorMessage = "Nội dung mô tả không được để trống")]
        public string? MoTa { get; set; }
        public string DaciemNhanDang { get; set; }
        public int? GioiTinh { get; set; }
        public bool active { get; set; } = false;
        public string? TrangThai { get; set; } = "Đang Tìm Kiếm";// Cần hỗ trợ từ cộng đồng , Đã tìm thấy

        [Required(ErrorMessage = "Địa chỉ cụ thể không được để trống")]
        public string? KhuVuc { get; set; }

        public int? IdTinhThanh { get; set; }
        [ForeignKey("IdTinhThanh")]
        public TinhThanh? TinhThanh { get; set; }

        public int? IdQuanHuyen { get; set; }
        [ForeignKey("IdQuanHuyen")]
        public QuanHuyen? QuanHuyen { get; set; }

        public DateTime NgayDang { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Mối quan hệ không được để trống")]
        public string? MoiQuanHe { get; set; } // Mối quan hệ với người mất tích

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Ngày mất tích không được để trống")]
        [DataType(DataType.Date)]
        public DateTime? NgayMatTich { get; set; }  // Ngày mất tích
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }  // Ngày sinh
        public bool NguoiDangBaiXoa { get; set; } = false; // Người đăng bài Đã Xóa bài viết
        public ICollection<AnhTimNguoi>? AnhTimNguois { get; set; }

        public string? IdNguoiDung { get; set; }
        [ForeignKey("IdNguoiDung")]
        public ApplicationUser? ApplicationUser { get; set; }

        public ICollection<BinhLuan>? BinhLuans { get; set; }

        public string? DonDangKiTrinhBao { get; set; }
        public ICollection<BaoCaoBaiViet> BaoCaoBaiViets { get; set; }
        public ICollection<TimThayNguoiThatLac> TimThayNguoiThatLacs { get; set; }
        public ICollection<NhanChung> NhanChungs { get; set; }


        // Lưu descriptor trung bình
        [Column(TypeName = "varbinary(512)")]
        public byte[]? AverageDescriptorBytes { get; set; }

        [NotMapped]
        public float[]? AverageDescriptor
        {
            get => AverageDescriptorBytes?.ToFloatArray();
            set => AverageDescriptorBytes = value?.ToByteArray();
        }

        public virtual ICollection<FaceDescriptor>? FaceDescriptors { get; set; }

        public void CalculateAverageDescriptor(IEnumerable<float[]> descriptors)
        {
            if (descriptors?.Any() != true)
            {
                AverageDescriptorBytes = null;
                return;
            }

            int length = descriptors.First().Length;
            float[] average = new float[length];

            foreach (var descriptor in descriptors)
            {
                for (int i = 0; i < length; i++)
                {
                    average[i] += descriptor[i];
                }
            }

            for (int i = 0; i < length; i++)
            {
                average[i] /= descriptors.Count();
            }

            AverageDescriptorBytes = average.ToByteArray();
        }

        // Có thể thêm phương thức update khi thêm/xóa ảnh
        public async Task UpdateAverageDescriptorAsync(ApplicationDbContext db)
        {
            var descriptors = await db.AnhTimNguois
                .Where(a => a.IdNguoiCanTim == this.Id && a.FaceDescriptor != null)
                .Select(a => JsonConvert.DeserializeObject<float[]>(a.FaceDescriptor))
                .ToListAsync();

            CalculateAverageDescriptor(descriptors);
        }
    }
}
