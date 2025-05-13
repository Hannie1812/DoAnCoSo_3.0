using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;

namespace WebTimNguoiThatLac.Models
{
    [Table("FaceDescriptor")]
    public class FaceDescriptor
    {
        [Key]
        public int Id { get; set; }
        public float[] Descriptor { get; set; } // Mảng 128 số
        public int? IDBaiViet { get; set; }
        [ForeignKey("IDBaiViet")]
        public TimNguoi? TimNguoi { get; set; }
    }
}
