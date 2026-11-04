using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_Luong")]
    public class Luong
    {
        [Key]
        [Column("sMaLuong")]
        public string SMaLuong { get; set; } = null!;

        [Column("FK_sMaNV")]
        public string FK_sMaNV { get; set; } = null!;

        [Column("dKyLuong")]
        public DateTime DKyLuong { get; set; }

        [Column("iSoGioLam")]
        public int ISoGioLam { get; set; }

        [Column("fLuongCoBan")]
        public decimal FLuongCoBan { get; set; }

        [Column("fThuong")]
        public decimal FThuong { get; set; }

        [Column("fPhat")]
        public decimal FPhat { get; set; }

        [Column("fThucLinh")]
        public decimal FThucLinh { get; set; }

        [Column("dNgayTinhLuong")]
        public DateTime DNgayTinhLuong { get; set; }
    }
}