using System.ComponentModel.DataAnnotations;
namespace TocoTocoHRM.ViewModels;
public class LoginViewModel
{
    [Required(ErrorMessage="Vui lòng nhập tên đăng nhập.")]
    public string TenDangNhap { get; set; } = "";
    [Required(ErrorMessage="Vui lòng nhập mật khẩu."), DataType(DataType.Password)]
    public string MatKhau { get; set; } = "";
    public bool GhiNho { get; set; }
}
