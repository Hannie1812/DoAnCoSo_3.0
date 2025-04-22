using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebTimNguoiThatLac.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() 
        {
            this.TimNguois = new HashSet<TimNguoi>();
            this.BinhLuans = new HashSet<BinhLuan>();
            this.BaoCaoBaiViets = new HashSet<BaoCaoBaiViet>();
            this.BaoCaoBinhLuans = new HashSet<BaoCaoBinhLuan>();
            this.NguoiThamGias = new HashSet<NguoiThamGia>();
        }

        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? NgaySinh { get; set; }
        public bool Active { get; set; } = true;
        private string? _cccd;
        public string? CCCD
        {
            get => _cccd == null ? null : WebTimNguoiThatLac.BoTro.Filter.DecryptCCCD(_cccd);
            set => _cccd = value == null ? null : WebTimNguoiThatLac.BoTro.Filter.EncryptCCCD(value);
        }
        public string? HinhAnh { get; set; }
        public int SoLanViPham { get; set; } = 0; // 👉 Đếm số lần vi phạm

        public bool IsAdmin { get; set; } = false; // 👉 Thêm để phân quyền
        public virtual ICollection<TimNguoi>? TimNguois { get; set; }
        public virtual ICollection<BinhLuan>? BinhLuans { get; set; }
        public virtual ICollection<BaoCaoBaiViet>? BaoCaoBaiViets { get; set; }
        public virtual ICollection<BaoCaoBinhLuan>? BaoCaoBinhLuans { get; set; }
        public virtual ICollection<NguoiThamGia>? NguoiThamGias { get; set; }
        public virtual ICollection<LichSuTimKiem>? LichSuTimKiems { get; set; }
        public virtual ICollection<HanhViDangNgo>? HanhViDangNgos { get; set; }

    }
}
