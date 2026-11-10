using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;
using NMCNPM.Services;
using NMCNPM.ViewModels;

namespace NMCNPM.Controllers
{
    public class AccountController : Controller
    {
        private readonly HrmDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(HrmDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .Include(t => t.Quyen)
                .FirstOrDefaultAsync(t => t.STenDangNhap == model.TenDangNhap);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (!taiKhoan.BTrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đang bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            bool isPasswordValid = PasswordUtil.Verify(model.MatKhau, taiKhoan.SMatKhauHash);
            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.SMaTK),
                new Claim(ClaimTypes.Name, taiKhoan.NhanVien?.STenNV ?? taiKhoan.STenDangNhap),
                new Claim("Username", taiKhoan.STenDangNhap),
                new Claim("MaNV", taiKhoan.FK_sMaNV),
                new Claim(ClaimTypes.Role, taiKhoan.Quyen?.STenQuyen ?? taiKhoan.FK_sMaQuyen),
                new Claim("MaQuyen", taiKhoan.FK_sMaQuyen)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNho,
                ExpiresUtc = model.GhiNho ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Người dùng {Username} đăng nhập thành công.", taiKhoan.STenDangNhap);

            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.MatKhauCu == model.MatKhauMoi)
            {
                ModelState.AddModelError("MatKhauMoi", "Mật khẩu mới không được trùng với mật khẩu hiện tại.");
                return View(model);
            }

            var username = User.FindFirst("Username")?.Value ?? User.Identity?.Name;
            var maTK = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.SMaTK == maTK || t.STenDangNhap == username);

            if (taiKhoan == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            if (!PasswordUtil.Verify(model.MatKhauCu, taiKhoan.SMatKhauHash))
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            taiKhoan.SMatKhauHash = PasswordUtil.Hash(model.MatKhauMoi);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Người dùng {Username} đã đổi mật khẩu thành công.", taiKhoan.STenDangNhap);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
