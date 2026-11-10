using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class HopDongController : Controller
    {
        private readonly HrmDbContext _context;

        public HopDongController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hrmDbContext = _context.HopDongs.Include(h => h.NhanVien);
            return View(await hrmDbContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HopDongLaoDong hopDong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hopDong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", hopDong.FK_sMaNV);
            return View(hopDong);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong == null) return NotFound();
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", hopDong.FK_sMaNV);
            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, HopDongLaoDong hopDong)
        {
            if (id != hopDong.SMaHD) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hopDong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HopDongExists(hopDong.SMaHD)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", hopDong.FK_sMaNV);
            return View(hopDong);
        }

        private bool HopDongExists(string id)
        {
            return _context.HopDongs.Any(e => e.SMaHD == id);
        }
    }
}