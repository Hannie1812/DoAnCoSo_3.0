using System.ComponentModel.DataAnnotations;

namespace WebTimNguoiThatLac.ThuMucTimKiem
{
    public class FaceSearchRequest
    {
        [Required]
        [MinLength(128)]
        [MaxLength(128)]
        public float[] Descriptor { get; set; }
    }
}
