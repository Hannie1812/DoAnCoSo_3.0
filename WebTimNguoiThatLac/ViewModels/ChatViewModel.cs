using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.ViewModels
{
    public class ChatViewModel
    {
        public List<HopThoaiTinNhan> HopThoais { get; set; }
        public HopThoaiTinNhan HopThoaiHienTai { get; set; }
        public string AdminId { get; set; }
    }
}
