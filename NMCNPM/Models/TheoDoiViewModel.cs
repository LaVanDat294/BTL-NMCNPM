using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NMCNPM.Models
{
    public class TheoDoiViewModel
    {
        // Tìm kiếm nhân viên
        public string TenNhanVien { get; set; }

        // Lọc chi nhánh
        public string MaPhongBan { get; set; }
        public string TenPhongBan { get; set; }

        // Danh sách chi nhánh để hiển thị Select
        public List<SelectListItem> DanhSachChiNhanh { get; set; }
            = new List<SelectListItem>();

        // Lọc khoảng thời gian
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }

        // Lọc giờ vào
        public TimeSpan? TuGioVao { get; set; }
        public TimeSpan? DenGioVao { get; set; }

        // Lọc giờ ra
        public TimeSpan? TuGioRa { get; set; }
        public TimeSpan? DenGioRa { get; set; }

        // Lọc trạng thái
        public string TrangThai { get; set; }

        // Danh sách kết quả
        public List<ChamCong> DanhSachChamCong { get; set; }
            = new List<ChamCong>();
    }
}