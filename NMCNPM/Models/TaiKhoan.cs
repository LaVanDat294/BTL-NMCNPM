using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NMCNPM.Models
{
    public class TaiKhoan
    {
        public string MaTK { get; set; }

        public string MaNV { get; set; }

        public string TenDangNhap { get; set; }

        public string MatKhauHash { get; set; }

        public string MaQuyen { get; set; }

        public string TenQuyen { get; set; }

        public string TenNV { get; set; }

        public bool TrangThai { get; set; }
    }
}