using System.ComponentModel.DataAnnotations;

namespace TocoTocoHRM.Models;

public class NhanVien
{
    [Key, Required, StringLength(20, MinimumLength=2, ErrorMessage="Mã nhân viên từ 2 đến 20 ký tự.")]
    public string MaNV { get; set; } = "";
    [Required, StringLength(100)]
    public string TenNV { get; set; } = "";
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }
    [Required, StringLength(12, MinimumLength=9, ErrorMessage="CCCD phải từ 9 đến 12 ký tự.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage="CCCD chỉ được chứa chữ số.")]
    public string CCCD { get; set; } = "";
    [StringLength(10)]
    public string? GioiTinh { get; set; }
    [StringLength(255)]
    public string? DiaChi { get; set; }
    [Phone, StringLength(15)]
    public string? SDT { get; set; }
    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }
    [DataType(DataType.Date)]
    public DateTime? NgayVaoLam { get; set; }
    [StringLength(50)]
    public string TrangThai { get; set; } = "Đang làm việc";
    [Required]
    public string MaCV { get; set; } = "";
    [Required]
    public string MaPB { get; set; } = "";
    [Range(0, 999999999999)]
    public decimal LuongCB { get; set; }
}
