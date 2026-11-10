using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_ChamCong")]
    public class ChamCong
    {
        [Key, Column("sMaChamCong"), StringLength(20)]
        public string SMaChamCong { get; set; } = null!;

        [Required, Column("FK_sMaNV"), StringLength(20)]
        public string FK_sMaNV { get; set; } = null!;

        [Required, Column("dNgayChamCong")]
        public DateTime DNgayChamCong { get; set; }

        [Column("dGioVao")]
        public DateTime? DGioVao { get; set; }

        [Column("dGioRa")]
        public DateTime? DGioRa { get; set; }

        [Column("iSoGioLam")]
        public int ISoGioLam { get; set; }

        [Column("sTrangThai"), StringLength(50)]
        public string? STrangThai { get; set; }

        [Column("sGhiChu"), StringLength(255)]
        public string? SGhiChu { get; set; }

        [ForeignKey("FK_sMaNV")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}