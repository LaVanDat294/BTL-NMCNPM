using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("sMaTK")]
        public string SMaTK { get; set; } = null!;

        [Column("FK_sMaNV")]
        public string FK_sMaNV { get; set; } = null!;

        [Column("sTenDangNhap")]
        public string STenDangNhap { get; set; } = null!;

        [Column("sMatKhauHash")]
        public string SMatKhauHash { get; set; } = null!;

        [Column("FK_sMaQuyen")]
        public string FK_sMaQuyen { get; set; } = null!;

        [Column("bTrangThai")]
        public bool BTrangThai { get; set; }
    }
}