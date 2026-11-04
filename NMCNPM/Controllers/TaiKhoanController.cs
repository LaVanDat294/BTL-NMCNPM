using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using NMCNPM.Models;
using NMCNPM.Helpers;

namespace NMCNPM.Controllers
{
    public class TaiKhoanController : Controller
    {
        // Chuỗi kết nối trực tiếp đến cơ sở dữ liệu (không cần thông qua tầng Repository)
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["HRMConnection"].ConnectionString;

        // =======================================================
        // 1. GIAO DIỆN ĐĂNG NHẬP
        // =======================================================

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập thì chuyển thẳng về trang chủ
            if (Session["MaTK"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // =======================================================
        // 2. XỬ LÝ ĐĂNG NHẬP (TRUY VẤN TRỰC TIẾP ADO.NET SQL)
        // =======================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            try
            {
                TaiKhoan tk = null;

                // Truy vấn trực tiếp tài khoản từ cơ sở dữ liệu
                string sql = @"
                    SELECT 
                        tk.sMaTK,
                        tk.FK_sMaNV,
                        tk.sTenDangNhap,
                        tk.sMatKhauHash,
                        tk.FK_sMaQuyen,
                        q.sTenQuyen,
                        nv.sTenNV,
                        tk.bTrangThai
                    FROM tbl_TaiKhoan tk
                    INNER JOIN tbl_NhanVien nv ON tk.FK_sMaNV = nv.sMaNV
                    INNER JOIN tbl_Quyen q ON tk.FK_sMaQuyen = q.sMaQuyen
                    WHERE tk.sTenDangNhap = @TenDangNhap OR tk.sMaTK = @TenDangNhap";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", model.TenDangNhap.Trim());
                    conn.Open();

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            tk = new TaiKhoan
                            {
                                MaTK = rd["sMaTK"].ToString(),
                                MaNV = rd["FK_sMaNV"].ToString(),
                                TenDangNhap = rd["sTenDangNhap"].ToString(),
                                MatKhauHash = rd["sMatKhauHash"].ToString(),
                                MaQuyen = rd["FK_sMaQuyen"].ToString(),
                                TenQuyen = rd["sTenQuyen"].ToString(),
                                TenNV = rd["sTenNV"].ToString(),
                                TrangThai = Convert.ToBoolean(rd["bTrangThai"])
                            };
                        }
                    }
                }

                // Kiểm tra tài khoản có tồn tại không
                if (tk == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                // Kiểm tra trạng thái hoạt động của tài khoản
                if (!tk.TrangThai)
                {
                    ModelState.AddModelError("", "Tài khoản đã bị khóa hoặc ngừng hoạt động.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                // Kiểm tra mật khẩu (hỗ trợ cả hash PBKDF2 và plain text)
                bool validPassword = PasswordHelper.VerifyPassword(model.MatKhau, tk.MatKhauHash);
                if (!validPassword)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }

                // Lưu thông tin người dùng vào Session
                Session.Clear();
                Session["MaTK"] = tk.MaTK;
                Session["MaNV"] = tk.MaNV;
                Session["TenDangNhap"] = tk.TenDangNhap;
                Session["HoTen"] = tk.TenNV;
                Session["MaQuyen"] = tk.MaQuyen;
                Session["TenQuyen"] = tk.TenQuyen;

                // Tạo Cookie xác thực đăng nhập (FormsAuthentication)
                string userData = tk.MaTK + "|" + tk.MaNV + "|" + tk.MaQuyen;
                DateTime expiration = model.GhiNho ? DateTime.Now.AddDays(7) : DateTime.Now.AddMinutes(30);

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    tk.TenDangNhap,
                    DateTime.Now,
                    expiration,
                    model.GhiNho,
                    userData,
                    FormsAuthentication.FormsCookiePath
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection
                };

                if (model.GhiNho)
                {
                    cookie.Expires = expiration;
                }

                Response.Cookies.Add(cookie);

                // Điều hướng về trang trước đó (nếu có) hoặc về thẳng Trang chủ
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("tbl_TaiKhoan"))
                {
                    ModelState.AddModelError("", "Cơ sở dữ liệu chưa có bảng tbl_TaiKhoan. Bạn hãy chạy file FULL_DATABASE_HRM_TOCOTOCO.sql trong SQL Server!");
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi kết nối cơ sở dữ liệu: " + ex.Message);
                }

                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

        // =======================================================
        // 3. ĐĂNG XUẤT
        // =======================================================

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            FormsAuthentication.SignOut();

            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Login");
        }

        // =======================================================
        // 4. ĐĂNG KÝ TÀI KHOẢN
        // =======================================================

        [HttpGet]
        public ActionResult Register()
        {
            try
            {
                bool hasAny = HasAnyAccountDirect();

                if (hasAny)
                {
                    if (Session["MaTK"] == null ||
                        Session["MaQuyen"] == null ||
                        Session["MaQuyen"].ToString() != "Q_ADMIN")
                    {
                        return RedirectToAction("Login");
                    }
                }

                var model = new RegisterViewModel();
                if (!hasAny)
                {
                    model.MaQuyen = "Q_ADMIN";
                    model.MaNV = "NV_ADMIN";
                }

                return View(model);
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            bool hasAny = false;
            try
            {
                hasAny = HasAnyAccountDirect();
            }
            catch { }

            if (hasAny)
            {
                if (Session["MaTK"] == null ||
                    Session["MaQuyen"] == null ||
                    Session["MaQuyen"].ToString() != "Q_ADMIN")
                {
                    return RedirectToAction("Login");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(model.MaQuyen))
                {
                    model.MaQuyen = "Q_ADMIN";
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            string maTK = "TK" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string hash = PasswordHelper.HashPassword(model.MatKhau);

            string sql = @"
                INSERT INTO tbl_TaiKhoan
                (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
                VALUES
                (@MaTK, @MaNV, @TenDangNhap, @MatKhauHash, @MaQuyen, 1)";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaTK", maTK);
                    cmd.Parameters.AddWithValue("@MaNV", model.MaNV.Trim());
                    cmd.Parameters.AddWithValue("@TenDangNhap", model.TenDangNhap.Trim());
                    cmd.Parameters.AddWithValue("@MatKhauHash", hash);
                    cmd.Parameters.AddWithValue("@MaQuyen", model.MaQuyen);

                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();

                    if (affected > 0)
                    {
                        TempData["Success"] = "Tạo tài khoản thành công!";
                        if (!hasAny)
                        {
                            return RedirectToAction("Login");
                        }
                        return RedirectToAction("Register");
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    ModelState.AddModelError("", "Nhân viên đã có tài khoản hoặc tên đăng nhập đã tồn tại.");
                }
                else if (ex.Number == 547)
                {
                    ModelState.AddModelError("", "Mã nhân viên hoặc mã quyền không tồn tại.");
                }
                else
                {
                    ModelState.AddModelError("", "Không thể lưu tài khoản vào cơ sở dữ liệu: " + ex.Message);
                }
                return View(model);
            }

            return View(model);
        }

        // =======================================================
        // 5. ĐỔI MẬT KHẨU
        // =======================================================

        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            if (Session["MaTK"] == null)
            {
                return RedirectToAction("Login");
            }

            return View(new DoiMatKhauViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (Session["MaTK"] == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string maTK = Session["MaTK"].ToString();
            string currentStoredHash = null;
            bool trangThai = false;

            try
            {
                string sqlSelect = "SELECT sMatKhauHash, bTrangThai FROM tbl_TaiKhoan WHERE sMaTK = @MaTK";
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sqlSelect, conn))
                {
                    cmd.Parameters.AddWithValue("@MaTK", maTK);
                    conn.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            currentStoredHash = rd["sMatKhauHash"].ToString();
                            trangThai = Convert.ToBoolean(rd["bTrangThai"]);
                        }
                    }
                }

                if (currentStoredHash == null || !trangThai)
                {
                    Session.Clear();
                    FormsAuthentication.SignOut();
                    return RedirectToAction("Login");
                }

                bool checkPassword = PasswordHelper.VerifyPassword(model.MatKhauCu, currentStoredHash);
                if (!checkPassword)
                {
                    ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không chính xác.");
                    return View(model);
                }

                string newHash = PasswordHelper.HashPassword(model.MatKhauMoi);
                string sqlUpdate = "UPDATE tbl_TaiKhoan SET sMatKhauHash = @MatKhauHash WHERE sMaTK = @MaTK AND bTrangThai = 1";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sqlUpdate, conn))
                {
                    cmd.Parameters.AddWithValue("@MatKhauHash", newHash);
                    cmd.Parameters.AddWithValue("@MaTK", maTK);
                    conn.Open();

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        Session.Clear();
                        Session.Abandon();
                        FormsAuthentication.SignOut();

                        TempData["Success"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
                        return RedirectToAction("Login");
                    }
                }

                ModelState.AddModelError("", "Không thể cập nhật mật khẩu.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật mật khẩu: " + ex.Message);
                return View(model);
            }
        }

        // =======================================================
        // 6. KHÔNG CÓ QUYỀN TRUY CẬP
        // =======================================================

        public ActionResult AccessDenied()
        {
            return View();
        }

        // =======================================================
        // HÀM TIỆN ÍCH NỘI BỘ (TRUY VẤN TRỰC TIẾP)
        // =======================================================

        private bool HasAnyAccountDirect()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM tbl_TaiKhoan", conn))
            {
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
