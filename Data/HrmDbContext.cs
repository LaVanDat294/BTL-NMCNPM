using Microsoft.EntityFrameworkCore;
using TocoTocoHRM.Models;

namespace TocoTocoHRM.Data;

public class HrmDbContext : DbContext
{
    public HrmDbContext(DbContextOptions<HrmDbContext> options) : base(options) { }

    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<PhongBan> PhongBans => Set<PhongBan>();
    public DbSet<ChucVu> ChucVus => Set<ChucVu>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NhanVien>(e =>
        {
            e.ToTable("tbl_NhanVien");
            e.HasKey(x => x.MaNV);
            e.Property(x => x.MaNV).HasColumnName("sMaNV").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.TenNV).HasColumnName("sTenNV").HasMaxLength(100);
            e.Property(x => x.NgaySinh).HasColumnName("dNgaySinh");
            e.Property(x => x.CCCD).HasColumnName("sCCCD").HasMaxLength(12).IsUnicode(false);
            e.Property(x => x.GioiTinh).HasColumnName("sGioiTinh").HasMaxLength(10);
            e.Property(x => x.DiaChi).HasColumnName("sDiaChi").HasMaxLength(255);
            e.Property(x => x.SDT).HasColumnName("sSDT").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.Email).HasColumnName("sEmail").HasMaxLength(100).IsUnicode(false);
            e.Property(x => x.NgayVaoLam).HasColumnName("dNgayVaoLam");
            e.Property(x => x.TrangThai).HasColumnName("sTrangThai").HasMaxLength(50);
            e.Property(x => x.MaCV).HasColumnName("FK_sMaCV").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.MaPB).HasColumnName("FK_sMaPB").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.LuongCB).HasColumnName("fLuongcb").HasColumnType("decimal(18,0)");
        });
        modelBuilder.Entity<PhongBan>(e =>
        {
            e.ToTable("tbl_PhongBan"); e.HasKey(x => x.MaPB);
            e.Property(x => x.MaPB).HasColumnName("sMaPB").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.TenPB).HasColumnName("sTenPB").HasMaxLength(100);
        });
        modelBuilder.Entity<ChucVu>(e =>
        {
            e.ToTable("tbl_ChucVu"); e.HasKey(x => x.MaCV);
            e.Property(x => x.MaCV).HasColumnName("sMaCV").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.TenCV).HasColumnName("sTenCV").HasMaxLength(100);
        });
        modelBuilder.Entity<TaiKhoan>(e =>
        {
            e.ToTable("tbl_TaiKhoan"); e.HasKey(x => x.MaTK);
            e.Property(x => x.MaTK).HasColumnName("sMaTK").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.MaNV).HasColumnName("FK_sMaNV").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.TenDangNhap).HasColumnName("sTenDangNhap").HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.MatKhauHash).HasColumnName("sMatKhauHash").HasMaxLength(255).IsUnicode(false);
            e.Property(x => x.MaQuyen).HasColumnName("FK_sMaQuyen").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.TrangThai).HasColumnName("bTrangThai");
        });
    }
}
