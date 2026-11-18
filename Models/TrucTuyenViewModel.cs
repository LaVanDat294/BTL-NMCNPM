using System;
using System.Collections.Generic;

namespace NMCNPM.Models
{
    public class TrucTuyenViewModel
    {
        // Thông tin nhân viên
        public string? MaNV { get; set; }
        public string? TenNV { get; set; }

        // Thông tin phòng ban
        public string? TenPB { get; set; }

        // Thời gian hiện tại
        public DateTime NgayHienTai { get; set; }
        public DateTime GioHienTai { get; set; }

        // Trạng thái chấm công hôm nay
        public string? TrangThaiChamCong { get; set; }

        // Thời gian vào và ra
        public DateTime? GioVao { get; set; }
        public DateTime? GioRa { get; set; }

        // Tổng số giờ làm
        public int SoGioLam { get; set; }

        // Lịch sử chấm công 7 ngày gần nhất
        public List<ChamCong> LichSu7Ngay { get; set; } = new List<ChamCong>();
    }
}
