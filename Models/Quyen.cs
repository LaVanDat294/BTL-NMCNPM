using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_Quyen")]
    public class Quyen
    {
        [Key]
        [Column("sMaQuyen")]
        public string SMaQuyen { get; set; } = null!;

        [Column("sTenQuyen")]
        public string STenQuyen { get; set; } = null!;
    }
}