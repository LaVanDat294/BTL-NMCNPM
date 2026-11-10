using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TocoTocoHRM.Data;
using TocoTocoHRM.Models;
using TocoTocoHRM.ViewModels;

namespace TocoTocoHRM.Controllers;

[Authorize]
public class NhanVienController : Controller
{
    private readonly HrmDbContext _db;
    public NhanVienController(HrmDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? tuKhoa, string? maPB, string? maCV, string? trangThai)
    {
        var query = from nv in _db.NhanViens.AsNoTracking()
                    join pb in _db.PhongBans on nv.MaPB equals pb.MaPB
                    join cv in _db.ChucVus on nv.MaCV equals cv.MaCV
                    select new TocoTocoHRM.ViewModels.NhanVienListItemViewModel { NhanVien = nv, TenPB = pb.TenPB, TenCV = cv.TenCV };
        if (!string.IsNullOrWhiteSpace(tuKhoa))
            query = query.Where(x => x.NhanVien.MaNV.Contains(tuKhoa) || x.NhanVien.TenNV.Contains(tuKhoa) ||
                                     (x.NhanVien.SDT ?? "").Contains(tuKhoa));
        if (!string.IsNullOrWhiteSpace(maPB)) query = query.Where(x => x.NhanVien.MaPB == maPB);
        if (!string.IsNullOrWhiteSpace(maCV)) query = query.Where(x => x.NhanVien.MaCV == maCV);
        if (!string.IsNullOrWhiteSpace(trangThai)) query = query.Where(x => x.NhanVien.TrangThai == trangThai);

        var data = await query.OrderBy(x => x.NhanVien.MaNV).ToListAsync();
        ViewBag.TuKhoa = tuKhoa; ViewBag.MaPB = maPB; ViewBag.MaCV = maCV; ViewBag.TrangThai = trangThai;
        ViewBag.PhongBans = new SelectList(await _db.PhongBans.AsNoTracking().ToListAsync(), "MaPB", "TenPB", maPB);
        ViewBag.ChucVus = new SelectList(await _db.ChucVus.AsNoTracking().ToListAsync(), "MaCV", "TenCV", maCV);
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View("Form", await BuildForm(new NhanVien { NgayVaoLam = DateTime.Today }));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NhanVienFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", await BuildForm(model.NhanVien));
        if (await _db.NhanViens.AnyAsync(x => x.MaNV == model.NhanVien.MaNV))
        {
            ModelState.AddModelError("NhanVien.MaNV", "Mã nhân viên đã tồn tại.");
            return View("Form", await BuildForm(model.NhanVien));
        }
        if (await _db.NhanViens.AnyAsync(x => x.CCCD == model.NhanVien.CCCD))
        {
            ModelState.AddModelError("NhanVien.CCCD", "CCCD đã được sử dụng.");
            return View("Form", await BuildForm(model.NhanVien));
        }
        _db.NhanViens.Add(model.NhanVien);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm nhân viên thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var nv = await _db.NhanViens.FindAsync(id);
        if (nv == null) return NotFound();
        return View("Form", await BuildForm(nv));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, NhanVienFormViewModel model)
    {
        if (id != model.NhanVien.MaNV) return BadRequest();
        if (!ModelState.IsValid) return View("Form", await BuildForm(model.NhanVien));
        if (await _db.NhanViens.AnyAsync(x => x.CCCD == model.NhanVien.CCCD && x.MaNV != id))
        {
            ModelState.AddModelError("NhanVien.CCCD", "CCCD đã được sử dụng bởi nhân viên khác.");
            return View("Form", await BuildForm(model.NhanVien));
        }
        _db.NhanViens.Update(model.NhanVien);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật nhân viên thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var nv = await _db.NhanViens.FindAsync(id);
        if (nv == null) { TempData["Error"] = "Không tìm thấy nhân viên."; return RedirectToAction(nameof(Index)); }
        _db.NhanViens.Remove(nv);
        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa nhân viên.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa vì nhân viên đã có dữ liệu chấm công, lương, hợp đồng, đơn từ hoặc tài khoản.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<NhanVienFormViewModel> BuildForm(NhanVien nv) => new()
    {
        NhanVien = nv,
        PhongBans = await _db.PhongBans.AsNoTracking()
            .Select(x => new SelectListItem(x.TenPB, x.MaPB)).ToListAsync(),
        ChucVus = await _db.ChucVus.AsNoTracking()
            .Select(x => new SelectListItem(x.TenCV, x.MaCV)).ToListAsync()
    };
}
