
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using NMCNPM.Filters;
using NMCNPM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace NMCNPM.Controllers
{
    [AuthorizeRole("Q_ADMIN", "Q_MANAGER", "Q_STAFF")]
    public class ChamCongController : Controller
    {
        // =====================================================
        // KẾT NỐI DATABASE
        // =====================================================

        private readonly string connectionString;

        public ChamCongController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        


            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Không tìm thấy chuỗi kết nối DefaultConnection trong appsettings.json."
                );
            }
        }

        // =====================================================
        // HÀM LẤY THÔNG TIN NHÂN VIÊN TỪ CLAIMS
        // =====================================================

        private string? LayMaNhanVien()
        {
            return User.FindFirst("MaNV")?.Value
                ?? User.FindFirst("FK_sMaNV")?.Value;
        }


        private string LayTenNhanVien()
        {
            return User.FindFirst("HoTen")?.Value
                ?? User.FindFirst("TenNV")?.Value
                ?? "";
        }

        // =====================================================
        // 1. CHẤM CÔNG TRỰC TUYẾN
        // =====================================================

        [HttpGet]
        public ActionResult TrucTuyen()
        {
            string? maNV = LayMaNhanVien();

            if (string.IsNullOrWhiteSpace(maNV))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            TrucTuyenViewModel model;
            string thongBao;

            bool thanhCong = LayThongTinTrucTuyen(
                maNV,
                out model,
                out thongBao
            );

            if (!thanhCong)
            {
                ViewBag.ThongBao = thongBao;

                model = new TrucTuyenViewModel
                {
                    MaNV = maNV,
                    TenNV = LayTenNhanVien(),
                    NgayHienTai = DateTime.Today,
                    GioHienTai = DateTime.Now,
                    TrangThaiChamCong = "Không có dữ liệu",
                    LichSu7Ngay = new List<ChamCong>()
                };
            }

            return View(model);
        }

        // =====================================================
        // 2. CHẤM CÔNG VÀO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChamCongVao()
        {
            string? maNV = LayMaNhanVien();

            if (string.IsNullOrWhiteSpace(maNV))
            {
                return Json(new
                {
                    success = false,
                    message = "Không xác định được nhân viên đăng nhập. Vui lòng đăng nhập lại."
                });
            }

            ChamCong? chamCong;
            string thongBao;

            bool thanhCong = XuLyChamCongVao(
                maNV,
                out chamCong,
                out thongBao
            );


            if (!thanhCong)
            {
                return Json(new
                {
                    success = false,
                    message = thongBao
                });
            }

            return Json(new
            {
                success = true,
                message = thongBao,
                data = new
                {
                    maChamCong = chamCong.MaChamCong,
                    ngayChamCong = chamCong.NgayChamCong.ToString("dd/MM/yyyy"),
                    gioVao = chamCong.GioVao.HasValue
                        ? chamCong.GioVao.Value.ToString("HH:mm:ss")
                        : "",
                    trangThai = chamCong.TrangThai
                }
            });
        }

        // =====================================================
        // 3. CHẤM CÔNG RA
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChamCongRa()
        {
            string? maNV = LayMaNhanVien();

            if (string.IsNullOrWhiteSpace(maNV))
            {
                return Json(new
                {
                    success = false,
                    message = "Không xác định được nhân viên đăng nhập. Vui lòng đăng nhập lại."
                });
            }

            ChamCong chamCong;
            string thongBao;

            bool thanhCong = XuLyChamCongRa(
                maNV,
                out chamCong,
                out thongBao
            );

            if (!thanhCong)
            {
                return Json(new
                {
                    success = false,
                    message = thongBao
                });
            }

            return Json(new
            {
                success = true,
                message = thongBao,
                data = new
                {
                    maChamCong = chamCong.MaChamCong,
                    ngayChamCong = chamCong.NgayChamCong.ToString("dd/MM/yyyy"),
                    gioVao = chamCong.GioVao.HasValue
                        ? chamCong.GioVao.Value.ToString("HH:mm:ss")
                        : "",
                    gioRa = chamCong.GioRa.HasValue
                        ? chamCong.GioRa.Value.ToString("HH:mm:ss")
                        : "",
                    soGioLam = chamCong.SoGioLam,
                    trangThai = chamCong.TrangThai
                }
            });
        }

        // =====================================================
        // 4. LỊCH SỬ CHẤM CÔNG
        // =====================================================

        [HttpGet]
        [AuthorizeRole("Q_ADMIN", "Q_MANAGER", "Q_STAFF")]
        public ActionResult LichSu(LichSuViewModel boLoc)
        {
            string? maNV = LayMaNhanVien();


            if (string.IsNullOrWhiteSpace(maNV))
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            if (boLoc == null)
            {
                boLoc = new LichSuViewModel();
            }

            List<ChamCong> danhSach;
            string thongBao;

            bool thanhCong = LayLichSu(
                maNV,
                boLoc,
                out danhSach,
                out thongBao
            );

            if (thanhCong)
            {
                boLoc.DanhSachChamCong = danhSach;
            }
            else
            {
                boLoc.DanhSachChamCong = new List<ChamCong>();
                ViewBag.ThongBao = thongBao;
            }

            return View(boLoc);
        }

        // =====================================================
        // 5. THEO DÕI CHẤM CÔNG
        // CHỈ ADMIN
        // =====================================================

        [HttpGet]
        [AuthorizeRole("Q_ADMIN")]
        public ActionResult TheoDoi(TheoDoiViewModel boLoc)
        {
            if (boLoc == null)
            {
                boLoc = new TheoDoiViewModel();
            }

            boLoc.DanhSachPhongBan = LayDanhSachPhongBan();

            List<ChamCong> danhSach;
            string thongBao;

            bool thanhCong = LayTheoDoi(
                boLoc,
                out danhSach,
                out thongBao
            );

            if (thanhCong)
            {
                boLoc.DanhSachChamCong = danhSach;
            }
            else
            {
                boLoc.DanhSachChamCong = new List<ChamCong>();
                ViewBag.ThongBao = thongBao;
            }

            return View(boLoc);
        }

        // =====================================================
        // 6. LẤY DANH SÁCH PHÒNG BAN
        // =====================================================

        private List<SelectListItem> LayDanhSachPhongBan()
        {
            List<SelectListItem> danhSach = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT sMaPB, sTenPB
                    FROM tbl_PhongBan
                    ORDER BY sTenPB";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            danhSach.Add(new SelectListItem
                            {
                                Value = rd["sMaPB"].ToString(),
                                Text = rd["sTenPB"].ToString()
                            });
                        }
                    }
                }
            }

            return danhSach;
        }

        // =====================================================
        // 7. LẤY THÔNG TIN CHẤM CÔNG TRỰC TUYẾN
        // =====================================================

        private bool LayThongTinTrucTuyen(
            string maNV,
            out TrucTuyenViewModel model,
            out string thongBao)
        {
            model = new TrucTuyenViewModel
            {
                LichSu7Ngay = new List<ChamCong>()
            };

            thongBao = "";

            if (string.IsNullOrWhiteSpace(maNV))
            {
                thongBao = "Không xác định được nhân viên đăng nhập.";
                return false;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // -----------------------------------------
                    // 7.1. LẤY THÔNG TIN NHÂN VIÊN
                    // -----------------------------------------

                    string sqlNhanVien = @"
                        SELECT
                            nv.sMaNV,
                            nv.sTenNV,
                            pb.sTenPB
                        FROM tbl_NhanVien nv
                        LEFT JOIN tbl_PhongBan pb
                            ON nv.FK_sMaPB = pb.sMaPB
                        WHERE nv.sMaNV = @MaNV";

                    using (SqlCommand cmd = new SqlCommand(sqlNhanVien, conn))
                    {
                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                thongBao = "Không tìm thấy thông tin nhân viên.";
                                return false;
                            }

                            model.MaNV = rd["sMaNV"].ToString();
                            model.TenNV = rd["sTenNV"].ToString();

                            model.TenPB = rd["sTenPB"] == DBNull.Value
                                ? ""
                                : rd["sTenPB"].ToString();
                        }
                    }

                    // -----------------------------------------
                    // 7.2. LẤY THÔNG TIN CHẤM CÔNG HÔM NAY
                    // -----------------------------------------

                    model.NgayHienTai = DateTime.Today;
                    model.GioHienTai = DateTime.Now;

                    string sqlHomNay = @"
                        SELECT
                            dGioVao,
                            dGioRa,
                            iSoGioLam,
                            sTrangThai
                        FROM tbl_ChamCong
                        WHERE FK_sMaNV = @MaNV
                          AND dNgayChamCong >= @NgayBatDau
                          AND dNgayChamCong < @NgayKetThuc";

                    using (SqlCommand cmd = new SqlCommand(sqlHomNay, conn))
                    {
                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        cmd.Parameters.Add(
                            "@NgayBatDau",
                            SqlDbType.Date
                        ).Value = DateTime.Today;

                        cmd.Parameters.Add(
                            "@NgayKetThuc",
                            SqlDbType.Date
                        ).Value = DateTime.Today.AddDays(1);

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                model.GioVao = rd["dGioVao"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(rd["dGioVao"]);

                                model.GioRa = rd["dGioRa"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(rd["dGioRa"]);

                                model.SoGioLam = rd["iSoGioLam"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(rd["iSoGioLam"]);

                                model.TrangThaiChamCong =
                                    rd["sTrangThai"] == DBNull.Value
                                    ? "Không xác định"
                                    : rd["sTrangThai"].ToString();
                            }
                            else
                            {
                                model.TrangThaiChamCong = "Chưa chấm công";
                            }
                        }
                    }

                    // -----------------------------------------
                    // 7.3. LẤY LỊCH SỬ CHẤM CÔNG 7 NGÀY
                    // -----------------------------------------

                    string sqlLichSu = @"
                        SELECT
                            sMaChamCong,
                            FK_sMaNV,
                            dNgayChamCong,
                            dGioVao,
                            dGioRa,
                            iSoGioLam,
                            sTrangThai,
                            sGhiChu
                        FROM tbl_ChamCong
                        WHERE FK_sMaNV = @MaNV
                          AND dNgayChamCong >= @NgayBatDau
                          AND dNgayChamCong < @NgayKetThuc
                        ORDER BY
                            dNgayChamCong DESC,
                            dGioVao DESC";

                    using (SqlCommand cmd = new SqlCommand(sqlLichSu, conn))
                    {
                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        // Bao gồm hôm nay và 6 ngày trước đó.
                        cmd.Parameters.Add(
                            "@NgayBatDau",
                            SqlDbType.Date
                        ).Value = DateTime.Today.AddDays(-6);

                        // Không bao gồm ngày mai.
                        cmd.Parameters.Add(
                            "@NgayKetThuc",
                            SqlDbType.Date
                        ).Value = DateTime.Today.AddDays(1);

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                ChamCong item = new ChamCong
                                {
                                    MaChamCong = rd["sMaChamCong"].ToString() ?? string.Empty,

                                    FK_sMaNV = rd["FK_sMaNV"].ToString() ?? string.Empty,

                                    NgayChamCong = Convert.ToDateTime(
                                        rd["dNgayChamCong"]
                                    ),


                                    GioVao = rd["dGioVao"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(rd["dGioVao"]),

                                    GioRa = rd["dGioRa"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(rd["dGioRa"]),

                                    SoGioLam = rd["iSoGioLam"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(rd["iSoGioLam"]),

                                    TrangThai = rd["sTrangThai"] == DBNull.Value
                                        ? ""
                                        : rd["sTrangThai"].ToString(),

                                    GhiChu = rd["sGhiChu"] == DBNull.Value
                                        ? ""
                                        : rd["sGhiChu"].ToString()
                                };

                                model.LichSu7Ngay.Add(item);
                            }
                        }
                    }
                }

                thongBao = "Lấy thông tin chấm công thành công.";
                return true;
            }
            catch (Exception ex)
            {
                thongBao = "Lỗi lấy thông tin chấm công: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 8. XỬ LÝ CHẤM CÔNG VÀO
        // =====================================================

        private bool XuLyChamCongVao(
            string maNV,
            out ChamCong? chamCong, // Thêm dấu ? vào đây
            out string thongBao)
        {
            chamCong = null;
            thongBao = "";

            if (string.IsNullOrWhiteSpace(maNV))
            {
                thongBao = "Không xác định được nhân viên.";
                return false;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlKiemTra = @"
                        SELECT COUNT(*)
                        FROM tbl_ChamCong
                        WHERE FK_sMaNV = @MaNV
                          AND dNgayChamCong = @NgayChamCong";

                    using (SqlCommand cmd = new SqlCommand(sqlKiemTra, conn))
                    {
                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        cmd.Parameters.Add(
                            "@NgayChamCong",
                            SqlDbType.Date
                        ).Value = DateTime.Today;

                        int soLuong = Convert.ToInt32(cmd.ExecuteScalar());

                        if (soLuong > 0)
                        {
                            thongBao = "Bạn đã có bản ghi chấm công hôm nay.";
                            return false;
                        }
                    }

                    DateTime gioVao = DateTime.Now;
                    TimeSpan gioBatDau = new TimeSpan(9, 0, 0);

                    string trangThai = gioVao.TimeOfDay > gioBatDau
                        ? "Đến muộn"
                        : "Đúng giờ";

                    string maChamCong = "CC" +
                        Guid.NewGuid().ToString("N").Substring(0, 18);

                    string sqlThem = @"
                        INSERT INTO tbl_ChamCong
                        (
                            sMaChamCong,
                            FK_sMaNV,
                            dNgayChamCong,
                            dGioVao,
                            dGioRa,
                            iSoGioLam,
                            sTrangThai,
                            sGhiChu
                        )
                        VALUES
                        (
                            @MaChamCong,
                            @MaNV,
                            @NgayChamCong,
                            @GioVao,
                            NULL,
                            0,
                            @TrangThai,
                            NULL
                        )";

                    using (SqlCommand cmd = new SqlCommand(sqlThem, conn))
                    {
                        cmd.Parameters.Add(
                            "@MaChamCong",
                            SqlDbType.VarChar,
                            20
                        ).Value = maChamCong;

                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        cmd.Parameters.Add(
                            "@NgayChamCong",
                            SqlDbType.Date
                        ).Value = DateTime.Today;

                        cmd.Parameters.Add(
                            "@GioVao",
                            SqlDbType.DateTime
                        ).Value = gioVao;

                        cmd.Parameters.Add(
                            "@TrangThai",
                            SqlDbType.NVarChar,
                            50
                        ).Value = trangThai;

                        cmd.ExecuteNonQuery();
                    }

                    chamCong = new ChamCong
                    {
                        MaChamCong = maChamCong,
                        FK_sMaNV = maNV,
                        NgayChamCong = DateTime.Today,
                        GioVao = gioVao,
                        GioRa = null,
                        SoGioLam = 0,
                        TrangThai = trangThai
                    };
                }

                thongBao = "Chấm công vào thành công.";
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    thongBao = "Bạn đã chấm công vào hôm nay.";
                    return false;
                }

                thongBao = "Lỗi cơ sở dữ liệu: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                thongBao = "Lỗi chấm công vào: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 9. XỬ LÝ CHẤM CÔNG RA
        // =====================================================

        private bool XuLyChamCongRa(
            string maNV,
            out ChamCong? chamCong, // Thêm dấu ? vào đây
            out string thongBao)
        {
            chamCong = null;
            thongBao = "";

            if (string.IsNullOrWhiteSpace(maNV))
            {
                thongBao = "Không xác định được nhân viên.";
                return false;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    DateTime gioRa = DateTime.Now;
                    TimeSpan gioKetThuc = new TimeSpan(17, 0, 0);

                    string sql = @"
                        UPDATE tbl_ChamCong
                        SET
                            dGioRa = @GioRa,
                            iSoGioLam = CASE
                                WHEN DATEDIFF(MINUTE, dGioVao, @GioRa) < 0
                                    THEN 0
                                ELSE DATEDIFF(MINUTE, dGioVao, @GioRa) / 60
                            END,
                            sTrangThai = CASE
                                WHEN CAST(@GioRa AS time) < @GioKetThuc
                                    THEN N'Ra về sớm'
                                ELSE sTrangThai
                            END
                        WHERE FK_sMaNV = @MaNV
                          AND dNgayChamCong = @NgayChamCong
                          AND dGioVao IS NOT NULL
                          AND dGioRa IS NULL";

                    int soDong;

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add(
                            "@GioRa",
                            SqlDbType.DateTime
                        ).Value = gioRa;

                        cmd.Parameters.Add(
                            "@GioKetThuc",
                            SqlDbType.Time
                        ).Value = gioKetThuc;

                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        cmd.Parameters.Add(
                            "@NgayChamCong",
                            SqlDbType.Date
                        ).Value = DateTime.Today;

                        soDong = cmd.ExecuteNonQuery();
                    }

                    if (soDong == 0)
                    {
                        thongBao = "Bạn chưa chấm công vào hoặc đã chấm công ra.";
                        return false;
                    }

                    string sqlLay = @"
                        SELECT *
                        FROM tbl_ChamCong
                        WHERE FK_sMaNV = @MaNV
                          AND dNgayChamCong = @NgayChamCong";

                    using (SqlCommand cmd = new SqlCommand(sqlLay, conn))
                    {
                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        cmd.Parameters.Add(
                            "@NgayChamCong",
                            SqlDbType.Date
                        ).Value = DateTime.Today;

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                thongBao = "Không tìm thấy dữ liệu chấm công.";
                                return false;
                            }

                            chamCong = ChuyenDuLieuChamCong(rd);
                        }
                    }
                }

                thongBao = "Chấm công ra thành công.";
                return true;
            }
            catch (Exception ex)
            {
                thongBao = "Lỗi chấm công ra: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 10. LẤY LỊCH SỬ CHẤM CÔNG
        // =====================================================

        private bool LayLichSu(
            string maNV,
            LichSuViewModel boLoc,
            out List<ChamCong> danhSach,
            out string thongBao)
        {
            danhSach = new List<ChamCong>();
            thongBao = "";

            if (string.IsNullOrWhiteSpace(maNV))
            {
                thongBao = "Không xác định được nhân viên đăng nhập.";
                return false;
            }

            if (boLoc == null)
            {
                boLoc = new LichSuViewModel();
            }

            try
            {
                StringBuilder sql = new StringBuilder(@"
                    SELECT
                        cc.sMaChamCong,
                        cc.FK_sMaNV,
                        nv.sTenNV,
                        cc.dNgayChamCong,
                        cc.dGioVao,
                        cc.dGioRa,
                        cc.iSoGioLam,
                        cc.sTrangThai,
                        cc.sGhiChu
                    FROM tbl_ChamCong cc
                    LEFT JOIN tbl_NhanVien nv
                        ON cc.FK_sMaNV = nv.sMaNV
                    WHERE cc.FK_sMaNV = @MaNV
                ");

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    cmd.Parameters.Add(
                        "@MaNV",
                        SqlDbType.VarChar,
                        20
                    ).Value = maNV;

                    if (boLoc.TuNgay.HasValue)
                    {
                        sql.Append(" AND cc.dNgayChamCong >= @TuNgay");

                        cmd.Parameters.Add(
                            "@TuNgay",
                            SqlDbType.Date
                        ).Value = boLoc.TuNgay.Value.Date;
                    }

                    if (boLoc.DenNgay.HasValue)
                    {
                        sql.Append(" AND cc.dNgayChamCong < @DenNgay");

                        cmd.Parameters.Add(
                            "@DenNgay",
                            SqlDbType.Date
                        ).Value = boLoc.DenNgay.Value.Date.AddDays(1);
                    }

                    if (!string.IsNullOrWhiteSpace(boLoc.TrangThai))
                    {
                        sql.Append(" AND cc.sTrangThai = @TrangThai");

                        cmd.Parameters.Add(
                            "@TrangThai",
                            SqlDbType.NVarChar,
                            50
                        ).Value = boLoc.TrangThai.Trim();
                    }

                    sql.Append(@"
                        ORDER BY
                            cc.dNgayChamCong DESC,
                            cc.dGioVao DESC
                    ");

                    cmd.CommandText = sql.ToString();

                    conn.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            danhSach.Add(ChuyenDuLieuChamCong(rd));
                        }
                    }
                }

                boLoc.SoNgayCong = danhSach
                    .Select(x => x.NgayChamCong.Date)
                    .Distinct()
                    .Count();

                boLoc.SoNgayMuon = danhSach
                    .Count(x =>
                        !string.IsNullOrWhiteSpace(x.TrangThai) &&
                        x.TrangThai.Contains("Đến muộn"));

                boLoc.SoNgayVeSom = danhSach
                    .Count(x =>
                        !string.IsNullOrWhiteSpace(x.TrangThai) &&
                        x.TrangThai.Contains("Ra về sớm"));

                boLoc.SoNgayNghi = danhSach
                    .Count(x => x.TrangThai == "Vắng");

                boLoc.TongSoGioLam = danhSach
                    .Sum(x => (decimal)x.SoGioLam);

                thongBao = "Lấy lịch sử chấm công thành công.";
                return true;
            }
            catch (Exception ex)
            {
                thongBao = "Lỗi lấy lịch sử chấm công: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 11. LẤY DỮ LIỆU THEO DÕI CHẤM CÔNG
        // =====================================================

        private bool LayTheoDoi(
            TheoDoiViewModel boLoc,
            out List<ChamCong> danhSach,
            out string thongBao)
        {
            danhSach = new List<ChamCong>();
            thongBao = "";

            try
            {
                StringBuilder sql = new StringBuilder(@"
                    SELECT
                        cc.sMaChamCong,
                        cc.FK_sMaNV,
                        nv.sTenNV,
                        cc.dNgayChamCong,
                        cc.dGioVao,
                        cc.dGioRa,
                        cc.iSoGioLam,
                        cc.sTrangThai,
                        cc.sGhiChu
                    FROM tbl_ChamCong cc
                    INNER JOIN tbl_NhanVien nv
                        ON cc.FK_sMaNV = nv.sMaNV
                    LEFT JOIN tbl_PhongBan pb
                        ON nv.FK_sMaPB = pb.sMaPB
                    WHERE 1 = 1
                ");

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (boLoc != null)
                    {
                        if (!string.IsNullOrWhiteSpace(boLoc.TenNhanVien))
                        {
                            sql.Append(" AND nv.sTenNV LIKE @TenNhanVien");

                            cmd.Parameters.Add(
                                "@TenNhanVien",
                                SqlDbType.NVarChar,
                                100
                            ).Value = "%" + boLoc.TenNhanVien.Trim() + "%";
                        }

                        if (boLoc.TuNgay.HasValue)
                        {
                            sql.Append(" AND cc.dNgayChamCong >= @TuNgay");

                            cmd.Parameters.Add(
                                "@TuNgay",
                                SqlDbType.Date
                            ).Value = boLoc.TuNgay.Value.Date;
                        }

                        if (boLoc.DenNgay.HasValue)
                        {
                            sql.Append(" AND cc.dNgayChamCong < @DenNgay");

                            cmd.Parameters.Add(
                                "@DenNgay",
                                SqlDbType.Date
                            ).Value = boLoc.DenNgay.Value.Date.AddDays(1);
                        }

                        if (!string.IsNullOrWhiteSpace(boLoc.MaPhongBan))
                        {
                            sql.Append(" AND nv.FK_sMaPB = @MaPhongBan");

                            cmd.Parameters.Add(
                                "@MaPhongBan",
                                SqlDbType.VarChar,
                                20
                            ).Value = boLoc.MaPhongBan.Trim();
                        }

                        ThemDieuKienGio(
                            sql,
                            cmd,
                            "cc.dGioVao",
                            boLoc.TuGioVao,
                            boLoc.DenGioVao,
                            "@TuGioVao",
                            "@DenGioVao"
                        );

                        ThemDieuKienGio(
                            sql,
                            cmd,
                            "cc.dGioRa",
                            boLoc.TuGioRa,
                            boLoc.DenGioRa,
                            "@TuGioRa",
                            "@DenGioRa"
                        );

                        if (!string.IsNullOrWhiteSpace(boLoc.TrangThai))
                        {
                            sql.Append(" AND cc.sTrangThai = @TrangThai");

                            cmd.Parameters.Add(
                                "@TrangThai",
                                SqlDbType.NVarChar,
                                50
                            ).Value = boLoc.TrangThai.Trim();
                        }
                    }

                    sql.Append(@"
                        ORDER BY
                            cc.dNgayChamCong DESC,
                            cc.dGioVao DESC
                    ");

                    cmd.CommandText = sql.ToString();

                    conn.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            danhSach.Add(ChuyenDuLieuChamCong(rd));
                        }
                    }
                }

                thongBao = "Lấy danh sách theo dõi chấm công thành công.";
                return true;
            }
            catch (Exception ex)
            {
                thongBao = "Lỗi theo dõi chấm công: " + ex.Message;
                return false;
            }
        }

        // =====================================================
        // 12. HÀM PHỤ: THÊM ĐIỀU KIỆN LỌC GIỜ
        // =====================================================

        private void ThemDieuKienGio(
            StringBuilder sql,
            SqlCommand cmd,
            string cotGio,
            TimeSpan? tuGio,
            TimeSpan? denGio,
            string tenThamSoTu,
            string tenThamSoDen)
        {
            if (tuGio.HasValue)
            {
                sql.Append(
                    " AND CAST(" + cotGio + " AS time) >= " +
                    tenThamSoTu
                );

                cmd.Parameters.Add(
                    tenThamSoTu,
                    SqlDbType.Time
                ).Value = tuGio.Value;
            }

            if (denGio.HasValue)
            {
                sql.Append(
                    " AND CAST(" + cotGio + " AS time) <= " +
                    tenThamSoDen
                );

                cmd.Parameters.Add(
                    tenThamSoDen,
                    SqlDbType.Time
                ).Value = denGio.Value;
            }
        }

        // =====================================================
        // 13. HÀM PHỤ: CHUYỂN SQL DATA READER THÀNH CHAMCONG
        // =====================================================

        private ChamCong ChuyenDuLieuChamCong(SqlDataReader rd)
        {
            int viTriTenNV = -1;

            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (rd.GetName(i).Equals(
                    "sTenNV",
                    StringComparison.OrdinalIgnoreCase))
                {
                    viTriTenNV = i;
                    break;
                }
            }

            return new ChamCong
            {
                MaChamCong = rd["sMaChamCong"].ToString() ?? string.Empty,

                FK_sMaNV = rd["FK_sMaNV"].ToString() ?? string.Empty,


                TenNV = viTriTenNV >= 0 && rd[viTriTenNV] != DBNull.Value
                    ? rd[viTriTenNV].ToString()
                    : "",

                NgayChamCong = Convert.ToDateTime(
                    rd["dNgayChamCong"]
                ),

                GioVao = rd["dGioVao"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(rd["dGioVao"]),

                GioRa = rd["dGioRa"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(rd["dGioRa"]),

                SoGioLam = rd["iSoGioLam"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(rd["iSoGioLam"]),

                TrangThai = rd["sTrangThai"] == DBNull.Value
                    ? ""
                    : rd["sTrangThai"].ToString(),

                GhiChu = rd["sGhiChu"] == DBNull.Value
                    ? ""
                    : rd["sGhiChu"].ToString()
            };
        }
    }
}
