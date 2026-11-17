using System;
using System.Collections.Generic;

namespace NMCNPM.Models
{
    public class LichSuViewModel
    {
        // Ngày cần xem
        public DateTime NgayChamCong { get; set; }

        // Khoảng thời gian lọc
        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        // Thống kê chấm công
        public int SoNgayCong { get; set; }

        public int SoNgayMuon { get; set; }

        public int SoNgayVeSom { get; set; }

        public int SoNgayNghi { get; set; }

        // Tổng số giờ làm
        public decimal TongSoGioLam { get; set; }

        // Lọc trạng thái
        public string TrangThai { get; set; }

        // Danh sách lịch sử chấm công
        public List<ChamCong> DanhSachChamCong { get; set; }
            = new List<ChamCong>();
    }
}