using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly IConfiguration _configuration;

        public TaiKhoanController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
 
        // 2. XỬ LÝ ĐĂNG NHẬP

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string tenDangNhap,
            string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) ||
                string.IsNullOrWhiteSpace(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection")!;

            using SqlConnection conn = new SqlConnection(connectionString);

            try
            {
                await conn.OpenAsync();

                string sql = @"
                    SELECT
                        tk.sMaTK,
                        tk.FK_sMaNV,
                        tk.sTenDangNhap,
                        tk.sMatKhauHash,
                        tk.FK_sMaQuyen,
                        tk.bTrangThai,
                        nv.sTenNV,
                        q.sTenQuyen
                    FROM tbl_TaiKhoan tk
                    INNER JOIN tbl_NhanVien nv
                        ON tk.FK_sMaNV = nv.sMaNV
                    INNER JOIN tbl_Quyen q
                        ON tk.FK_sMaQuyen = q.sMaQuyen
                    WHERE tk.sTenDangNhap = @TenDangNhap";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@TenDangNhap", tenDangNhap.Trim());

                using SqlDataReader reader =
                    await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    ViewBag.Error =
                        "Tên đăng nhập hoặc mật khẩu không chính xác!";

                    return View();
                }

                // Kiểm tra trạng thái tài khoản
                bool trangThai =
                    Convert.ToBoolean(reader["bTrangThai"]);

                if (!trangThai)
                {
                    ViewBag.Error =
                        "Tài khoản đã bị khóa!";

                    return View();
                }

                // Kiểm tra mật khẩu
                string matKhauHash =
                    reader["sMatKhauHash"].ToString()!;

                bool hopLe =
                    PasswordHelper.VerifyPassword(
                        matKhau, matKhauHash);

                if (!hopLe)
                {
                    ViewBag.Error =
                        "Tên đăng nhập hoặc mật khẩu không chính xác!";

                    return View();
                }

                // Lấy thông tin tài khoản
                string maTK =
                    reader["sMaTK"].ToString()!;

                string maNV =
                    reader["FK_sMaNV"].ToString()!;

                string tenNV =
                    reader["sTenNV"].ToString()!;

                string maQuyen =
                    reader["FK_sMaQuyen"].ToString()!;

                string tenQuyen =
                    reader["sTenQuyen"].ToString()!;

                // Tạo Claims lưu thông tin đăng nhập
                var claims = new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier, maTK),

                    new Claim(
                        ClaimTypes.Name, tenDangNhap.Trim()),

                    new Claim(
                        ClaimTypes.Role, maQuyen),

                    new Claim("MaNV", maNV),

                    new Claim("TenNV", tenNV),

                    new Claim("TenQuyen", tenQuyen)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal =
                    new ClaimsPrincipal(identity);

                // Tạo Cookie đăng nhập
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.UtcNow
                            .AddMinutes(60)
                    }
                );

                // Chuyển đến trang chủ
                return RedirectToAction("Index", "Home");
            }
            catch (SqlException)
            {
                ViewBag.Error =
                    "Không thể kết nối cơ sở dữ liệu!";

                return View();
            }
        }

        // 3. ĐĂNG XUẤT

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login");
        }

        // 4. HIỂN THỊ TRANG ĐỔI MẬT KHẨU
        [Authorize]
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            return View();
        }

        // 5. XỬ LÝ ĐỔI MẬT KHẨU

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(
            string matKhauCu,
            string matKhauMoi,
            string xacNhanMatKhau)
        {
            // Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(matKhauCu) ||
                string.IsNullOrWhiteSpace(matKhauMoi) ||
                string.IsNullOrWhiteSpace(xacNhanMatKhau))
            {
                ViewBag.Error =
                    "Vui lòng nhập đầy đủ thông tin!";

                return View();
            }

            if (matKhauMoi.Length < 8)
            {
                ViewBag.Error =
                    "Mật khẩu mới phải có ít nhất 8 ký tự!";

                return View();
            }

            if (matKhauMoi != xacNhanMatKhau)
            {
                ViewBag.Error =
                    "Mật khẩu xác nhận không khớp!";

                return View();
            }

            if (matKhauCu == matKhauMoi)
            {
                ViewBag.Error =
                    "Mật khẩu mới phải khác mật khẩu cũ!";

                return View();
            }

            string maNV =
                User.FindFirst("MaNV")?.Value ?? "";

            if (string.IsNullOrEmpty(maNV))
            {
                return RedirectToAction("Login");
            }

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection")!;

            using SqlConnection conn =
                new SqlConnection(connectionString);

            try
            {
                await conn.OpenAsync();

                // Lấy mật khẩu hiện tại
                string sql = @"
                    SELECT sMatKhauHash
                    FROM tbl_TaiKhoan
                    WHERE FK_sMaNV = @MaNV
                    AND bTrangThai = 1";

                using SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@MaNV", maNV);

                object? result =
                    await cmd.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    ViewBag.Error =
                        "Không tìm thấy tài khoản!";

                    return View();
                }

                string matKhauHash =
                    result.ToString()!;

                // Xác thực mật khẩu cũ
                if (!PasswordHelper.VerifyPassword(
                    matKhauCu, matKhauHash))
                {
                    ViewBag.Error =
                        "Mật khẩu hiện tại không chính xác!";

                    return View();
                }

                // Mã hóa mật khẩu mới
                string hashMoi =
                    PasswordHelper.HashPassword(matKhauMoi);

                // Cập nhật mật khẩu
                string updateSql = @"
                    UPDATE tbl_TaiKhoan
                    SET sMatKhauHash = @HashMoi
                    WHERE FK_sMaNV = @MaNV
                    AND bTrangThai = 1";

                using SqlCommand updateCmd =
                    new SqlCommand(updateSql, conn);

                updateCmd.Parameters.AddWithValue(
                    "@HashMoi", hashMoi);

                updateCmd.Parameters.AddWithValue(
                    "@MaNV", maNV);

                int rows =
                    await updateCmd.ExecuteNonQueryAsync();

                if (rows > 0)
                {
                    ViewBag.Success =
                        "Đổi mật khẩu thành công!";
                }
                else
                {
                    ViewBag.Error =
                        "Đổi mật khẩu thất bại!";
                }

                return View();
            }
            catch (SqlException)
            {
                ViewBag.Error =
                    "Có lỗi xảy ra khi cập nhật cơ sở dữ liệu!";

                return View();
            }
        }

        // 6. TRANG KHÔNG CÓ QUYỀN TRUY CẬP

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
