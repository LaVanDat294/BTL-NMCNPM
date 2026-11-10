using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_PhongBan")]
    public class PhongBan
    {
        [Key, Column("sMaPB"), StringLength(20)]
        public string SMaPB { get; set; } = null!;

        [Required, Column("sTenPB"), StringLength(100)]
        public string STenPB { get; set; } = null!;

        [Column("sGhiChu"), StringLength(255)]
        public string? SGhiChu { get; set; }
    }
}