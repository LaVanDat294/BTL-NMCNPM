using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_NhanVien")]
    public class NhanVien
    {
        [Key, Column("sMaNV"), StringLength(20)]
        public string SMaNV { get; set; } = null!;

        [Required, Column("sTenNV"), StringLength(100)]
        public string STenNV { get; set; } = null!;

        [Column("dNgaySinh")]
        public DateTime? DNgaySinh { get; set; }

        [Required, Column("sCCCD"), RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải là 12 số")]
        public string SCCCD { get; set; } = null!;

        [Column("sGioiTinh"), StringLength(10)]
        public string? SGioiTinh { get; set; }

        [Column("sDiaChi"), StringLength(255)]
        public string? SDiaChi { get; set; }

        [Column("sSDT"), RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải là 10 số")]
        public string? SSDT { get; set; }

        [Column("sEmail"), EmailAddress, StringLength(100)]
        public string? SEmail { get; set; }

        [Column("dNgayVaoLam")]
        public DateTime? DNgayVaoLam { get; set; }

        [Column("sTrangThai"), StringLength(50)]
        public string? STrangThai { get; set; }

        [Required, Column("FK_sMaCV"), StringLength(20)]
        public string FK_sMaCV { get; set; } = null!;

        [Required, Column("FK_sMaPB"), StringLength(20)]
        public string FK_sMaPB { get; set; } = null!;

        [Column("fLuongcb", TypeName = "decimal(18,0)")]
        public decimal FLuongcb { get; set; }

        [ForeignKey("FK_sMaCV")]
        public virtual ChucVu? ChucVu { get; set; }

        [ForeignKey("FK_sMaPB")]
        public virtual PhongBan? PhongBan { get; set; }
    }
}