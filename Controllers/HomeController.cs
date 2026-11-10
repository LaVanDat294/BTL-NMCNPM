using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NMCNPM.Models;

namespace NMCNPM.Controllers
{
    public class HomeController : Controller
    {
        private readonly HrmDbContext _context;

        public HomeController(HrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalNhanVien = await _context.NhanViens.CountAsync();
            ViewBag.TotalPhongBan = await _context.PhongBans.CountAsync();
            ViewBag.TotalChucVu = await _context.ChucVus.CountAsync();
            ViewBag.TotalHopDong = await _context.HopDongs.CountAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
