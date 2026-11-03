using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_ChamCong")]
    public class ChamCong
    {
        [Key]
        [Column("sMaChamCong")]
        public string SMaChamCong { get; set; } = null!;

        [Column("FK_sMaNV")]
        public string FK_sMaNV { get; set; } = null!;

        [Column("dNgayChamCong")]
        public DateTime DNgayChamCong { get; set; }

        [Column("dGioVao")]
        public DateTime? DGioVao { get; set; }

        [Column("dGioRa")]
        public DateTime? DGioRa { get; set; }

        [Column("iSoGioLam")]
        public int ISoGioLam { get; set; }

        [Column("sTrangThai")]
        public string? STrangThai { get; set; }

        [Column("sGhiChu")]
        public string? SGhiChu { get; set; }
    }
}