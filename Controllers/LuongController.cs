using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class LuongController : Controller
    {
        private readonly HrmDbContext _context;

        public LuongController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.NhanViens = new SelectList(_context.NhanViens, "SMaNV", "STenNV");
            var hrmDbContext = _context.Luongs.Include(l => l.NhanVien);
            return View(await hrmDbContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Luong luong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(luong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", luong.FK_sMaNV);
            return View(luong);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var luong = await _context.Luongs.FindAsync(id);
            if (luong == null) return NotFound();
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", luong.FK_sMaNV);
            return View(luong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Luong luong)
        {
            if (id != luong.SMaLuong) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(luong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LuongExists(luong.SMaLuong)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FK_sMaNV"] = new SelectList(_context.NhanViens, "SMaNV", "STenNV", luong.FK_sMaNV);
            return View(luong);
        }

        public async Task<IActionResult> CalculateSalary(string maNV, DateTime kyLuong)
        {
            // 1. Lấy dữ liệu chấm công của nhân viên trong tháng
            var chamCongs = await _context.ChamCongs
                .Where(c => c.FK_sMaNV == maNV && c.DNgayChamCong.Month == kyLuong.Month && c.DNgayChamCong.Year == kyLuong.Year)
                .ToListAsync();

            int totalHours = chamCongs.Sum(c => c.ISoGioLam); // Giả định ChamCong có thuộc tính ISoGioLam

            // 2. Lấy thông tin lương cơ bản từ bảng Nhân viên hoặc bảng Luong (cũ)
            var nhanVien = await _context.NhanViens.FindAsync(maNV);

            // 3. Tính toán lương
            decimal luongCoBan = nhanVien != null ? nhanVien.FLuongcb : 0; // Đã sửa từ FLuongCoBan thành FLuongcb
            decimal thucLinh = (luongCoBan / 160) * totalHours; // Giả định 160 giờ chuẩn/tháng

            // 4. Lưu hoặc cập nhật vào database
            var luong = new Luong
            {
                SMaLuong = Guid.NewGuid().ToString().Substring(0, 8),
                FK_sMaNV = maNV,
                DKyLuong = kyLuong,
                ISoGioLam = totalHours,
                FLuongCoBan = luongCoBan,
                DNgayTinhLuong = DateTime.Now
            };

            _context.Add(luong);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool LuongExists(string id)
        {
            return _context.Luongs.Any(e => e.SMaLuong == id);
        }
    }
}