using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class ChucVuController : Controller
    {
        private readonly HrmDbContext _context;

        public ChucVuController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ChucVus.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chucVu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chucVu);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var chucVu = await _context.ChucVus.FindAsync(id);
            if (chucVu == null) return NotFound();
            return View(chucVu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ChucVu chucVu)
        {
            if (id != chucVu.SMaCV) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chucVu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChucVuExists(chucVu.SMaCV)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chucVu);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var chucVu = await _context.ChucVus.FirstOrDefaultAsync(m => m.SMaCV == id);
            if (chucVu == null) return NotFound();
            return View(chucVu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var chucVu = await _context.ChucVus.FindAsync(id);
            if (chucVu != null)
            {
                _context.ChucVus.Remove(chucVu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ChucVuExists(string id)
        {
            return _context.ChucVus.Any(e => e.SMaCV == id);
        }
    }
}