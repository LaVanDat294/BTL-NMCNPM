using NMCNPM.Models;
namespace NMCNPM.ViewModels;
public class NhanVienListItemViewModel
{
    public NhanVien NhanVien { get; set; } = new();
    public string TenPB { get; set; } = "";
    public string TenCV { get; set; } = "";
}
