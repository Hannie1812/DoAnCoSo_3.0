using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebTimNguoiThatLac.Data;
using WebTimNguoiThatLac.Models;

namespace WebTimNguoiThatLac.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HopThoaiTinNhan> TaoHopThoaiMoi(string userId, string otherUserId, bool isGroup = false)
        {
            var hopThoai = new HopThoaiTinNhan
            {
                TieuDeChat = isGroup ? "Nhóm chat mới" : "Chat riêng",
                IsGroup = isGroup,
                NgayTao = DateTime.Now
            };

            _context.HopThoaiTinNhans.Add(hopThoai);
            await _context.SaveChangesAsync();

            // Thêm người tham gia
            var thanhVien = new List<NguoiThamGia>
            {
                new NguoiThamGia { MaHopThoaiTinNhan = hopThoai.Id, MaNguoiThamGia = userId },
                new NguoiThamGia { MaHopThoaiTinNhan = hopThoai.Id, MaNguoiThamGia = otherUserId }
            };

            _context.NguoiThamGias.AddRange(thanhVien);
            await _context.SaveChangesAsync();

            return hopThoai;
        }

        public async Task<List<HopThoaiTinNhan>> LayDanhSachHopThoai(string userId)
        {
            return await _context.NguoiThamGias
                .Where(ng => ng.MaNguoiThamGia == userId)
                .Include(ng => ng.HopThoai)
                    .ThenInclude(h => h.TinNhans.OrderByDescending(t => t.NgayGui).Take(1))
                .Include(ng => ng.HopThoai)
                    .ThenInclude(h => h.NguoiThamGias)
                        .ThenInclude(tv => tv.ApplicationUser)
                .OrderByDescending(ng => ng.HopThoai.LastMessageTime ?? ng.HopThoai.NgayTao)
                .Select(ng => ng.HopThoai)
                .ToListAsync();
        }
    }
}