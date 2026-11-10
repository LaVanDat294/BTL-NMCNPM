using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

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
            var hrmDbContext = _context.DonTus.Include(d => d.NhanVien);
            return View(await hrmDbContext.ToListAsync());
        }

        public IActionResult TaoDon()
        {
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV");
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

        public async Task<IActionResult> PheDuyet(string id)
        {
            if (id == null) return NotFound();
            var donTu = await _context.DonTus.Include(d => d.NhanVien).FirstOrDefaultAsync(m => m.SMaDon == id);
            if (donTu == null) return NotFound();
            return View(donTu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PheDuyet(string id, string trangThai, string nguoiDuyet)
        {
            var donTu = await _context.DonTus.FindAsync(id);
            if (donTu == null) return NotFound();

            donTu.STrangThai = trangThai;
            donTu.SNguoiDuyet = nguoiDuyet;
            _context.Update(donTu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}