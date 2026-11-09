using System;

namespace NMCNPM.Models
{
    public class ChamCong
    {
        public string MaChamCong { get; set; }

        public string MaNV { get; set; }

        public string TenNV { get; set; }

        public DateTime NgayChamCong { get; set; }

        public DateTime? GioVao { get; set; }

        public DateTime? GioRa { get; set; }

        public int SoGioLam { get; set; }

        public string TrangThai { get; set; }

        public string TrangThaiVao { get; set; }

        public string TrangThaiRa { get; set; }

        public string GhiChu { get; set; }

        public string TenPhongBan { get; set; }
    }
}