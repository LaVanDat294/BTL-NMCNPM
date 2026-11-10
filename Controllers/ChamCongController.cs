using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class ChamCongController : Controller
    {
        private readonly HrmDbContext _context;

        public ChamCongController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hrmDbContext = _context.ChamCongs.Include(c => c.NhanVien);
            return View(await hrmDbContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChamCong chamCong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chamCong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", chamCong.FK_sMaNV);
            return View(chamCong);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var chamCong = await _context.ChamCongs.FindAsync(id);
            if (chamCong == null) return NotFound();
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", chamCong.FK_sMaNV);
            return View(chamCong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ChamCong chamCong)
        {
            if (id != chamCong.SMaChamCong) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chamCong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChamCongExists(chamCong.SMaChamCong)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", chamCong.FK_sMaNV);
            return View(chamCong);
        }

        private bool ChamCongExists(string id)
        {
            return _context.ChamCongs.Any(e => e.SMaChamCong == id);
        }
    }
}