
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;
using System.Security.Claims;

namespace NMCNPM.Controllers
{
    [Authorize]
    [Route("HopDong")]
    public class HopDongController : Controller
    {
        private readonly HrmDbContext _db;

        private const int SoDongMoiTrang = 10;
        private const string DaHuy = "Đã hủy";

        public HopDongController(HrmDbContext db)
        {
            _db = db;
        }

        private bool LaNhanSu()
        {
            return User.IsInRole("Admin")
                || User.IsInRole("HR")
                || User.IsInRole("NhanSu");
        }

        private string? LayMaNhanVienDangNhap()
        {
            return User.FindFirst("MaNV")?.Value;
        }

        private IQueryable<HopDongLaoDong> ApDungPhamVi(
            IQueryable<HopDongLaoDong> query)
        {
            if (LaNhanSu())
                return query;

            string? maNV = LayMaNhanVienDangNhap();

            if (string.IsNullOrEmpty(maNV))
                return query.Where(hd => false);

            return query.Where(hd => hd.FK_sMaNV == maNV);
        }

        private bool CoQuyenXem(HopDongLaoDong hd)
        {
            if (LaNhanSu())
                return true;

            string? maNV = LayMaNhanVienDangNhap();

            return !string.IsNullOrEmpty(maNV)
                && hd.FK_sMaNV == maNV;
        }

        private bool CoQuyenQuanLy()
        {
            return LaNhanSu();
        }

        private void NapDanhSachNhanVien(string? maNVChon = null)
        {
            var nhanViens = _db.NhanViens
                .AsNoTracking()
                .Where(nv => nv.STrangThai != "Đã nghỉ việc");

            if (!LaNhanSu())
            {
                string? maNV = LayMaNhanVienDangNhap();
                nhanViens = nhanViens.Where(nv => nv.SMaNV == maNV);
            }

            ViewBag.NhanViens = nhanViens
                .OrderBy(nv => nv.STenNV)
                .ToList();

            ViewBag.MaNVChon = maNVChon;
        }

        private async Task NapThongTinNhanVienAsync(
            string maNV,
            CancellationToken cancellationToken = default)
        {
            var nv = await _db.NhanViens
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.SMaNV == maNV,
                    cancellationToken);

            ViewBag.TenNhanVien = nv?.STenNV ?? "";
            ViewBag.LuongCoBan = nv?.FLuongcb ?? 0;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            if (page < 1)
                page = 1;

            var query = ApDungPhamVi(_db.HopDongs.AsNoTracking());

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(hd =>
                    hd.SMaHD.Contains(tuKhoa)
                    || hd.FK_sMaNV.Contains(tuKhoa)
                    || _db.NhanViens.Any(nv =>
                        nv.SMaNV == hd.FK_sMaNV
                        && nv.STenNV.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(hd =>
                    hd.STrangThai == trangThai);
            }

            int tongSoHopDong = await query.CountAsync(cancellationToken);

            var danhSach = await (
                from hd in query
                join nv in _db.NhanViens.AsNoTracking()
                    on hd.FK_sMaNV equals nv.SMaNV
                orderby hd.DNgayKy descending
                select new HopDongDanhSachVM
                {
                    SMaHD = hd.SMaHD,
                    MaNV = nv.SMaNV,
                    TenNV = nv.STenNV,
                    NgayKy = hd.DNgayKy,
                    NgayHetHan = hd.DNgayHetHan,
                    LuongCoBan = nv.FLuongcb,
                    TrangThai = hd.STrangThai
                })
                .Skip((page - 1) * SoDongMoiTrang)
                .Take(SoDongMoiTrang)
                .ToListAsync(cancellationToken);

            var model = new HopDongDanhSachTrangVM
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                TrangHienTai = page,
                TongSoTrang = (int)Math.Ceiling(
                    tongSoHopDong / (double)SoDongMoiTrang),
                TongSoHopDong = tongSoHopDong
            };

            return View(model);
        }

        [HttpGet("them-moi")]
        public IActionResult ThemMoi()
        {
            if (!CoQuyenQuanLy())
                return Forbid();

            NapDanhSachNhanVien();

            return View();
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemMoi(
            HopDongLaoDong model,
            CancellationToken cancellationToken = default)
        {
            if (!CoQuyenQuanLy())
                return Forbid();

            model.SMaHD = model.SMaHD?.Trim() ?? "";
            model.FK_sMaNV = model.FK_sMaNV?.Trim() ?? "";
            model.SLoaiHD = model.SLoaiHD?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(model.SMaHD))
                ModelState.AddModelError("SMaHD", "Vui lòng nhập mã hợp đồng.");

            if (string.IsNullOrWhiteSpace(model.FK_sMaNV))
                ModelState.AddModelError("FK_sMaNV", "Vui lòng chọn nhân viên.");

            if (string.IsNullOrWhiteSpace(model.SLoaiHD))
                ModelState.AddModelError("SLoaiHD", "Vui lòng chọn loại hợp đồng.");

            if (!await _db.NhanViens.AnyAsync(
                nv => nv.SMaNV == model.FK_sMaNV,
                cancellationToken))
            {
                ModelState.AddModelError("FK_sMaNV", "Nhân viên không tồn tại.");
            }

            if (await _db.HopDongs.AnyAsync(
                hd => hd.SMaHD == model.SMaHD,
                cancellationToken))
            {
                ModelState.AddModelError("SMaHD", "Mã hợp đồng đã tồn tại.");
            }

            if (!TrangThaiHopLe(model.STrangThai))
            {
                ModelState.AddModelError("STrangThai", "Trạng thái không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                NapDanhSachNhanVien(model.FK_sMaNV);
                return View(model);
            }

            var hopDong = new HopDongLaoDong
            {
                SMaHD = model.SMaHD,
                FK_sMaNV = model.FK_sMaNV,
                SLoaiHD = model.SLoaiHD,
                DNgayKy = model.DNgayKy,
                DNgayHetHan = model.DNgayHetHan,
                STrangThai = model.STrangThai
            };

            _db.HopDongs.Add(hopDong);
            await _db.SaveChangesAsync(cancellationToken);

            TempData["ThongBao"] = "Tạo hợp đồng lao động thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{ma}/sua")]
        public async Task<IActionResult> ChinhSua(
            string ma,
            CancellationToken cancellationToken = default)
        {
            if (!CoQuyenQuanLy())
                return Forbid();

            var hopDong = await _db.HopDongs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    hd => hd.SMaHD == ma,
                    cancellationToken);

            if (hopDong == null)
            {
                TempData["Loi"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            if (hopDong.STrangThai == DaHuy)
            {
                TempData["Loi"] = "Hợp đồng đã hủy, không thể sửa.";
                return RedirectToAction(nameof(Index));
            }

            await NapThongTinNhanVienAsync(
                hopDong.FK_sMaNV,
                cancellationToken);

            return View(hopDong);
        }

        [HttpPost("{ma}/sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChinhSua(
            string ma,
            HopDongLaoDong model,
            CancellationToken cancellationToken = default)
        {
            if (!CoQuyenQuanLy())
                return Forbid();

            var hopDong = await _db.HopDongs
                .FirstOrDefaultAsync(
                    hd => hd.SMaHD == ma,
                    cancellationToken);

            if (hopDong == null)
            {
                TempData["Loi"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            if (hopDong.STrangThai == DaHuy)
            {
                TempData["Loi"] = "Hợp đồng đã hủy, không thể sửa.";
                return RedirectToAction(nameof(Index));
            }

            if (!TrangThaiHopLe(model.STrangThai))
            {
                ModelState.AddModelError("STrangThai", "Trạng thái không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await NapThongTinNhanVienAsync(
                    hopDong.FK_sMaNV,
                    cancellationToken);

                model.SMaHD = hopDong.SMaHD;
                model.FK_sMaNV = hopDong.FK_sMaNV;
                model.SLoaiHD = hopDong.SLoaiHD;
                model.DNgayHetHan = hopDong.DNgayHetHan;

                return View(model);
            }

            hopDong.DNgayKy = model.DNgayKy;
            hopDong.STrangThai = model.STrangThai;

            await _db.SaveChangesAsync(cancellationToken);

            TempData["ThongBao"] = "Cập nhật hợp đồng thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{ma}/xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(
            string ma,
            CancellationToken cancellationToken = default)
        {
            if (!CoQuyenQuanLy())
                return Forbid();

            var hopDong = await _db.HopDongs
                .FirstOrDefaultAsync(
                    hd => hd.SMaHD == ma,
                    cancellationToken);

            if (hopDong == null)
            {
                TempData["Loi"] = "Không tìm thấy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            if (hopDong.STrangThai == DaHuy)
            {
                TempData["Loi"] = "Hợp đồng đã được hủy trước đó.";
                return RedirectToAction(nameof(Index));
            }

            hopDong.STrangThai = DaHuy;

            await _db.SaveChangesAsync(cancellationToken);

            TempData["ThongBao"] = "Đã hủy hợp đồng thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{ma}")]
        public async Task<IActionResult> ChiTiet(
            string ma,
            CancellationToken cancellationToken = default)
        {
            var hopDong = await ApDungPhamVi(
                    _db.HopDongs.AsNoTracking())
                .Where(hd => hd.SMaHD == ma)
                .Join(
                    _db.NhanViens.AsNoTracking(),
                    hd => hd.FK_sMaNV,
                    nv => nv.SMaNV,
                    (hd, nv) => new HopDongChiTietVM
                    {
                        SMaHD = hd.SMaHD,
                        MaNV = nv.SMaNV,
                        TenNV = nv.STenNV,
                        LoaiHD = hd.SLoaiHD,
                        NgayKy = hd.DNgayKy,
                        NgayHetHan = hd.DNgayHetHan,
                        LuongCoBan = nv.FLuongcb,
                        TrangThai = hd.STrangThai
                    })
                .FirstOrDefaultAsync(cancellationToken);

            if (hopDong == null)
            {
                TempData["Loi"] = "Không tìm thấy hợp đồng hoặc bạn không có quyền xem.";
                return RedirectToAction(nameof(Index));
            }

            return View(hopDong);
        }

        private bool TrangThaiHopLe(string? trangThai)
        {
            return trangThai == "Còn hiệu lực"
                || trangThai == "Hết hạn"
                || trangThai == DaHuy;
        }
    }

    public class HopDongDanhSachVM
    {
        public string SMaHD { get; set; } = "";
        public string MaNV { get; set; } = "";
        public string TenNV { get; set; } = "";
        public DateTime NgayKy { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public decimal LuongCoBan { get; set; }
        public string? TrangThai { get; set; }
    }

    public class HopDongDanhSachTrangVM
    {
        public List<HopDongDanhSachVM> DanhSach { get; set; } = new();
        public string? TuKhoa { get; set; }
        public string? TrangThai { get; set; }
        public int TrangHienTai { get; set; }
        public int TongSoTrang { get; set; }
        public int TongSoHopDong { get; set; }
    }

    public class HopDongChiTietVM
    {
        public string SMaHD { get; set; } = "";
        public string MaNV { get; set; } = "";
        public string TenNV { get; set; } = "";
        public string LoaiHD { get; set; } = "";
        public DateTime NgayKy { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public decimal LuongCoBan { get; set; }
        public string? TrangThai { get; set; }
    }
}
