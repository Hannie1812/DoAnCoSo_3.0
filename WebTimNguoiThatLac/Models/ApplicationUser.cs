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
        public string? CCCD { get; set; }

        public string? HinhAnh { get; set; }

        public virtual ICollection<TimNguoi>? TimNguois { get; set; }
        public virtual ICollection<BinhLuan>? BinhLuans { get; set; }
        public ICollection<BaoCaoBaiViet> BaoCaoBaiViets { get; set; }
        public ICollection<BaoCaoBinhLuan> BaoCaoBinhLuans { get; set; }
        public ICollection<NguoiThamGia> NguoiThamGias { get; set; }

    }
}
