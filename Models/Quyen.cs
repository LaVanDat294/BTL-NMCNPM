using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMCNPM.Models
{
    [Table("tbl_Quyen")]
    public class Quyen
    {
        [Key, Column("sMaQuyen"), StringLength(20)]
        public string SMaQuyen { get; set; } = null!;

        [Required, Column("sTenQuyen"), StringLength(50)]
        public string STenQuyen { get; set; } = null!;
    }
}