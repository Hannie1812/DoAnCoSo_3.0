using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.ViewModels
{
    public class KetQuaTimKiemItem
    {
        public TimNguoi BaiDang { get; set; }
        public double PhanTramKhop { get; set; }
        public List<AnhTimNguoi> AnhDaiDien { get; set; } = new List<AnhTimNguoi>();
    }
}
