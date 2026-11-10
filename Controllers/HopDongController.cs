using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    [Route("HopDong")]
    public class HopDongController : Controller
    {
        private readonly HrmDbContext _context;

        public HopDongController(HrmDbContext context)
        {
            _context = context;
        }

        private async Task LoadNhanVien()
        {
            var nhanViens = await _context.NhanViens
                .OrderBy(nv => nv.STenNV)
                .ToListAsync();

            ViewBag.DanhSachNhanVien = new SelectList(
                nhanViens,
                "SMaNV",
                "STenNV"
            );
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            var query = _context.HopDongs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(hd =>
                    hd.SMaHD.Contains(searchString) ||
                    hd.FK_sMaNV.Contains(searchString) ||
                    _context.NhanViens.Any(nv =>
                        nv.SMaNV == hd.FK_sMaNV &&
                        nv.STenNV.Contains(searchString)
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(hd => hd.STrangThai == trangThai);
            }

            var danhSach = await query
                .OrderByDescending(hd => hd.DNgayKy)
                .ToListAsync();

            ViewBag.TenNhanVien = await _context.NhanViens
                .ToDictionaryAsync(nv => nv.SMaNV, nv => nv.STenNV);

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;

            return View(danhSach);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var hopDong = await _context.HopDongs
                .FirstOrDefaultAsync(hd => hd.SMaHD == id);

            if (hopDong == null)
            {
                TempData["Error"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.SMaNV == hopDong.FK_sMaNV);

            ViewBag.TenNhanVien = nhanVien?.STenNV;

            return View(hopDong);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> ThemMoi()
        {
            await LoadNhanVien();
            return View("Create");
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemMoi(HopDongLaoDong hopDong)
        {
            if (string.IsNullOrWhiteSpace(hopDong.SMaHD))
            {
                ModelState.AddModelError("SMaHD", "Vui lòng nhập mã hợp đồng.");
            }
            else
            {
                hopDong.SMaHD = hopDong.SMaHD.Trim();

                if (await _context.HopDongs.AnyAsync(hd => hd.SMaHD == hopDong.SMaHD))
                {
                    ModelState.AddModelError("SMaHD", "Mã hợp đồng đã tồn tại.");
                }
            }

            if (string.IsNullOrWhiteSpace(hopDong.SLoaiHD))
            {
                ModelState.AddModelError("SLoaiHD", "Vui lòng chọn loại hợp đồng.");
            }

            if (string.IsNullOrWhiteSpace(hopDong.FK_sMaNV))
            {
                ModelState.AddModelError("FK_sMaNV", "Vui lòng chọn nhân viên.");
            }
            else
            {
                bool nhanVienTonTai = await _context.NhanViens
                    .AnyAsync(nv => nv.SMaNV == hopDong.FK_sMaNV);

                if (!nhanVienTonTai)
                {
                    ModelState.AddModelError("FK_sMaNV", "Nhân viên không tồn tại.");
                }
            }

            if (hopDong.DNgayHetHan.HasValue &&
                hopDong.DNgayHetHan.Value < hopDong.DNgayKy)
            {
                ModelState.AddModelError(
                    "DNgayHetHan",
                    "Ngày hết hạn không được trước ngày ký."
                );
            }

            if (!ModelState.IsValid)
            {
                await LoadNhanVien();
                return View("Create", hopDong);
            }

            hopDong.STrangThai = "Còn hiệu lực";

            _context.HopDongs.Add(hopDong);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm hợp đồng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}/sua")]
        public async Task<IActionResult> Sua(string id)
        {
            var hopDong = await _context.HopDongs
                .FirstOrDefaultAsync(hd => hd.SMaHD == id);

            if (hopDong == null)
            {
                TempData["Error"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            if (hopDong.STrangThai == "Đã huỷ")
            {
                TempData["Error"] = "Không thể sửa hợp đồng đã huỷ.";
                return RedirectToAction(nameof(Index));
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.SMaNV == hopDong.FK_sMaNV);

            ViewBag.TenNhanVien = nhanVien?.STenNV;

            return View("Edit", hopDong);
        }

        [HttpPost("{id}/sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(string id, HopDongLaoDong hopDong)
        {
            if (id != hopDong.SMaHD)
            {
                return NotFound();
            }

            var hopDongCu = await _context.HopDongs
                .FirstOrDefaultAsync(hd => hd.SMaHD == id);

            if (hopDongCu == null)
            {
                TempData["Error"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            if (hopDongCu.STrangThai == "Đã huỷ")
            {
                TempData["Error"] = "Không thể sửa hợp đồng đã huỷ.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(hopDong.SLoaiHD))
            {
                ModelState.AddModelError("SLoaiHD", "Vui lòng chọn loại hợp đồng.");
            }

            if (hopDong.DNgayHetHan.HasValue &&
                hopDong.DNgayHetHan.Value < hopDong.DNgayKy)
            {
                ModelState.AddModelError(
                    "DNgayHetHan",
                    "Ngày hết hạn không được trước ngày ký."
                );
            }

            if (!ModelState.IsValid)
            {
                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(nv => nv.SMaNV == hopDongCu.FK_sMaNV);

                ViewBag.TenNhanVien = nhanVien?.STenNV;
                return View("Edit", hopDong);
            }

            hopDongCu.SLoaiHD = hopDong.SLoaiHD;
            hopDongCu.DNgayKy = hopDong.DNgayKy;
            hopDongCu.DNgayHetHan = hopDong.DNgayHetHan;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật hợp đồng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id}/xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(string id)
        {
            var hopDong = await _context.HopDongs
                .FirstOrDefaultAsync(hd => hd.SMaHD == id);

            if (hopDong == null)
            {
                TempData["Error"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            if (hopDong.STrangThai == "Đã huỷ")
            {
                TempData["Error"] = "Hợp đồng này đã được huỷ trước đó.";
                return RedirectToAction(nameof(Index));
            }

            hopDong.STrangThai = "Đã huỷ";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Huỷ hợp đồng thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
