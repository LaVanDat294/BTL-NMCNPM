using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_HopDongLaoDong")]
    public class HopDongLaoDong
    {
        [Key, Column("sMaHD"), StringLength(20)]
        public string SMaHD { get; set; } = null!;

        [Required, Column("FK_sMaNV"), StringLength(20)]
        public string FK_sMaNV { get; set; } = null!;

        [Required, Column("sLoaiHD"), StringLength(50)]
        public string SLoaiHD { get; set; } = null!;

        [Required, Column("dNgayKy")]
        public DateTime DNgayKy { get; set; }

        [Column("dNgayHetHan")]
        public DateTime? DNgayHetHan { get; set; }

        [Column("sTrangThai"), StringLength(50)]
        public string? STrangThai { get; set; }

        [ForeignKey("FK_sMaNV")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}