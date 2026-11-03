using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_HopDongLaoDong")]
    public class HopDongLaoDong
    {
        [Key]
        [Column("sMaHD")]
        public string SMaHD { get; set; } = null!;

        [Column("FK_sMaNV")]
        public string FK_sMaNV { get; set; } = null!;

        [Column("sLoaiHD")]
        public string SLoaiHD { get; set; } = null!;

        [Column("dNgayKy")]
        public DateTime DNgayKy { get; set; }

        [Column("dNgayHetHan")]
        public DateTime? DNgayHetHan { get; set; }

        [Column("sTrangThai")]
        public string? STrangThai { get; set; }
    }
}