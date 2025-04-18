using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Repositories
{
    public class EFTimNguoiRepository : ITimNguoiRepository
    {
        private readonly ApplicationDbContext _context;
        public EFTimNguoiRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TimNguoi>> GetAllTimNguoiAsync()
        {
            return await _context.TimNguois.ToListAsync();
        }
        public async Task<TimNguoi> GetTimNguoiByIdAsync(int id)
        {
            return await _context.TimNguois.FindAsync(id);
        }
    }
}
