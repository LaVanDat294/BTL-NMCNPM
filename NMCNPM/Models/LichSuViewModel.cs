using System;
using System.Collections.Generic;

namespace NMCNPM.Models
{
    public class LichSuViewModel
    {
        // Ngày cần xem
        public DateTime NgayChamCong { get; set; }

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        public int SoNgayCong { get; set; }

        public int SoNgayMuon { get; set; }

        public int SoNgayVeSom { get; set; }

        public int SoNgayNghi { get; set; }

        public decimal TongSoGioLam { get; set; }

        // Lọc trạng thái
        public string TrangThai { get; set; }

        // Danh sách chấm công
        public List<ChamCong> DanhSachChamCong { get; set; }
            = new List<ChamCong>();
    }
}