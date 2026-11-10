using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_DonTu")]
    public class DonTu
    {
        [Key, Column("sMaDon"), StringLength(20)]
        public string SMaDon { get; set; } = null!;

        [Required, Column("FK_sMaNV"), StringLength(20)]
        public string FK_sMaNV { get; set; } = null!;

        [Required, Column("sLoaiDon"), StringLength(50)]
        public string SLoaiDon { get; set; } = null!;

        [Column("dNgayGui")]
        public DateTime? DNgayGui { get; set; }

        [Column("sLyDo"), StringLength(500)]
        public string? SLyDo { get; set; }

        [Column("sTrangThai"), StringLength(50)]
        public string? STrangThai { get; set; }

        [Column("sNguoiDuyet"), StringLength(20)]
        public string? SNguoiDuyet { get; set; }

        [ForeignKey("FK_sMaNV")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}