using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_ChucVu")]
    public class ChucVu
    {
        [Key]
        [Column("sMaCV")]
        public string SMaCV { get; set; } = null!;

        [Column("sTenCV")]
        public string STenCV { get; set; } = null!;

        [Column("fPhuCap")]
        public decimal FPhuCap { get; set; }
    }
}