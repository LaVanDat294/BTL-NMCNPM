
using System.ComponentModel.DataAnnotations;

namespace NMCNPM.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên")]
        public string MaNV { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quyền")]
        public string MaQuyen { get; set; }
    }
}
