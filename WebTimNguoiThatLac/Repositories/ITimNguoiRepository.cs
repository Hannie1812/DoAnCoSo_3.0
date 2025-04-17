using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Repositories
{
    public interface ITimNguoiRepository
    {
        Task<IEnumerable<TimNguoi>> GetAllTimNguoiAsync();
        Task<TimNguoi> GetTimNguoiByIdAsync(int id);
    }
}
