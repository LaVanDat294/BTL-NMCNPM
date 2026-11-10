using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly HrmDbContext _context;

        public BaoCaoController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalNhanVien = await _context.NhanViens.CountAsync();
            ViewBag.TotalPhongBan = await _context.PhongBans.CountAsync();
            ViewBag.TotalChucVu = await _context.ChucVus.CountAsync();
            ViewBag.TotalHopDong = await _context.HopDongs.CountAsync();

            var luongs = await _context.Luongs.ToListAsync();
            ViewBag.TongQuyLuong = luongs.Sum(l => l.FThucLinh);

            var expiringContracts = await _context.HopDongs
                .Include(h => h.NhanVien)
                .Where(h => h.DNgayHetHan.HasValue && h.DNgayHetHan.Value <= DateTime.Now.AddDays(30) && h.DNgayHetHan.Value >= DateTime.Now)
                .ToListAsync();

            ViewBag.ExpiringContracts = expiringContracts;

            return View();
        }
    }
}
