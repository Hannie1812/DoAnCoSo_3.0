using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;
using WebTimNguoiThatLac.Services;

namespace WebTimNguoiThatLac.Controllers
{
    // Controller API
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private ApplicationDbContext db;
        public PostsController(ApplicationDbContext db){ 
            this.db = db;
            
        }
        [HttpGet("with-images")]
        public async Task<IActionResult> GetPostsWithImages()
        {
            var posts  = await db.TimNguois
                    .AsNoTracking() // Tăng performance
                    .Where(t => t.active)
                    .Include(t => t.AnhTimNguois)
                    .Select(t => new {
                        t.Id,
                        t.HoTen,
                        t.TieuDe,
                        t.MoTa,
                        DacDiem = t.DaciemNhanDang,
                        t.NgayDang,
                        Images = t.AnhTimNguois.Select(a => new {
                            Url = a.HinhAnh,
                            IsMain = a.TrangThai == 1
                        }).ToList()
                    })
                    .ToListAsync();

            return Ok(posts);
        }
    }
}
