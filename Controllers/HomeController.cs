using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TocoTocoHRM.Data;

namespace TocoTocoHRM.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly HrmDbContext _db;
    public HomeController(HrmDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.TongNhanVien = await _db.NhanViens.CountAsync();
        ViewBag.DangLam = await _db.NhanViens.CountAsync(x => x.TrangThai == "Đang làm việc");
        ViewBag.PhongBan = await _db.PhongBans.CountAsync();
        ViewBag.NghiViec = await _db.NhanViens.CountAsync(x => x.TrangThai != "Đang làm việc");
        return View();
    }
}
