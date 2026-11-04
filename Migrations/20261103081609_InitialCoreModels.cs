using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NMCNPM.Migrations
{
    /// <inheritdoc />
    public partial class InitialCoreModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_ChamCong",
                columns: table => new
                {
                    sMaChamCong = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dNgayChamCong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dGioVao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dGioRa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    iSoGioLam = table.Column<int>(type: "int", nullable: false),
                    sTrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sGhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ChamCong", x => x.sMaChamCong);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ChucVu",
                columns: table => new
                {
                    sMaCV = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sTenCV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fPhuCap = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ChucVu", x => x.sMaCV);
                });

            migrationBuilder.CreateTable(
                name: "tbl_DonTu",
                columns: table => new
                {
                    sMaDon = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sLoaiDon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dNgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sLyDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sTrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sNguoiDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_DonTu", x => x.sMaDon);
                });

            migrationBuilder.CreateTable(
                name: "tbl_HopDongLaoDong",
                columns: table => new
                {
                    sMaHD = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sLoaiHD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dNgayKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dNgayHetHan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sTrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_HopDongLaoDong", x => x.sMaHD);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Luong",
                columns: table => new
                {
                    sMaLuong = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dKyLuong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    iSoGioLam = table.Column<int>(type: "int", nullable: false),
                    fLuongCoBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fThuong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fPhat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fThucLinh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    dNgayTinhLuong = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Luong", x => x.sMaLuong);
                });

            migrationBuilder.CreateTable(
                name: "tbl_NhanVien",
                columns: table => new
                {
                    sMaNV = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sTenNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dNgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sCCCD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sGioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sDiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sSDT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dNgayVaoLam = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sTrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_sMaCV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_sMaPB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fLuongcb = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_NhanVien", x => x.sMaNV);
                });

            migrationBuilder.CreateTable(
                name: "tbl_PhongBan",
                columns: table => new
                {
                    sMaPB = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sTenPB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sGhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PhongBan", x => x.sMaPB);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Quyen",
                columns: table => new
                {
                    sMaQuyen = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sTenQuyen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Quyen", x => x.sMaQuyen);
                });

            migrationBuilder.CreateTable(
                name: "tbl_TaiKhoan",
                columns: table => new
                {
                    sMaTK = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTenDangNhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sMatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_sMaQuyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bTrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_TaiKhoan", x => x.sMaTK);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_ChamCong");

            migrationBuilder.DropTable(
                name: "tbl_ChucVu");

            migrationBuilder.DropTable(
                name: "tbl_DonTu");

            migrationBuilder.DropTable(
                name: "tbl_HopDongLaoDong");

            migrationBuilder.DropTable(
                name: "tbl_Luong");

            migrationBuilder.DropTable(
                name: "tbl_NhanVien");

            migrationBuilder.DropTable(
                name: "tbl_PhongBan");

            migrationBuilder.DropTable(
                name: "tbl_Quyen");

            migrationBuilder.DropTable(
                name: "tbl_TaiKhoan");
        }
    }
}
