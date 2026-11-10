using Microsoft.AspNetCore.Mvc.Rendering;
using TocoTocoHRM.Models;
namespace TocoTocoHRM.ViewModels;
public class NhanVienFormViewModel
{
    public NhanVien NhanVien { get; set; } = new();
    public List<SelectListItem> PhongBans { get; set; } = new();
    public List<SelectListItem> ChucVus { get; set; } = new();
}
