using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.ViewModels
{
    public class TimKiemNangCaoViewModel
    {
        public List<string> DacDiemDaChon { get; set; } = new List<string>();
        public List<DacDiemNhanDangGroup> DacDiemCoTheChon { get; set; } = new List<DacDiemNhanDangGroup>();
        public List<KetQuaTimKiemItem> KetQuaTimKiem { get; set; } = new List<KetQuaTimKiemItem>();
    }
}
