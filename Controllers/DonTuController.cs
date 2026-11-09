
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NMCNPM.Models;
using System.Data;

namespace NMCNPM.Controllers
{
    public class DonTuController : Controller
    {
        private readonly string _connectionString;

        public DonTuController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối DefaultConnection.");
        }

        // Lấy mã nhân viên đang đăng nhập
        private string? LayMaNV()
        {
            return HttpContext.Session.GetString("sMaNV");
        }

        // Kiểm tra quyền quản lý
        private bool LaQuanLy()
        {
            string? quyen = HttpContext.Session.GetString("sMaQuyen");
            return quyen == "Q_MANAGER" || quyen == "Q_ADMIN";
        }

        // 1. Danh sách đơn của nhân viên
        public IActionResult Index()
        {
            string? maNV = LayMaNV();

            if (string.IsNullOrEmpty(maNV))
                return RedirectToAction("Login", "Account");

            List<DonTu> danhSach = new List<DonTu>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT *
                    FROM vw_DanhSachDonTu
                    WHERE FK_sMaNV = @MaNV
                    ORDER BY dNgayGui DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNV;

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new DonTu
                            {
                                sMaDon = reader["sMaDon"].ToString()!,
                                FK_sMaNV = reader["FK_sMaNV"].ToString()!,
                                sTenNguoiTao = reader["sTenNguoiTao"].ToString(),
                                sLoaiDon = reader["sLoaiDon"].ToString()!,
                                dNgayGui = Convert.ToDateTime(reader["dNgayGui"]),
                                sLyDo = reader["sLyDo"].ToString()!,
                                sTrangThai = reader["sTrangThai"].ToString(),
                                sNguoiDuyet = reader["sNguoiDuyet"] == DBNull.Value
                                    ? null : reader["sNguoiDuyet"].ToString(),
                                sTenNguoiDuyet = reader["sTenNguoiDuyet"] == DBNull.Value
                                    ? null : reader["sTenNguoiDuyet"].ToString()
                            });
                        }
                    }
                }
            }

            return View(danhSach);
        }

        // 2. Mở giao diện tạo đơn
        [HttpGet]
        public IActionResult TaoDon()
        {
            if (string.IsNullOrEmpty(LayMaNV()))
                return RedirectToAction("Login", "Account");

            return View(new DonTu());
        }

        // 3. Xử lý tạo đơn mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TaoDon(DonTu model)
        {
            string? maNV = LayMaNV();

            if (string.IsNullOrEmpty(maNV))
                return RedirectToAction("Login", "Account");

            string[] loaiDonHopLe =
            {
                "Nghỉ phép",
                "Đổi ca",
                "Giải trình"
            };

            if (!loaiDonHopLe.Contains(model.sLoaiDon))
                ModelState.AddModelError("sLoaiDon", "Loại đơn không hợp lệ.");

            if (string.IsNullOrWhiteSpace(model.sLyDo))
                ModelState.AddModelError("sLyDo", "Vui lòng nhập lý do.");

            if (!ModelState.IsValid)
                return View(model);

            string maDon = "DT" + Guid.NewGuid().ToString("N")
                .Substring(0, 10).ToUpper();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO tbl_DonTu
                    (
                        sMaDon,
                        FK_sMaNV,
                        sLoaiDon,
                        dNgayGui,
                        sLyDo,
                        sTrangThai,
                        sNguoiDuyet
                    )
                    VALUES
                    (
                        @MaDon,
                        @MaNV,
                        @LoaiDon,
                        GETDATE(),
                        @LyDo,
                        N'Chờ duyệt',
                        NULL
                    )";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@MaDon", SqlDbType.VarChar, 20).Value = maDon;
                    cmd.Parameters.Add("@MaNV", SqlDbType.VarChar, 20).Value = maNV;
                    cmd.Parameters.Add("@LoaiDon", SqlDbType.NVarChar, 50).Value = model.sLoaiDon;
                    cmd.Parameters.Add("@LyDo", SqlDbType.NVarChar, 500).Value = model.sLyDo.Trim();

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["ThongBao"] = "Gửi đơn thành công!";
            return RedirectToAction("Index");
        }

        // 4. Quản lý xem danh sách đơn chờ duyệt
        [HttpGet]
        public IActionResult PheDuyet()
        {
            if (!LaQuanLy())
                return RedirectToAction("Index", "Home");

            List<DonTu> danhSach = new List<DonTu>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT *
                    FROM vw_DanhSachDonTu
                    WHERE sTrangThai = N'Chờ duyệt'
                    ORDER BY dNgayGui ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new DonTu
                            {
                                sMaDon = reader["sMaDon"].ToString()!,
                                FK_sMaNV = reader["FK_sMaNV"].ToString()!,
                                sTenNguoiTao = reader["sTenNguoiTao"].ToString(),
                                sLoaiDon = reader["sLoaiDon"].ToString()!,
                                dNgayGui = Convert.ToDateTime(reader["dNgayGui"]),
                                sLyDo = reader["sLyDo"].ToString()!,
                                sTrangThai = reader["sTrangThai"].ToString()
                            });
                        }
                    }
                }
            }

            return View(danhSach);
        }

        // 5. Quản lý duyệt hoặc từ chối đơn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XuLyDon(string sMaDon, string sTrangThai)
        {
            string? maNguoiDuyet = LayMaNV();

            if (!LaQuanLy() || string.IsNullOrEmpty(maNguoiDuyet))
                return RedirectToAction("Index", "Home");

            if (string.IsNullOrWhiteSpace(sMaDon) ||
                (sTrangThai != "Đã duyệt" && sTrangThai != "Từ chối"))
            {
                TempData["ThongBao"] = "Thông tin xử lý đơn không hợp lệ.";
                return RedirectToAction("PheDuyet");
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE tbl_DonTu
                    SET
                        sTrangThai = @TrangThai,
                        sNguoiDuyet = @NguoiDuyet,
                        dNgayDuyet = GETDATE()
                    WHERE
                        sMaDon = @MaDon
                        AND sTrangThai = N'Chờ duyệt'
                        AND FK_sMaNV <> @NguoiDuyet";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@TrangThai", SqlDbType.NVarChar, 50).Value = sTrangThai;
                    cmd.Parameters.Add("@NguoiDuyet", SqlDbType.VarChar, 20).Value = maNguoiDuyet;
                    cmd.Parameters.Add("@MaDon", SqlDbType.VarChar, 20).Value = sMaDon;

                    conn.Open();

                    int soDong = cmd.ExecuteNonQuery();

                    TempData["ThongBao"] = soDong > 0
                        ? "Xử lý đơn thành công!"
                        : "Không thể xử lý đơn này. Đơn có thể đã được duyệt hoặc bạn không thể tự duyệt đơn của mình.";
                }
            }

            return RedirectToAction("PheDuyet");
        }
    }
}
