using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly HrmDbContext _context;

        public NhanVienController(HrmDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH NHÂN VIÊN
        public async Task<IActionResult> Index(string searchString, string maPB, string maCV, string trangThai)
        {
            // Nạp danh sách Phòng ban & Chức vụ cho Dropdown lọc
            ViewBag.PhongBans = new SelectList(await _context.PhongBans.ToListAsync(), "SMaPB", "STenPB", maPB);
            ViewBag.ChucVus = new SelectList(await _context.ChucVus.ToListAsync(), "SMaCV", "STenCV", maCV);

            // Nạp dictionary để tra cứu Tên phòng ban & Tên chức vụ theo Mã nhanh chóng trên View
            ViewBag.PhongBanDict = await _context.PhongBans.ToDictionaryAsync(p => p.SMaPB, p => p.STenPB);
            ViewBag.ChucVuDict = await _context.ChucVus.ToDictionaryAsync(c => c.SMaCV, c => c.STenCV);

            var query = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim().ToLower();
                query = query.Where(n => n.SMaNV.ToLower().Contains(searchString) ||
                                         n.STenNV.ToLower().Contains(searchString) ||
                                         (n.SSDT != null && n.SSDT.Contains(searchString)) ||
                                         (n.SEmail != null && n.SEmail.ToLower().Contains(searchString)));
            }

            if (!string.IsNullOrWhiteSpace(maPB))
            {
                query = query.Where(n => n.FK_sMaPB == maPB);
            }

            if (!string.IsNullOrWhiteSpace(maCV))
            {
                query = query.Where(n => n.FK_sMaCV == maCV);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                trangThai = trangThai.Trim().ToLower();
                
                if (trangThai.Contains("nghỉ") || trangThai.Contains("nghi"))
                {
                    query = query.Where(n => n.STrangThai != null && (n.STrangThai.ToLower().Contains("nghỉ") || n.STrangThai.ToLower().Contains("nghi")));
                }
                else if (trangThai.Contains("làm") || trangThai.Contains("lam"))
                {
                    query = query.Where(n => n.STrangThai != null && (n.STrangThai.ToLower().Contains("làm") || n.STrangThai.ToLower().Contains("lam")));
                }
                else
                {
                    query = query.Where(n => n.STrangThai == trangThai);
                }
            }

            var result = await query.OrderBy(n => n.SMaNV).ToListAsync();
            return View(result);
        }

        // 2. CHI TIẾT NHÂN VIÊN
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(m => m.SMaNV == id);
            if (nhanVien == null) return NotFound();

            var pb = await _context.PhongBans.FirstOrDefaultAsync(p => p.SMaPB == nhanVien.FK_sMaPB);
            var cv = await _context.ChucVus.FirstOrDefaultAsync(c => c.SMaCV == nhanVien.FK_sMaCV);

            ViewBag.TenPB = pb?.STenPB ?? nhanVien.FK_sMaPB;
            ViewBag.TenCV = cv?.STenCV ?? nhanVien.FK_sMaCV;

            return View(nhanVien);
        }

        // 3. THÊM NHÂN VIÊN (GET)
        public async Task<IActionResult> Create()
        {
            ViewBag.PhongBans = new SelectList(await _context.PhongBans.ToListAsync(), "SMaPB", "STenPB");
            ViewBag.ChucVus = new SelectList(await _context.ChucVus.ToListAsync(), "SMaCV", "STenCV");
            return View();
        }

        // 3. THÊM NHÂN VIÊN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PhongBans = new SelectList(await _context.PhongBans.ToListAsync(), "SMaPB", "STenPB", nhanVien.FK_sMaPB);
            ViewBag.ChucVus = new SelectList(await _context.ChucVus.ToListAsync(), "SMaCV", "STenCV", nhanVien.FK_sMaCV);
            return View(nhanVien);
        }

        // 4. SỬA NHÂN VIÊN (GET)
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            ViewBag.PhongBans = new SelectList(await _context.PhongBans.ToListAsync(), "SMaPB", "STenPB", nhanVien.FK_sMaPB);
            ViewBag.ChucVus = new SelectList(await _context.ChucVus.ToListAsync(), "SMaCV", "STenCV", nhanVien.FK_sMaCV);
            return View(nhanVien);
        }

        // 4. SỬA NHÂN VIÊN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NhanVien nhanVien)
        {
            if (id != nhanVien.SMaNV) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NhanViens.Any(e => e.SMaNV == nhanVien.SMaNV))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PhongBans = new SelectList(await _context.PhongBans.ToListAsync(), "SMaPB", "STenPB", nhanVien.FK_sMaPB);
            ViewBag.ChucVus = new SelectList(await _context.ChucVus.ToListAsync(), "SMaCV", "STenCV", nhanVien.FK_sMaCV);
            return View(nhanVien);
        }

        // 5. XÓA NHÂN VIÊN (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}