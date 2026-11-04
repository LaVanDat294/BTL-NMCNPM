using Microsoft.EntityFrameworkCore;

namespace NMCNPM.Models
{
    public class HrmDbContext : DbContext
    {
        public HrmDbContext(DbContextOptions<HrmDbContext> options) : base(options) { }

        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<ChucVu> ChucVus { get; set; }
        public DbSet<ChamCong> ChamCongs { get; set; }
        public DbSet<Luong> Luongs { get; set; }
        public DbSet<HopDongLaoDong> HopDongs { get; set; }
        public DbSet<DonTu> DonTus { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<Quyen> Quyens { get; set; }
    }
}