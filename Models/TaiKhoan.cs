using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_TaiKhoan")]
    public class TaiKhoan
    {
        [Key, Column("sMaTK"), StringLength(20)]
        public string SMaTK { get; set; } = null!;

        [Required, Column("FK_sMaNV"), StringLength(20)]
        public string FK_sMaNV { get; set; } = null!;

        [Required, Column("sTenDangNhap"), StringLength(50)]
        public string STenDangNhap { get; set; } = null!;

        [Required, Column("sMatKhauHash"), StringLength(255)]
        public string SMatKhauHash { get; set; } = null!;

        [Required, Column("FK_sMaQuyen"), StringLength(20)]
        public string FK_sMaQuyen { get; set; } = null!;

        [Column("bTrangThai")]
        public bool BTrangThai { get; set; } = true;

        [ForeignKey("FK_sMaNV")]
        public virtual NhanVien? NhanVien { get; set; }

        [ForeignKey("FK_sMaQuyen")]
        public virtual Quyen? Quyen { get; set; }
    }
}