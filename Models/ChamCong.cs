using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_ChamCong")]
    public class ChamCong
    {
        [Key]
        [Column("sMaChamCong")]
        [StringLength(20)]
        public string MaChamCong { get; set; }

        [Column("FK_sMaNV")]
        public string FK_sMaNV { get; set; }

        // Tên nhân viên lấy từ bảng tbl_NhanVien khi JOIN
        [NotMapped]
        public string TenNV { get; set; }

        [Column("dNgayChamCong")]
        public DateTime NgayChamCong { get; set; }

        // Tương thích với LuongController
        [NotMapped]
        public DateTime DNgayChamCong
        {
            get { return NgayChamCong; }
            set { NgayChamCong = value; }
        }

        [Column("dGioVao")]
        public DateTime? GioVao { get; set; }

        [Column("dGioRa")]
        public DateTime? GioRa { get; set; }

        [Column("iSoGioLam")]
        public int SoGioLam { get; set; }

        // Tương thích với LuongController
        [NotMapped]
        public int ISoGioLam
        {
            get { return SoGioLam; }
            set { SoGioLam = value; }
        }

        [Column("sTrangThai")]
        public string TrangThai { get; set; }

        [Column("sGhiChu")]
        public string GhiChu { get; set; }
    }
}