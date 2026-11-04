using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_PhongBan")]
    public class PhongBan
    {
        [Key]
        [Column("sMaPB")]
        public string SMaPB { get; set; } = null!;

        [Column("sTenPB")]
        public string STenPB { get; set; } = null!;

        [Column("sGhiChu")]
        public string? SGhiChu { get; set; }
    }
}