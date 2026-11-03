using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using NMCNPM.Models;
using NMCNPM.Data;
using NMCNPM.Helpers;

namespace NMCNPM.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly TaiKhoanRepository repo =
            new TaiKhoanRepository();

        // =========================
        // 1. GIAO DIỆN ĐĂNG NHẬP
        // =========================

        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            if (Session["MaTK"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        // =========================
        // 2. XỬ LÝ ĐĂNG NHẬP
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tìm tài khoản
            TaiKhoan tk = repo.GetByMaTK(
                model.TenDangNhap.Trim());

            // Kiểm tra tài khoản
            if (tk == null)
            {
                ModelState.AddModelError("",
                    "Tên đăng nhập hoặc mật khẩu không chính xác.");

                return View(model);
            }

            // Kiểm tra trạng thái
            if (!tk.TrangThai)
            {
                ModelState.AddModelError("",
                    "Tài khoản đã bị khóa hoặc ngừng hoạt động.");

                return View(model);
            }

            // Kiểm tra mật khẩu
            bool validPassword =
                PasswordHelper.VerifyPassword(
                    model.MatKhau,
                    tk.MatKhauHash);

            if (!validPassword)
            {
                ModelState.AddModelError("",
                    "Tên đăng nhập hoặc mật khẩu không chính xác.");

                return View(model);
            }

            // Lưu thông tin vào Session
            Session.Clear();

            Session["MaTK"] = tk.MaTK;
            Session["MaNV"] = tk.MaNV;
            Session["TenDangNhap"] = tk.TenDangNhap;
            Session["HoTen"] = tk.TenNV;
            Session["MaQuyen"] = tk.MaQuyen;
            Session["TenQuyen"] = tk.TenQuyen;

            // Tạo Cookie đăng nhập
            string userData =
                tk.MaTK + "|" + tk.MaNV + "|" + tk.MaQuyen;

            DateTime expiration = model.GhiNho
                ? DateTime.Now.AddDays(7)
                : DateTime.Now.AddMinutes(30);

            FormsAuthenticationTicket ticket =
                new FormsAuthenticationTicket(
                    1,
                    tk.TenDangNhap,
                    DateTime.Now,
                    expiration,
                    model.GhiNho,
                    userData,
                    FormsAuthentication.FormsCookiePath
                );

            string encryptedTicket =
                FormsAuthentication.Encrypt(ticket);

            HttpCookie cookie = new HttpCookie(
                FormsAuthentication.FormsCookieName,
                encryptedTicket
            );

            cookie.HttpOnly = true;
            cookie.Secure = Request.IsSecureConnection;

            if (model.GhiNho)
            {
                cookie.Expires = expiration;
            }

            Response.Cookies.Add(cookie);

            // Chuyển đến trang chủ
            return RedirectToAction("Index", "Home");
        }

        // =========================
        // 3. ĐĂNG XUẤT
        // =========================

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            FormsAuthentication.SignOut();

            if (Request.Cookies[
                FormsAuthentication.FormsCookieName] != null)
            {
                HttpCookie cookie = new HttpCookie(
                    FormsAuthentication.FormsCookieName);

                cookie.Expires = DateTime.Now.AddDays(-1);
                cookie.HttpOnly = true;

                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Login");
        }


        // =========================
        // 4. GIAO DIỆN ĐỔI MẬT KHẨU
        // =========================

        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            if (Session["MaTK"] == null)
            {
                return RedirectToAction("Login");
            }

            return View(new DoiMatKhauViewModel());
        }


        [HttpGet]
        public ActionResult Register()
        {
            if (Session["MaTK"] == null ||
                Session["MaQuyen"] == null ||
                Session["MaQuyen"].ToString() != "Q_ADMIN")
            {
                return RedirectToAction("Login");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (Session["MaTK"] == null ||
                Session["MaQuyen"] == null ||
                Session["MaQuyen"].ToString() != "Q_ADMIN")
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Tạo mã tài khoản
            string maTK = "TK" +
                Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            // Tạo mã băm mật khẩu
            string hash = PasswordHelper.HashPassword(model.MatKhau);

            TaiKhoan tk = new TaiKhoan
            {
                MaTK = maTK,
                MaNV = model.MaNV.Trim(),
                TenDangNhap = model.TenDangNhap.Trim(),
                MatKhauHash = hash,
                MaQuyen = model.MaQuyen,
                TrangThai = true
            };

            try
            {
                bool result = repo.CreateAccount(tk);

                if (result)
                {
                    TempData["Success"] = "Tạo tài khoản thành công!";
                    return RedirectToAction("Register");
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    ModelState.AddModelError("",
                        "Nhân viên đã có tài khoản hoặc tên đăng nhập đã tồn tại.");
                }
                else if (ex.Number == 547)
                {
                    ModelState.AddModelError("",
                        "Mã nhân viên hoặc mã quyền không tồn tại.");
                }
                else
                {
                    ModelState.AddModelError("",
                        "Không thể lưu tài khoản vào cơ sở dữ liệu.");
                }

                return View(model);
            }

            return View(model);
        }

        // =========================
        // 5. XỬ LÝ ĐỔI MẬT KHẨU
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(
            DoiMatKhauViewModel model)
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

            // Lấy tài khoản
            TaiKhoan tk = repo.GetByMaTK(maTK);

            if (tk == null || !tk.TrangThai)
            {
                Session.Clear();
                FormsAuthentication.SignOut();

                return RedirectToAction("Login");
            }

            bool checkPassword =
             PasswordHelper.VerifyPassword(
                 model.MatKhauCu,
                 tk.MatKhauHash);

            if (!checkPassword)
            {
                ModelState.AddModelError(
                    "MatKhauCu",
                    "Mật khẩu hiện tại không chính xác.");

                return View(model);
            }

            // Tạo mật khẩu mới
            string newHash =
                PasswordHelper.HashPassword(
                    model.MatKhauMoi);

            // Cập nhật SQL Server
            bool result =
                repo.UpdatePassword(maTK, newHash);

            if (result)
            {
                // Đăng xuất các phiên đăng nhập hiện tại
                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();

                TempData["Success"] =
                    "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";

                return RedirectToAction("Login");
            }

            ModelState.AddModelError("",
                "Không thể cập nhật mật khẩu.");

            return View(model);
        }

        // =========================
        // 6. KHÔNG CÓ QUYỀN TRUY CẬP
        // =========================

        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}