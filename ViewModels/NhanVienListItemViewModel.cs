using TocoTocoHRM.Models;
namespace TocoTocoHRM.ViewModels;
public class NhanVienListItemViewModel
{
    public NhanVien NhanVien { get; set; } = new();
    public string TenPB { get; set; } = "";
    public string TenCV { get; set; } = "";
}
