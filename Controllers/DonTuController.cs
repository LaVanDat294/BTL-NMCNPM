using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;
using NMCNPM.Filters;
using System.Security.Claims;

namespace NMCNPM.Controllers
{
    public class DonTuController : Controller
    {
        private readonly HrmDbContext _context;

        public DonTuController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userMaNV = User.FindFirst("MaNV")?.Value;
            var userRole = User.FindFirst("MaQuyen")?.Value;

            IQueryable<DonTu> query = _context.DonTus.Include(d => d.NhanVien);

            // Nếu là Staff thì chỉ xem đơn của chính mình
            if (userRole == "Q_STAFF" && !string.IsNullOrEmpty(userMaNV))
            {
                query = query.Where(d => d.FK_sMaNV == userMaNV);
            }

            return View(await query.ToListAsync());
        }

        public IActionResult TaoDon()
        {
            var userMaNV = User.FindFirst("MaNV")?.Value;
            if (!string.IsNullOrEmpty(userMaNV))
            {
                ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens.Where(n => n.SMaNV == userMaNV), "SMaNV", "STenNV", userMaNV);
            }
            else
            {
                ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoDon(DonTu donTu)
        {
            if (ModelState.IsValid)
            {
                donTu.DNgayGui = DateTime.Now;
                donTu.STrangThai = "Chờ duyệt";
                _context.Add(donTu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", donTu.FK_sMaNV);
            return View(donTu);
        }

        [AuthorizeRole("Q_ADMIN", "Q_MANAGER")]
        public async Task<IActionResult> PheDuyet(string id)
        {
            if (id == null) return NotFound();
            var donTu = await _context.DonTus.Include(d => d.NhanVien).FirstOrDefaultAsync(m => m.SMaDon == id);
            if (donTu == null) return NotFound();
            return View(donTu);
        }

        [HttpPost]
        [AuthorizeRole("Q_ADMIN", "Q_MANAGER")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PheDuyet(string id, string trangThai)
        {
            var donTu = await _context.DonTus.FindAsync(id);
            if (donTu == null) return NotFound();

            var currentMaNV = User.FindFirst("MaNV")?.Value ?? User.Identity?.Name ?? "Admin";

            donTu.STrangThai = trangThai;
            donTu.SNguoiDuyet = currentMaNV;
            _context.Update(donTu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}