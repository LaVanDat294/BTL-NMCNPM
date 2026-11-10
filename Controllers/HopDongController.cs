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
            var danhSach = _context.HopDongs.Include(h => h.NhanVien);
            return View(await danhSach.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var hopDong = await _context.HopDongs
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(h => h.SMaHD == id);

            if (hopDong == null) return NotFound();

            return View(hopDong);
        }

        public IActionResult Create()
        {
            ViewData["FK_sMaNV"] = new SelectList(
                _context.NhanViens, "SMaNV", "STenNV");

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

                TempData["Success"] = "Thêm hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["FK_sMaNV"] = new SelectList(
                _context.NhanViens, "SMaNV", "STenNV", hopDong.FK_sMaNV);

            return View(hopDong);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var hopDong = await _context.HopDongs.FindAsync(id);

            if (hopDong == null) return NotFound();

            ViewData["FK_sMaNV"] = new SelectList(
                _context.NhanViens, "SMaNV", "STenNV", hopDong.FK_sMaNV);

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

                    TempData["Success"] = "Cập nhật hợp đồng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HopDongExists(hopDong.SMaHD))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["FK_sMaNV"] = new SelectList(
                _context.NhanViens, "SMaNV", "STenNV", hopDong.FK_sMaNV);

            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var hopDong = await _context.HopDongs
                .FirstOrDefaultAsync(h => h.SMaHD == id);

            if (hopDong == null) return NotFound();

            hopDong.STrangThai = "Đã hủy";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy hợp đồng thành công!";

            return RedirectToAction(nameof(Index));
        }

        private bool HopDongExists(string id)
        {
            return _context.HopDongs.Any(e => e.SMaHD == id);
        }
    }
}