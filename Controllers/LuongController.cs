using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;
using NMCNPM.Filters;

namespace NMCNPM.Controllers;

// Quản lý lương: chốt kỳ lương theo dKyLuong (ngày 01 của tháng).
// Thực lĩnh = Lương cơ bản + Thưởng − Phạt.
// Một nhân viên chỉ có một bản ghi lương trong mỗi kỳ; đã chốt thì không tính lại,
// nhưng vẫn được điều chỉnh thưởng/phạt ở màn DieuChinh.
[AuthorizeRole("Q_ADMIN", "Q_MANAGER")]
public class LuongController : Controller
{
    private const string TrangThaiDangLam = "Đang làm việc";
 
    private readonly HrmDbContext _db;
    private readonly bool _ownsDb;
 
    public LuongController(IServiceProvider services, IConfiguration configuration)
    {
        // Program.cs hiện chưa đăng ký HrmDbContext; dùng DI nếu có, nếu không thì tự tạo từ connection string.
        var db = services.GetService<HrmDbContext>();
        if (db is null)
        {
            var options = new DbContextOptionsBuilder<HrmDbContext>()
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .Options;
            db = new HrmDbContext(options);
            _ownsDb = true;
        }
        _db = db;
    }
 
    // ------------------------------------------------------------------
    // GET: /Luong?ky=2026-11  -> Tổng quan kỳ lương
    // ------------------------------------------------------------------
    public async Task<IActionResult> Index(string? ky)
    {
        var kyLuong = TryParseKy(ky, out var parsed) ? parsed : DauThang(DateTime.Today);
 
        var dangLam = await _db.NhanViens.AsNoTracking()
            .Where(n => n.STrangThai == null || n.STrangThai == TrangThaiDangLam)
            .Select(n => n.SMaNV)
            .ToListAsync();
 
        var luongKy = await _db.Luongs.AsNoTracking()
            .Where(l => l.DKyLuong == kyLuong)
            .Select(l => new { l.FK_sMaNV, l.FThucLinh })
            .ToListAsync();
 
        var daChot = luongKy.Select(l => l.FK_sMaNV).ToHashSet();
 
        return View(new LuongIndexViewModel
        {
            Ky = kyLuong.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            SoBanGhi = luongKy.Count,
            TongQuyLuong = luongKy.Sum(l => l.FThucLinh),
            SoNhanVienDangLam = dangLam.Count,
            SoChuaChot = dangLam.Count(ma => !daChot.Contains(ma))
        });
    }
 
    // ------------------------------------------------------------------
    // GET: /Luong/BangLuong?ky=2026-11&phongBan=PB01&tuKhoa=NV01
    // ------------------------------------------------------------------
    public async Task<IActionResult> BangLuong(string? ky, string? phongBan, string? tuKhoa)
    {
        DateTime kyLuong;
        string? loi = null;
 
        if (string.IsNullOrWhiteSpace(ky))
        {
            // Mặc định: kỳ lương gần nhất đã có dữ liệu
            var ganNhat = await _db.Luongs.AsNoTracking()
                .OrderByDescending(l => l.DKyLuong)
                .Select(l => (DateTime?)l.DKyLuong)
                .FirstOrDefaultAsync();
            kyLuong = DauThang(ganNhat ?? DateTime.Today);
        }
        else if (!TryParseKy(ky, out kyLuong))
        {
            loi = "Vui lòng chọn kỳ lương";
            kyLuong = DauThang(DateTime.Today);
        }
 
        var vm = new BangLuongViewModel
        {
            Ky = kyLuong.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            PhongBan = phongBan,
            TuKhoa = tuKhoa,
            ThongBaoLoi = loi,
            DanhSachPhongBan = await _db.NhanViens.AsNoTracking()
                .Select(n => n.FK_sMaPB).Distinct().OrderBy(x => x).ToListAsync()
        };
 
        if (loi != null)
        {
            return View(vm);
        }
 
        var query = from l in _db.Luongs.AsNoTracking()
                    join nv in _db.NhanViens.AsNoTracking() on l.FK_sMaNV equals nv.SMaNV
                    where l.DKyLuong == kyLuong
                    select new { l, nv };
 
        if (!string.IsNullOrWhiteSpace(phongBan))
        {
            query = query.Where(x => x.nv.FK_sMaPB == phongBan);
        }
 
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(x => x.nv.SMaNV.Contains(k) || x.nv.STenNV.Contains(k));
        }
 
        vm.Dong = await query
            .OrderBy(x => x.nv.SMaNV)
            .Select(x => new LuongDongViewModel
            {
                SMaLuong = x.l.SMaLuong,
                MaNV = x.nv.SMaNV,
                TenNV = x.nv.STenNV,
                MaPB = x.nv.FK_sMaPB,
                LuongCoBan = x.l.FLuongCoBan,
                SoGioLam = x.l.ISoGioLam,
                Thuong = x.l.FThuong,
                Phat = x.l.FPhat,
                ThucLinh = x.l.FThucLinh
            })
            .ToListAsync();
 
        vm.TongThucLinh = vm.Dong.Sum(d => d.ThucLinh);
        return View(vm);
    }
 
    // ------------------------------------------------------------------
    // GET: /Luong/DieuChinh/L2026110001
    // ------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> DieuChinh(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }
 
        var vm = await BuildDieuChinhAsync(id);
        return vm is null ? NotFound() : View(vm);
    }
 
    // POST: /Luong/DieuChinh
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DieuChinh(string? sMaLuong, decimal thuong, decimal phat)
    {
        if (string.IsNullOrWhiteSpace(sMaLuong))
        {
            return NotFound();
        }
 
        var luong = await _db.Luongs.FirstOrDefaultAsync(l => l.SMaLuong == sMaLuong);
        var vm = await BuildDieuChinhAsync(sMaLuong);
        if (luong is null || vm is null)
        {
            return NotFound();
        }
 
        // Lương CB và số giờ làm luôn lấy từ CSDL, chỉ nhận Thưởng/Phạt từ form.
        vm.Thuong = thuong;
        vm.Phat = phat;
 
        if (thuong < 0)
            ModelState.AddModelError(string.Empty, "Thưởng không được âm.");
        if (phat < 0)
            ModelState.AddModelError(string.Empty, "Phạt không được âm.");
        if (luong.FLuongCoBan + thuong - phat < 0)
            ModelState.AddModelError(string.Empty, "Phạt vượt quá Lương cơ bản + Thưởng.");
 
        if (!ModelState.IsValid)
        {
            return View(vm);
        }
 
        luong.FThuong = thuong;
        luong.FPhat = phat;
        luong.FThucLinh = luong.FLuongCoBan + thuong - phat;   // Thực lĩnh = Lương CB + Thưởng − Phạt
        luong.DNgayTinhLuong = DateTime.Now;
        await _db.SaveChangesAsync();
 
        TempData["ThongBao"] = $"Đã cập nhật thưởng/phạt cho {vm.TenNV}.";
        return RedirectToAction(nameof(BangLuong), new { ky = vm.Ky });
    }
 
    // ------------------------------------------------------------------
    // GET: /Luong/ChotKy?ky=2026-11
    // ------------------------------------------------------------------
    public async Task<IActionResult> ChotKy(string? ky)
    {
        var kyLuong = TryParseKy(ky, out var parsed) ? parsed : DauThang(DateTime.Today);
        return View(await BuildChotKyAsync(kyLuong, null));
    }
 
    // POST: /Luong/ChotKy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChotKy(ChotKyViewModel input)
    {
        if (!TryParseKy(input.Ky, out var kyLuong))
        {
            ModelState.AddModelError(nameof(input.Ky), "Kỳ lương không hợp lệ (định dạng yyyy-MM).");
            return View(await BuildChotKyAsync(DauThang(DateTime.Today), input.Rows));
        }
 
        if (kyLuong > DauThang(DateTime.Today))
        {
            ModelState.AddModelError(nameof(input.Ky), "Không thể chốt lương cho kỳ trong tương lai.");
        }
 
        // Lương CB và số giờ làm luôn lấy lại từ CSDL, chỉ nhận Thưởng/Phạt từ form.
        var model = await BuildChotKyAsync(kyLuong, input.Rows);
        var canChot = model.Rows.Where(r => r.Chon && !r.DaChot).ToList();
 
        for (var i = 0; i < model.Rows.Count; i++)
        {
            var r = model.Rows[i];
            if (!r.Chon || r.DaChot) continue;
 
            if (r.Thuong < 0)
                ModelState.AddModelError($"Rows[{i}].Thuong", $"{r.MaNV}: Thưởng không được âm.");
            if (r.Phat < 0)
                ModelState.AddModelError($"Rows[{i}].Phat", $"{r.MaNV}: Phạt không được âm.");
            if (r.LuongCoBan + r.Thuong - r.Phat < 0)
                ModelState.AddModelError($"Rows[{i}].Phat", $"{r.MaNV}: Phạt vượt quá Lương CB + Thưởng.");
        }
 
        if (canChot.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Không có nhân viên nào cần chốt lương trong kỳ này.");
        }
 
        if (!ModelState.IsValid)
        {
            return View(model);
        }
 
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
 
        // Kiểm tra lại trong transaction để tránh chốt trùng (FK_sMaNV, dKyLuong).
        var maNVs = canChot.Select(r => r.MaNV).ToList();
        var daCo = await _db.Luongs
            .Where(l => l.DKyLuong == kyLuong && maNVs.Contains(l.FK_sMaNV))
            .Select(l => l.FK_sMaNV)
            .ToListAsync();
 
        var prefix = "L" + kyLuong.ToString("yyyyMM", CultureInfo.InvariantCulture);
        var maHienCo = await _db.Luongs
            .Where(l => l.SMaLuong.StartsWith(prefix))
            .Select(l => l.SMaLuong)
            .ToListAsync();
        var stt = maHienCo
            .Select(m => int.TryParse(m.AsSpan(prefix.Length), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
 
        var now = DateTime.Now;
        var soBanGhi = 0;
        foreach (var r in canChot.Where(r => !daCo.Contains(r.MaNV)))
        {
            stt++;
            _db.Luongs.Add(new Luong
            {
                SMaLuong = prefix + stt.ToString("D4", CultureInfo.InvariantCulture),
                FK_sMaNV = r.MaNV,
                DKyLuong = kyLuong,
                ISoGioLam = r.SoGioLam,
                FLuongCoBan = r.LuongCoBan,
                FThuong = r.Thuong,
                FPhat = r.Phat,
                FThucLinh = r.LuongCoBan + r.Thuong - r.Phat,
                DNgayTinhLuong = now
            });
            soBanGhi++;
        }
 
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
 
        var boQua = canChot.Count - soBanGhi;
        TempData["ThongBao"] = $"Đã chốt lương kỳ {kyLuong:MM/yyyy} cho {soBanGhi} nhân viên."
            + (boQua > 0 ? $" Bỏ qua {boQua} nhân viên đã được chốt trước đó." : string.Empty);
 
        return RedirectToAction(nameof(BangLuong), new { ky = kyLuong.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }
 
    // ================= HÀM HỖ TRỢ =================
 
    private async Task<DieuChinhViewModel?> BuildDieuChinhAsync(string id)
    {
        return await (from l in _db.Luongs.AsNoTracking()
                      join nv in _db.NhanViens.AsNoTracking() on l.FK_sMaNV equals nv.SMaNV
                      where l.SMaLuong == id
                      select new DieuChinhViewModel
                      {
                          SMaLuong = l.SMaLuong,
                          MaNV = nv.SMaNV,
                          TenNV = nv.STenNV,
                          Ky = l.DKyLuong.ToString("yyyy-MM"),
                          LuongCoBan = l.FLuongCoBan,
                          SoGioLam = l.ISoGioLam,
                          Thuong = l.FThuong,
                          Phat = l.FPhat
                      }).FirstOrDefaultAsync();
    }
 
    private async Task<ChotKyViewModel> BuildChotKyAsync(DateTime kyLuong, List<ChotKyRow>? posted)
    {
        var kyKetThuc = kyLuong.AddMonths(1);
 
        var nhanViens = await _db.NhanViens.AsNoTracking()
            .Where(n => n.STrangThai == null || n.STrangThai == TrangThaiDangLam)
            .OrderBy(n => n.SMaNV)
            .ToListAsync();
 
        var gioLam = await _db.ChamCongs.AsNoTracking()
            .Where(c => c.DNgayChamCong >= kyLuong && c.DNgayChamCong < kyKetThuc)
            .GroupBy(c => c.FK_sMaNV)
            .Select(g => new { MaNV = g.Key, Tong = g.Sum(c => c.ISoGioLam) })
            .ToDictionaryAsync(x => x.MaNV, x => x.Tong);
 
        var daChot = await _db.Luongs.AsNoTracking()
            .Where(l => l.DKyLuong == kyLuong)
            .ToDictionaryAsync(l => l.FK_sMaNV);
 
        var postedByNV = (posted ?? new List<ChotKyRow>())
            .Where(p => !string.IsNullOrEmpty(p.MaNV))
            .GroupBy(p => p.MaNV)
            .ToDictionary(g => g.Key, g => g.First());
 
        var rows = new List<ChotKyRow>();
        foreach (var nv in nhanViens)
        {
            if (daChot.TryGetValue(nv.SMaNV, out var l))
            {
                rows.Add(new ChotKyRow
                {
                    MaNV = nv.SMaNV,
                    TenNV = nv.STenNV,
                    SoGioLam = l.ISoGioLam,
                    LuongCoBan = l.FLuongCoBan,
                    Thuong = l.FThuong,
                    Phat = l.FPhat,
                    DaChot = true,
                    MaLuong = l.SMaLuong
                });
                continue;
            }
 
            postedByNV.TryGetValue(nv.SMaNV, out var p);
            rows.Add(new ChotKyRow
            {
                MaNV = nv.SMaNV,
                TenNV = nv.STenNV,
                SoGioLam = gioLam.TryGetValue(nv.SMaNV, out var gio) ? gio : 0,
                LuongCoBan = nv.FLuongcb,
                Thuong = p?.Thuong ?? 0,
                Phat = p?.Phat ?? 0,
                Chon = p?.Chon ?? true
            });
        }
 
        return new ChotKyViewModel
        {
            Ky = kyLuong.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            Rows = rows
        };
    }
 
    private static DateTime DauThang(DateTime d) => new(d.Year, d.Month, 1);
 
    private static bool TryParseKy(string? ky, out DateTime kyLuong)
    {
        var ok = DateTime.TryParseExact(ky, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out kyLuong);
        if (ok) kyLuong = DauThang(kyLuong);
        return ok;
    }
 
    protected override void Dispose(bool disposing)
    {
        if (disposing && _ownsDb)
        {
            _db.Dispose();
        }
        base.Dispose(disposing);
    }
}
 
// ================= VIEW MODELS =================
 
public class LuongIndexViewModel
{
    public string Ky { get; set; } = string.Empty;
    public int SoBanGhi { get; set; }
    public decimal TongQuyLuong { get; set; }
    public int SoNhanVienDangLam { get; set; }
    public int SoChuaChot { get; set; }
    public bool DaChotXong => SoBanGhi > 0 && SoChuaChot == 0;
}
 
public class LuongDongViewModel
{
    public string SMaLuong { get; set; } = string.Empty;
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string MaPB { get; set; } = string.Empty;
    public decimal LuongCoBan { get; set; }
    public int SoGioLam { get; set; }
    public decimal Thuong { get; set; }
    public decimal Phat { get; set; }
    public decimal ThucLinh { get; set; }
}
 
public class BangLuongViewModel
{
    public string Ky { get; set; } = string.Empty;
    public string? PhongBan { get; set; }
    public string? TuKhoa { get; set; }
    public string? ThongBaoLoi { get; set; }
    public List<string> DanhSachPhongBan { get; set; } = new();
    public List<LuongDongViewModel> Dong { get; set; } = new();
    public decimal TongThucLinh { get; set; }
}
 
public class DieuChinhViewModel
{
    public string SMaLuong { get; set; } = string.Empty;
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string Ky { get; set; } = string.Empty;
    public decimal LuongCoBan { get; set; }
    public int SoGioLam { get; set; }
    public decimal Thuong { get; set; }
    public decimal Phat { get; set; }
    public decimal ThucLinh => LuongCoBan + Thuong - Phat;
}
 
public class ChotKyViewModel
{
    public string Ky { get; set; } = string.Empty;
    public List<ChotKyRow> Rows { get; set; } = new();
}
 
public class ChotKyRow
{
    public string MaNV { get; set; } = string.Empty;
    public string? TenNV { get; set; }
    public int SoGioLam { get; set; }
    public decimal LuongCoBan { get; set; }
    public decimal Thuong { get; set; }
    public decimal Phat { get; set; }
    public decimal ThucLinh => LuongCoBan + Thuong - Phat;
    public bool Chon { get; set; }
    public bool DaChot { get; set; }
    public string? MaLuong { get; set; }
}