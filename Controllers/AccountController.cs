using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TocoTocoHRM.Data;
using TocoTocoHRM.Services;
using TocoTocoHRM.ViewModels;

namespace TocoTocoHRM.Controllers;

public class AccountController : Controller
{
    private readonly HrmDbContext _db;

    public AccountController(HrmDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var tenDangNhap = model.TenDangNhap.Trim();

        var account = await _db.TaiKhoans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

        if (account == null)
        {
            ModelState.AddModelError("", "Không tìm thấy tài khoản admin trong database.");
            return View(model);
        }

        if (!account.TrangThai)
        {
            ModelState.AddModelError("", "Tài khoản đang bị khóa.");
            return View(model);
        }

        if (!PasswordUtil.Verify(model.MatKhau, account.MatKhauHash))
        {
            ModelState.AddModelError("", "Mật khẩu không khớp với dữ liệu trong database.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, account.TenDangNhap),
            new Claim(ClaimTypes.NameIdentifier, account.MaNV),
            new Claim(ClaimTypes.Role, account.MaQuyen)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = model.GhiNho,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }
}
