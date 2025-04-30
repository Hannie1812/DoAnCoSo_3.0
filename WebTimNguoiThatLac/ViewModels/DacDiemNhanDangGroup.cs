namespace WebTimNguoiThatLac.ViewModels
{
    public class DacDiemNhanDangGroup
    {
        public string DacDiem { get; set; }
        public int SoLuong { get; set; }
        public bool DaChon { get; set; }
        public List<DacDiemNhanDangGroup> DacDiemLienQuan { get; set; } = new List<DacDiemNhanDangGroup>();
    }
}
