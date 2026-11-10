using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class PhongBanController : Controller
    {
        private readonly HrmDbContext _context;

        public PhongBanController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.PhongBans.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phongBan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phongBan);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound();
            return View(phongBan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PhongBan phongBan)
        {
            if (id != phongBan.SMaPB) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phongBan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhongBanExists(phongBan.SMaPB)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(phongBan);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var phongBan = await _context.PhongBans.FirstOrDefaultAsync(m => m.SMaPB == id);
            if (phongBan == null) return NotFound();
            return View(phongBan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan != null)
            {
                _context.PhongBans.Remove(phongBan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PhongBanExists(string id)
        {
            return _context.PhongBans.Any(e => e.SMaPB == id);
        }
    }
}