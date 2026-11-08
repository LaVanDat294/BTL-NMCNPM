
using NMCNPM.Filters;
using NMCNPM.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Mvc;
using System.Linq;

namespace NMCNPM.Controllers
{
    [AuthorizeRole("Q_ADMIN", "Q_MANAGER", "Q_STAFF")]
    public class ChamCongController : Controller
    {
        // =====================================================
        // KẾT NỐI DATABASE
        // =====================================================

        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["HRMConnection"].ConnectionString;


        // =====================================================
        // 1. CHẤM CÔNG TRỰC TUYẾN
        // Admin, Manager, Staff
        // =====================================================

        [HttpGet]
        public ActionResult TrucTuyen()
        {
            string maNV = Session["MaNV"] == null
                ? null
                : Session["MaNV"].ToString();

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

                return View(new TrucTuyenViewModel
                {
                    MaNV = maNV,
                    TenNV = Session["HoTen"] == null
                        ? ""
                        : Session["HoTen"].ToString(),
                    NgayHienTai = DateTime.Today,
                    GioHienTai = DateTime.Now,
                    TrangThaiChamCong = "Không có dữ liệu"
                });
            }

            return View(model);
        }


        // =====================================================
        // 2. CHẤM CÔNG VÀO
        // Admin, Manager, Staff
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChamCongVao()
        {
            string maNV = Session["MaNV"] == null
                ? null
                : Session["MaNV"].ToString();

            if (string.IsNullOrWhiteSpace(maNV))
            {
                return Json(new
                {
                    success = false,
                    message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
                });
            }

            ChamCong chamCong;

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
        // Admin, Manager, Staff
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChamCongRa()
        {
            string maNV = Session["MaNV"] == null
                ? null
                : Session["MaNV"].ToString();

            if (string.IsNullOrWhiteSpace(maNV))
            {
                return Json(new
                {
                    success = false,
                    message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
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
        // Admin, Manager
        // =====================================================

        [HttpGet]
        [AuthorizeRole("Q_ADMIN", "Q_MANAGER", "Q_STAFF")]
        public ActionResult LichSu(LichSuViewModel boLoc)
        {
            string maNV = Session["MaNV"] == null
                ? null
                : Session["MaNV"].ToString();

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
        // Chỉ Admin
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

        [HttpGet]
        [AuthorizeRole("Q_ADMIN")]
        public ActionResult TheoDoi(TheoDoiViewModel boLoc)
        {
            if (boLoc == null)
            {
                boLoc = new TheoDoiViewModel();
            }

            // Tải danh sách phòng ban cho dropdown
            boLoc.DanhSachChiNhanh = LayDanhSachPhongBan();

            // Lấy danh sách chấm công theo điều kiện tìm kiếm
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
        // 6. LẤY THÔNG TIN CHẤM CÔNG TRỰC TUYẾN
        // =====================================================

        private bool LayThongTinTrucTuyen(
            string maNV,
            out TrucTuyenViewModel model,
            out string thongBao)
        {
            model = new TrucTuyenViewModel();

            thongBao = "";

            if (string.IsNullOrWhiteSpace(maNV))
            {
                thongBao = "Không xác định được nhân viên đăng nhập.";
                return false;
            }

            try
            {
                using (SqlConnection conn =
                    new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Lấy thông tin nhân viên và phòng ban
                    string sqlNhanVien = @"
                        SELECT
                            nv.sMaNV,
                            nv.sTenNV,
                            pb.sTenPB
                        FROM tbl_NhanVien nv
                        INNER JOIN tbl_PhongBan pb
                            ON nv.FK_sMaPB = pb.sMaPB
                        WHERE nv.sMaNV = @MaNV";

                    using (SqlCommand cmd =
                        new SqlCommand(sqlNhanVien, conn))
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
                                thongBao =
                                    "Không tìm thấy thông tin nhân viên.";

                                return false;
                            }

                            model.MaNV = rd["sMaNV"].ToString();
                            model.TenNV = rd["sTenNV"].ToString();
                            model.TenPB = rd["sTenPB"].ToString();
                        }
                    }

                    model.NgayHienTai = DateTime.Today;
                    model.GioHienTai = DateTime.Now;

                    // Lấy dữ liệu chấm công hôm nay
                    string sqlHomNay = @"
                        SELECT TOP 1 *
                        FROM tbl_ChamCong
                        WHERE FK_sMaNV = @MaNV
                          AND dNgayChamCong = @NgayChamCong";

                    using (SqlCommand cmd =
                        new SqlCommand(sqlHomNay, conn))
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
                            if (rd.Read())
                            {
                                model.GioVao =
                                    rd["dGioVao"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(rd["dGioVao"]);

                                model.GioRa =
                                    rd["dGioRa"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(rd["dGioRa"]);

                                model.SoGioLam =
                                    rd["iSoGioLam"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(rd["iSoGioLam"]);

                                model.TrangThaiChamCong =
                                    rd["sTrangThai"] == DBNull.Value
                                    ? "Không xác định"
                                    : rd["sTrangThai"].ToString();
                            }
                            else
                            {
                                model.TrangThaiChamCong =
                                    "Chưa chấm công";
                            }
                        }
                    }

                    // Lấy lịch sử 7 ngày gần nhất
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
                          AND dNgayChamCong >= DATEADD(
                              DAY, -6, CONVERT(date, GETDATE()))
                        ORDER BY dNgayChamCong DESC";

                    using (SqlCommand cmd =
                        new SqlCommand(sqlLichSu, conn))
                    {
                        cmd.Parameters.Add(
                            "@MaNV",
                            SqlDbType.VarChar,
                            20
                        ).Value = maNV;

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                model.LichSu7Ngay.Add(
                                    ChuyenDuLieuChamCong(rd)
                                );
                            }
                        }
                    }
                }

                thongBao = "Lấy thông tin chấm công thành công.";

                return true;
            }
            catch (Exception ex)
            {
                thongBao =
                    "Lỗi lấy thông tin chấm công: " + ex.Message;

                return false;
            }
        }


        // =====================================================
        // 7. XỬ LÝ CHẤM CÔNG VÀO
        // =====================================================

        private bool XuLyChamCongVao(
    string maNV,
    out ChamCong chamCong,
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
                using (SqlConnection conn =
                    new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra nhân viên đã chấm công hôm nay chưa
                    string sqlKiemTra = @"
                SELECT COUNT(*)
                FROM tbl_ChamCong
                WHERE FK_sMaNV = @MaNV
                  AND dNgayChamCong = @NgayChamCong";

                    using (SqlCommand cmd =
                        new SqlCommand(sqlKiemTra, conn))
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

                        int soLuong =
                            Convert.ToInt32(cmd.ExecuteScalar());

                        if (soLuong > 0)
                        {
                            thongBao =
                                "Bạn đã có bản ghi chấm công hôm nay.";

                            return false;
                        }
                    }

                    DateTime gioVao = DateTime.Now;

                    // Quy định giờ bắt đầu: 09:00
                    TimeSpan gioBatDau = new TimeSpan(9, 0, 0);

                    // Xác định trạng thái
                    string trangThai =
                        gioVao.TimeOfDay > gioBatDau
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

                    using (SqlCommand cmd =
                        new SqlCommand(sqlThem, conn))
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
                        MaNV = maNV,
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
        // 8. XỬ LÝ CHẤM CÔNG RA
        // =====================================================

        private bool XuLyChamCongRa(
            string maNV,
            out ChamCong chamCong,
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
                using (SqlConnection conn =
                    new SqlConnection(connectionString))
                {
                    conn.Open();

                    DateTime gioRa = DateTime.Now;

                    // Quy định giờ kết thúc: 17:00
                    TimeSpan gioKetThuc = new TimeSpan(17, 0, 0);

                    string sql = @"
                UPDATE tbl_ChamCong
                SET
                    dGioRa = @GioRa,
                    iSoGioLam = DATEDIFF(HOUR, dGioVao, @GioRa),
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

                    using (SqlCommand cmd =
                        new SqlCommand(sql, conn))
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
                        thongBao =
                            "Bạn chưa chấm công vào hoặc đã chấm công ra.";

                        return false;
                    }

                    // Lấy lại bản ghi sau khi cập nhật
                    string sqlLay = @"
                SELECT *
                FROM tbl_ChamCong
                WHERE FK_sMaNV = @MaNV
                  AND dNgayChamCong = @NgayChamCong";

                    using (SqlCommand cmd =
                        new SqlCommand(sqlLay, conn))
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
                                thongBao =
                                    "Không tìm thấy dữ liệu chấm công.";

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
        // 9. LẤY LỊCH SỬ CHẤM CÔNG
        // Admin, Manager
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
        ");

                using (SqlConnection conn =
                    new SqlConnection(connectionString))

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    // Chỉ lấy lịch sử của nhân viên đang đăng nhập
                    cmd.Parameters.Add(
                        "@MaNV",
                        SqlDbType.VarChar,
                        20
                    ).Value = maNV;

                    // Lọc từ ngày
                    if (boLoc.TuNgay.HasValue)
                    {
                        sql.Append(
                            " AND dNgayChamCong >= @TuNgay");

                        cmd.Parameters.Add(
                            "@TuNgay",
                            SqlDbType.Date
                        ).Value = boLoc.TuNgay.Value.Date;
                    }

                    // Lọc đến ngày
                    if (boLoc.DenNgay.HasValue)
                    {
                        sql.Append(
                            " AND dNgayChamCong < @DenNgay");

                        cmd.Parameters.Add(
                            "@DenNgay",
                            SqlDbType.Date
                        ).Value = boLoc.DenNgay.Value.Date.AddDays(1);
                    }

                    // Lọc trạng thái
                    if (!string.IsNullOrWhiteSpace(boLoc.TrangThai))
                    {
                        sql.Append(
                            " AND sTrangThai = @TrangThai");

                        cmd.Parameters.Add(
                            "@TrangThai",
                            SqlDbType.NVarChar,
                            50
                        ).Value = boLoc.TrangThai.Trim();
                    }

                    // Sắp xếp ngày mới nhất lên đầu
                    sql.Append(@"
                ORDER BY
                    dNgayChamCong DESC,
                    dGioVao DESC
            ");

                    cmd.CommandText = sql.ToString();

                    conn.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            danhSach.Add(
                                ChuyenDuLieuChamCong(rd)
                            );
                        }
                    }
                }

                // =========================================
                // TÍNH CÁC CHỈ SỐ THỐNG KÊ
                // =========================================

                // Số ngày có bản ghi chấm công
                boLoc.SoNgayCong = danhSach
                    .Select(x => x.NgayChamCong.Date)
                    .Distinct()
                    .Count();

                // Số ngày đến muộn
                boLoc.SoNgayMuon = danhSach
                    .Count(x => x.TrangThai == "Đến muộn");

                // Số ngày ra về sớm
                boLoc.SoNgayVeSom = danhSach
                    .Count(x => x.TrangThai == "Ra về sớm");

                // Số ngày vắng
                boLoc.SoNgayNghi = danhSach
                    .Count(x => x.TrangThai == "Vắng");

                // Tổng số giờ làm
                boLoc.TongSoGioLam = danhSach
                    .Sum(x => (decimal)x.SoGioLam);

                thongBao = "Lấy lịch sử chấm công thành công.";

                return true;
            }
            catch (Exception ex)
            {
                thongBao =
                    "Lỗi lấy lịch sử chấm công: " + ex.Message;

                return false;
            }
        }


        // =====================================================
        // 10. LẤY DỮ LIỆU THEO DÕI CHẤM CÔNG
        // Chỉ Admin
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

                using (SqlConnection conn =
                    new SqlConnection(connectionString))

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (boLoc != null)
                    {
                        // 1. Lọc tên nhân viên
                        if (!string.IsNullOrWhiteSpace(
                            boLoc.TenNhanVien))
                        {
                            sql.Append(
                                " AND nv.sTenNV LIKE @TenNhanVien");

                            cmd.Parameters.Add(
                                "@TenNhanVien",
                                SqlDbType.NVarChar,
                                100
                            ).Value =
                                "%" + boLoc.TenNhanVien.Trim() + "%";
                        }

                        // 2. Lọc từ ngày
                        if (boLoc.TuNgay.HasValue)
                        {
                            sql.Append(
                                " AND cc.dNgayChamCong >= @TuNgay");

                            cmd.Parameters.Add(
                                "@TuNgay",
                                SqlDbType.Date
                            ).Value = boLoc.TuNgay.Value.Date;
                        }

                        // 3. Lọc đến ngày
                        if (boLoc.DenNgay.HasValue)
                        {
                            sql.Append(
                                " AND cc.dNgayChamCong < @DenNgay");

                            cmd.Parameters.Add(
                                "@DenNgay",
                                SqlDbType.Date
                            ).Value =
                                boLoc.DenNgay.Value.Date.AddDays(1);
                        }

                        // 4. Lọc phòng ban
                        if (!string.IsNullOrWhiteSpace(
                            boLoc.MaPhongBan))
                        {
                            sql.Append(
                                " AND nv.FK_sMaPB = @MaPhongBan");

                            cmd.Parameters.Add(
                                "@MaPhongBan",
                                SqlDbType.VarChar,
                                20
                            ).Value = boLoc.MaPhongBan.Trim();
                        }

                        // 5. Lọc giờ vào
                        ThemDieuKienGio(
                            sql,
                            cmd,
                            "cc.dGioVao",
                            boLoc.TuGioVao,
                            boLoc.DenGioVao,
                            "@TuGioVao",
                            "@DenGioVao"
                        );

                        // 6. Lọc giờ ra
                        ThemDieuKienGio(
                            sql,
                            cmd,
                            "cc.dGioRa",
                            boLoc.TuGioRa,
                            boLoc.DenGioRa,
                            "@TuGioRa",
                            "@DenGioRa"
                        );

                        // 7. Lọc trạng thái
                        if (!string.IsNullOrWhiteSpace(
                            boLoc.TrangThai))
                        {
                            sql.Append(
                                " AND cc.sTrangThai = @TrangThai");

                            cmd.Parameters.Add(
                                "@TrangThai",
                                SqlDbType.NVarChar,
                                50
                            ).Value = boLoc.TrangThai.Trim();
                        }
                    }

                    // Sắp xếp ngày mới nhất lên đầu
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
                            danhSach.Add(
                                ChuyenDuLieuChamCong(rd)
                            );
                        }
                    }
                }

                thongBao =
                    "Lấy danh sách theo dõi chấm công thành công.";

                return true;
            }
            catch (Exception ex)
            {
                thongBao =
                    "Lỗi theo dõi chấm công: " + ex.Message;

                return false;
            }
        }


        // =====================================================
        // 11. HÀM PHỤ: THÊM ĐIỀU KIỆN LỌC GIỜ
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
                    tenThamSoTu);

                cmd.Parameters.Add(
                    tenThamSoTu,
                    SqlDbType.Time
                ).Value = tuGio.Value;
            }

            if (denGio.HasValue)
            {
                sql.Append(
                    " AND CAST(" + cotGio + " AS time) <= " +
                    tenThamSoDen);

                cmd.Parameters.Add(
                    tenThamSoDen,
                    SqlDbType.Time
                ).Value = denGio.Value;
            }
        }


        // =====================================================
        // 12. HÀM PHỤ: CHUYỂN SQL DATA READER THÀNH CHAMCONG
        // =====================================================

        private ChamCong ChuyenDuLieuChamCong(SqlDataReader rd)
        {
            int viTriTenNV = -1;

            // Kiểm tra truy vấn có trả về cột sTenNV hay không
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
                MaChamCong = rd["sMaChamCong"].ToString(),

                MaNV = rd["FK_sMaNV"].ToString(),

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
