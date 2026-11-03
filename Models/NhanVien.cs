using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_NhanVien")]
    public class NhanVien
    {
        [Key]
        [Column("sMaNV")]
        public string SMaNV { get; set; } = null!;

        [Column("sTenNV")]
        public string STenNV { get; set; } = null!;

        [Column("dNgaySinh")]
        public DateTime? DNgaySinh { get; set; }

        [Column("sCCCD")]
        public string SCCCD { get; set; } = null!;

        [Column("sGioiTinh")]
        public string? SGioiTinh { get; set; }

        [Column("sDiaChi")]
        public string? SDiaChi { get; set; }

        [Column("sSDT")]
        public string? SSDT { get; set; }

        [Column("sEmail")]
        public string? SEmail { get; set; }

        [Column("dNgayVaoLam")]
        public DateTime? DNgayVaoLam { get; set; }

        [Column("sTrangThai")]
        public string? STrangThai { get; set; }

        [Column("FK_sMaCV")]
        public string FK_sMaCV { get; set; } = null!;

        [Column("FK_sMaPB")]
        public string FK_sMaPB { get; set; } = null!;

        [Column("fLuongcb")]
        public decimal FLuongcb { get; set; }
    }
}