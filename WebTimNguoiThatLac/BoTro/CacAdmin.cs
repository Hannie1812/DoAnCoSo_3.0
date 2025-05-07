using WebTimNguoiThatLac.Data;

namespace WebTimNguoiThatLac.BoTro
{
    public class CacAdmin
    {
        private ApplicationDbContext _context;
        public CacAdmin(ApplicationDbContext context)
        {
            _context = context;
        }

        public string IdAdmin()
        {
           
            var ad = _context.Users.FirstOrDefault(i => i.IsAdmin);
            if (ad == null)
            {
                return "";
            }
            return ad.Id;
        }
    }
}
