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

        // =========================================================================
        // 1. DANH SÁCH NHÂN VIÊN (INDEX) + BỘ LỌC TÌM KIẾM
        // =========================================================================
        // GET: NhanVien
        public async Task<IActionResult> Index(string searchString, string maPB, string maCV, string trangThai)
        {
            ViewBag.PhongBans = new SelectList(await _context.PhongBans.ToListAsync(), "SMaPB", "STenPB", maPB);
            ViewBag.ChucVus = new SelectList(await _context.ChucVus.ToListAsync(), "SMaCV", "STenCV", maCV);

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
                query = query.Where(n => n.STrangThai == trangThai);
            }

            var listNhanVien = await query.OrderBy(n => n.SMaNV).ToListAsync();
            return View(listNhanVien);
        }

        // =========================================================================
        // 2. XEM CHI TIẾT NHÂN VIÊN (DETAILS)
        // =========================================================================
        // GET: NhanVien/Details/NV001
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(m => m.SMaNV == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            var phongBan = await _context.PhongBans.FindAsync(nhanVien.FK_sMaPB);
            var chucVu = await _context.ChucVus.FindAsync(nhanVien.FK_sMaCV);

            ViewBag.TenPB = phongBan?.STenPB ?? nhanVien.FK_sMaPB;
            ViewBag.TenCV = chucVu?.STenCV ?? nhanVien.FK_sMaCV;

            return View(nhanVien);
        }

        // =========================================================================
        // 3. THÊM MỚI NHÂN VIÊN (CREATE)
        // =========================================================================
        // GET: NhanVien/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nhanVien)
        {
            if (await _context.NhanViens.AnyAsync(n => n.SMaNV == nhanVien.SMaNV))
            {
                ModelState.AddModelError("SMaNV", "Mã nhân viên này đã tồn tại trong hệ thống.");
            }

            if (await _context.NhanViens.AnyAsync(n => n.SCCCD == nhanVien.SCCCD))
            {
                ModelState.AddModelError("SCCCD", "Số CCCD này đã được đăng ký cho nhân viên khác.");
            }

            if (ModelState.IsValid)
            {
                if (nhanVien.DNgayVaoLam == null)
                {
                    nhanVien.DNgayVaoLam = DateTime.Now;
                }
                if (string.IsNullOrEmpty(nhanVien.STrangThai))
                {
                    nhanVien.STrangThai = "Đang làm việc";
                }

                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(nhanVien.FK_sMaPB, nhanVien.FK_sMaCV);
            return View(nhanVien);
        }

        // =========================================================================
        // 4. CHỈNH SỬA THÔNG TIN NHÂN VIÊN (EDIT)
        // =========================================================================
        // GET: NhanVien/Edit/NV001
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(nhanVien.FK_sMaPB, nhanVien.FK_sMaCV);
            return View(nhanVien);
        }

        // POST: NhanVien/Edit/NV001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NhanVien nhanVien)
        {
            if (id != nhanVien.SMaNV)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.SMaNV))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(nhanVien.FK_sMaPB, nhanVien.FK_sMaCV);
            return View(nhanVien);
        }

        // =========================================================================
        // 5. XÓA NHÂN VIÊN (DELETE)
        // =========================================================================
        // POST: NhanVien/Delete/NV001
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // HÀM BỔ TRỢ (HELPER METHODS)
        // =========================================================================
        private async Task PopulateDropdownsAsync(string selectedPB = null, string selectedCV = null)
        {
            var phongBans = await _context.PhongBans.ToListAsync();
            var chucVus = await _context.ChucVus.ToListAsync();

            ViewBag.FK_sMaPB = new SelectList(phongBans, "SMaPB", "STenPB", selectedPB);
            ViewBag.FK_sMaCV = new SelectList(chucVus, "SMaCV", "STenCV", selectedCV);
        }

        private bool NhanVienExists(string id)
        {
            return _context.NhanViens.Any(e => e.SMaNV == id);
        }
    }
}