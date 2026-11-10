using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_Luong")]
    public class Luong
    {
        [Key, Column("sMaLuong"), StringLength(20)]
        public string SMaLuong { get; set; } = null!;

        [Required, Column("FK_sMaNV"), StringLength(20)]
        public string FK_sMaNV { get; set; } = null!;

        [Required, Column("dKyLuong")]
        public DateTime DKyLuong { get; set; }

        [Column("iSoGioLam")]
        public int ISoGioLam { get; set; }

        [Column("fLuongCoBan", TypeName = "decimal(18,0)")]
        public decimal FLuongCoBan { get; set; }

        [Column("fThuong", TypeName = "decimal(18,0)")]
        public decimal FThuong { get; set; }

        [Column("fPhat", TypeName = "decimal(18,0)")]
        public decimal FPhat { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("fThucLinh", TypeName = "decimal(18,0)")]
        public decimal FThucLinh { get; set; }

        [Column("dNgayTinhLuong")]
        public DateTime? DNgayTinhLuong { get; set; }

        [ForeignKey("FK_sMaNV")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}