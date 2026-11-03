using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_DonTu")]
    public class DonTu
    {
        [Key]
        [Column("sMaDon")]
        public string SMaDon { get; set; } = null!;

        [Column("FK_sMaNV")]
        public string FK_sMaNV { get; set; } = null!;

        [Column("sLoaiDon")]
        public string SLoaiDon { get; set; } = null!;

        [Column("dNgayGui")]
        public DateTime DNgayGui { get; set; }

        [Column("sLyDo")]
        public string? SLyDo { get; set; }

        [Column("sTrangThai")]
        public string? STrangThai { get; set; }

        [Column("sNguoiDuyet")]
        public string? SNguoiDuyet { get; set; }
    }
}