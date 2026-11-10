using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_ChucVu")]
    public class ChucVu
    {
        [Key, Column("sMaCV"), StringLength(20)]
        public string SMaCV { get; set; } = null!;

        [Required, Column("sTenCV"), StringLength(100)]
        public string STenCV { get; set; } = null!;

        [Column("fPhuCap", TypeName = "decimal(18,0)")]
        public decimal FPhuCap { get; set; }
    }
}