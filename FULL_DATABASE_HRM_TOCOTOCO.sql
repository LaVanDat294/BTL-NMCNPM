-- ====================================================================
-- HỆ THỐNG QUẢN LÝ NHÂN SỰ TOCOTOCO (db_HRM_Tocotoco)
-- SCRIPT TẠO TOÀN BỘ CƠ SỞ DỮ LIỆU & DỮ LIỆU MẪU ĐẦY ĐỦ
-- MẬT KHẨU MẶC ĐỊNH CHO TẤT CẢ TÀI KHOẢN: 123456
-- ====================================================================

-- 1. TẠO CƠ SỞ DỮ LIỆU NẾU CHƯA CÓ
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'db_HRM_Tocotoco')
BEGIN
    CREATE DATABASE db_HRM_Tocotoco;
END
GO

USE db_HRM_Tocotoco;
GO

-- 2. TẠO BẢNG PHÒNG BAN (tbl_PhongBan)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_PhongBan')
BEGIN
    CREATE TABLE tbl_PhongBan (
        sMaPB VARCHAR(20) NOT NULL,
        sTenPB NVARCHAR(100) NOT NULL,
        sGhiChu NVARCHAR(255) NULL,
        CONSTRAINT PK_tbl_PhongBan PRIMARY KEY (sMaPB)
    );
END
GO

-- 3. TẠO BẢNG CHỨC VỤ (tbl_ChucVu)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_ChucVu')
BEGIN
    CREATE TABLE tbl_ChucVu (
        sMaCV VARCHAR(20) NOT NULL,
        sTenCV NVARCHAR(100) NOT NULL,
        fPhuCap DECIMAL(18,0) DEFAULT 0,
        CONSTRAINT PK_tbl_ChucVu PRIMARY KEY (sMaCV)
    );
END
GO

-- 4. TẠO BẢNG PHÂN QUYỀN (tbl_Quyen - RBAC)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Quyen')
BEGIN
    CREATE TABLE tbl_Quyen (
        sMaQuyen VARCHAR(20) NOT NULL,
        sTenQuyen NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_tbl_Quyen PRIMARY KEY (sMaQuyen)
    );
END
GO

-- 5. TẠO BẢNG NHÂN VIÊN (tbl_NhanVien)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_NhanVien')
BEGIN
    CREATE TABLE tbl_NhanVien (
        sMaNV VARCHAR(20) NOT NULL,
        sTenNV NVARCHAR(100) NOT NULL,
        dNgaySinh DATETIME NULL,
        sCCCD VARCHAR(12) NOT NULL,
        sGioiTinh NVARCHAR(10) NULL,
        sDiaChi NVARCHAR(255) NULL,
        sSDT VARCHAR(15) NULL,
        sEmail VARCHAR(100) NULL,
        dNgayVaoLam DATETIME DEFAULT GETDATE(),
        sTrangThai NVARCHAR(50) DEFAULT N'Đang làm việc',
        FK_sMaCV VARCHAR(20) NOT NULL,
        FK_sMaPB VARCHAR(20) NOT NULL,
        fLuongcb DECIMAL(18,0) DEFAULT 0,
        CONSTRAINT PK_tbl_NhanVien PRIMARY KEY (sMaNV),
        CONSTRAINT UQ_tbl_NhanVien_CCCD UNIQUE (sCCCD),
        CONSTRAINT FK_NhanVien_ChucVu FOREIGN KEY (FK_sMaCV) REFERENCES tbl_ChucVu(sMaCV) ON DELETE NO ACTION,
        CONSTRAINT FK_NhanVien_PhongBan FOREIGN KEY (FK_sMaPB) REFERENCES tbl_PhongBan(sMaPB) ON DELETE NO ACTION
    );
END
GO

-- 6. TẠO BẢNG TÀI KHOẢN (tbl_TaiKhoan)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_TaiKhoan')
BEGIN
    CREATE TABLE tbl_TaiKhoan (
        sMaTK VARCHAR(20) NOT NULL,
        FK_sMaNV VARCHAR(20) NOT NULL,
        sTenDangNhap VARCHAR(50) NOT NULL,
        sMatKhauHash VARCHAR(255) NOT NULL,
        FK_sMaQuyen VARCHAR(20) NOT NULL,
        bTrangThai BIT DEFAULT 1,
        CONSTRAINT PK_tbl_TaiKhoan PRIMARY KEY (sMaTK),
        CONSTRAINT UQ_tbl_TaiKhoan_NV UNIQUE (FK_sMaNV),
        CONSTRAINT UQ_tbl_TaiKhoan_TenDN UNIQUE (sTenDangNhap),
        CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (FK_sMaNV) REFERENCES tbl_NhanVien(sMaNV) ON DELETE NO ACTION,
        CONSTRAINT FK_TaiKhoan_Quyen FOREIGN KEY (FK_sMaQuyen) REFERENCES tbl_Quyen(sMaQuyen) ON DELETE NO ACTION
    );
END
GO

-- 7. TẠO BẢNG CHẤM CÔNG (tbl_ChamCong)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_ChamCong')
BEGIN
    CREATE TABLE tbl_ChamCong (
        sMaChamCong VARCHAR(20) NOT NULL,
        FK_sMaNV VARCHAR(20) NOT NULL,
        dNgayChamCong DATE NOT NULL,
        dGioVao DATETIME NULL,
        dGioRa DATETIME NULL,
        iSoGioLam INT DEFAULT 0,
        sTrangThai NVARCHAR(50) NULL, 
        sGhiChu NVARCHAR(255) NULL,
        CONSTRAINT PK_tbl_ChamCong PRIMARY KEY (sMaChamCong),
        CONSTRAINT UQ_tbl_ChamCong_NV_Ngay UNIQUE (FK_sMaNV, dNgayChamCong),
        CONSTRAINT FK_ChamCong_NhanVien FOREIGN KEY (FK_sMaNV) REFERENCES tbl_NhanVien(sMaNV) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX IX_tbl_ChamCong_NV_Ngay
    ON tbl_ChamCong (FK_sMaNV, dNgayChamCong);
END
GO

-- 8. TẠO BẢNG LƯƠNG (tbl_Luong)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_Luong')
BEGIN
    CREATE TABLE tbl_Luong (
        sMaLuong VARCHAR(20) NOT NULL,
        FK_sMaNV VARCHAR(20) NOT NULL,
        dKyLuong DATE NOT NULL, 
        iSoGioLam INT DEFAULT 0,
        fLuongCoBan DECIMAL(18,0) DEFAULT 0,
        fThuong DECIMAL(18,0) DEFAULT 0,
        fPhat DECIMAL(18,0) DEFAULT 0,
        fThucLinh AS (fLuongCoBan + fThuong - fPhat) PERSISTED,
        dNgayTinhLuong DATETIME DEFAULT GETDATE(),
        CONSTRAINT PK_tbl_Luong PRIMARY KEY (sMaLuong),
        CONSTRAINT UQ_tbl_Luong_NV_Ky UNIQUE (FK_sMaNV, dKyLuong),
        CONSTRAINT FK_Luong_NhanVien FOREIGN KEY (FK_sMaNV) REFERENCES tbl_NhanVien(sMaNV) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX IX_tbl_Luong_NV_Ky
    ON tbl_Luong (FK_sMaNV, dKyLuong);
END
GO

-- 9. TẠO BẢNG HỢP ĐỒNG LAO ĐỘNG (tbl_HopDongLaoDong)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_HopDongLaoDong')
BEGIN
    CREATE TABLE tbl_HopDongLaoDong (
        sMaHD VARCHAR(20) NOT NULL,
        FK_sMaNV VARCHAR(20) NOT NULL,
        sLoaiHD NVARCHAR(50) NOT NULL, 
        dNgayKy DATE NOT NULL,
        dNgayHetHan DATE NULL,
        sTrangThai NVARCHAR(50) DEFAULT N'Hiệu lực',
        CONSTRAINT PK_tbl_HopDongLaoDong PRIMARY KEY (sMaHD),
        CONSTRAINT FK_HopDong_NhanVien FOREIGN KEY (FK_sMaNV) REFERENCES tbl_NhanVien(sMaNV) ON DELETE NO ACTION
    );
END
GO

-- 10. TẠO BẢNG ĐƠN TỪ (tbl_DonTu)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_DonTu')
BEGIN
    CREATE TABLE tbl_DonTu (
        sMaDon VARCHAR(20) NOT NULL,
        FK_sMaNV VARCHAR(20) NOT NULL,
        sLoaiDon NVARCHAR(50) NOT NULL, 
        dNgayGui DATETIME DEFAULT GETDATE(),
        sLyDo NVARCHAR(500) NULL,
        sTrangThai NVARCHAR(50) DEFAULT N'Chờ duyệt',
        sNguoiDuyet VARCHAR(20) NULL,
        CONSTRAINT PK_tbl_DonTu PRIMARY KEY (sMaDon),
        CONSTRAINT FK_DonTu_NhanVien FOREIGN KEY (FK_sMaNV) REFERENCES tbl_NhanVien(sMaNV) ON DELETE NO ACTION
    );
END
GO

-- ====================================================================
-- THÊM DỮ LIỆU KHỞI TẠO HỆ THỐNG
-- ====================================================================

-- PHÂN QUYỀN
IF NOT EXISTS (SELECT 1 FROM tbl_Quyen WHERE sMaQuyen = 'Q_ADMIN')
    INSERT INTO tbl_Quyen (sMaQuyen, sTenQuyen) VALUES ('Q_ADMIN', N'Quản trị hệ thống (Admin)');

IF NOT EXISTS (SELECT 1 FROM tbl_Quyen WHERE sMaQuyen = 'Q_MANAGER')
    INSERT INTO tbl_Quyen (sMaQuyen, sTenQuyen) VALUES ('Q_MANAGER', N'Quản lý cửa hàng (Store Manager)');

IF NOT EXISTS (SELECT 1 FROM tbl_Quyen WHERE sMaQuyen = 'Q_STAFF')
    INSERT INTO tbl_Quyen (sMaQuyen, sTenQuyen) VALUES ('Q_STAFF', N'Nhân viên thu ngân / Pha chế (Staff)');
GO

-- PHÒNG BAN
IF NOT EXISTS (SELECT 1 FROM tbl_PhongBan WHERE sMaPB = 'PB_HR')
    INSERT INTO tbl_PhongBan (sMaPB, sTenPB, sGhiChu) VALUES ('PB_HR', N'Phòng Nhân sự', N'Trụ sở chính');

IF NOT EXISTS (SELECT 1 FROM tbl_PhongBan WHERE sMaPB = 'PB_CH01')
    INSERT INTO tbl_PhongBan (sMaPB, sTenPB, sGhiChu) VALUES ('PB_CH01', N'Cửa hàng Tocotoco Cầu Giấy', N'Chi nhánh 1');

IF NOT EXISTS (SELECT 1 FROM tbl_PhongBan WHERE sMaPB = 'PB_CH02')
    INSERT INTO tbl_PhongBan (sMaPB, sTenPB, sGhiChu) VALUES ('PB_CH02', N'Cửa hàng Tocotoco Hoàn Kiếm', N'Chi nhánh 2');
GO

-- CHỨC VỤ
IF NOT EXISTS (SELECT 1 FROM tbl_ChucVu WHERE sMaCV = 'CV_ADMIN')
    INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES ('CV_ADMIN', N'Chuyên viên CNTT / QTHT', 2000000);

IF NOT EXISTS (SELECT 1 FROM tbl_ChucVu WHERE sMaCV = 'CV_QLCH')
    INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES ('CV_QLCH', N'Cửa hàng trưởng', 1500000);

IF NOT EXISTS (SELECT 1 FROM tbl_ChucVu WHERE sMaCV = 'CV_TN')
    INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES ('CV_TN', N'Thu ngân POS', 500000);

IF NOT EXISTS (SELECT 1 FROM tbl_ChucVu WHERE sMaCV = 'CV_PC')
    INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES ('CV_PC', N'Nhân viên pha chế', 500000);
GO

-- NHÂN VIÊN
IF NOT EXISTS (SELECT 1 FROM tbl_NhanVien WHERE sMaNV = 'NV_ADMIN')
    INSERT INTO tbl_NhanVien (sMaNV, sTenNV, dNgaySinh, sCCCD, sGioiTinh, sDiaChi, sSDT, sEmail, FK_sMaCV, FK_sMaPB, fLuongcb)
    VALUES ('NV_ADMIN', N'Quản trị viên', '2000-01-01', '012345678901', N'Nam', N'Hà Nội', '0900000000', 'admin@tocotoco.vn', 'CV_ADMIN', 'PB_HR', 15000000);

IF NOT EXISTS (SELECT 1 FROM tbl_NhanVien WHERE sMaNV = 'NV_QL01')
    INSERT INTO tbl_NhanVien (sMaNV, sTenNV, dNgaySinh, sCCCD, sGioiTinh, sDiaChi, sSDT, sEmail, FK_sMaCV, FK_sMaPB, fLuongcb)
    VALUES ('NV_QL01', N'Trần Quản Lý', '1998-05-15', '012345678902', N'Nữ', N'Hà Nội', '0911111111', 'quanly@tocotoco.vn', 'CV_QLCH', 'PB_CH01', 10000000);

IF NOT EXISTS (SELECT 1 FROM tbl_NhanVien WHERE sMaNV = 'NV_NV01')
    INSERT INTO tbl_NhanVien (sMaNV, sTenNV, dNgaySinh, sCCCD, sGioiTinh, sDiaChi, sSDT, sEmail, FK_sMaCV, FK_sMaPB, fLuongcb)
    VALUES ('NV_NV01', N'Nguyễn Thu Ngân', '2003-08-20', '012345678903', N'Nữ', N'Hà Nội', '0922222222', 'thungan@tocotoco.vn', 'CV_TN', 'PB_CH01', 6000000);
GO

-- TÀI KHOẢN (MẬT KHẨU BĂM PBKDF2 CỦA '123456')
-- Admin
IF NOT EXISTS (SELECT 1 FROM tbl_TaiKhoan WHERE sTenDangNhap = 'admin')
BEGIN
    INSERT INTO tbl_TaiKhoan (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
    VALUES ('TK_ADMIN', 'NV_ADMIN', 'admin', 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=', 'Q_ADMIN', 1);
END
ELSE
BEGIN
    UPDATE tbl_TaiKhoan
    SET sMatKhauHash = 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=', bTrangThai = 1
    WHERE sTenDangNhap = 'admin';
END
GO

-- Quản lý (Manager)
IF NOT EXISTS (SELECT 1 FROM tbl_TaiKhoan WHERE sTenDangNhap = 'quanly')
BEGIN
    INSERT INTO tbl_TaiKhoan (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
    VALUES ('TK_QL01', 'NV_QL01', 'quanly', 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=', 'Q_MANAGER', 1);
END
ELSE
BEGIN
    UPDATE tbl_TaiKhoan
    SET sMatKhauHash = 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=', bTrangThai = 1
    WHERE sTenDangNhap = 'quanly';
END
GO

-- Nhân viên (Staff)
IF NOT EXISTS (SELECT 1 FROM tbl_TaiKhoan WHERE sTenDangNhap = 'nhanvien')
BEGIN
    INSERT INTO tbl_TaiKhoan (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
    VALUES ('TK_NV01', 'NV_NV01', 'nhanvien', 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=', 'Q_STAFF', 1);
END
ELSE
BEGIN
    UPDATE tbl_TaiKhoan
    SET sMatKhauHash = 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=', bTrangThai = 1
    WHERE sTenDangNhap = 'nhanvien';
END
GO

-- KIỂM TRA KẾT QUẢ
SELECT 
    tk.sMaTK AS [Mã TK],
    tk.sTenDangNhap AS [Tên Đăng Nhập],
    nv.sTenNV AS [Họ Tên Nhân Viên],
    q.sTenQuyen AS [Vai Trò],
    tk.FK_sMaQuyen AS [Mã Quyền],
    CASE WHEN tk.bTrangThai = 1 THEN N'Hoạt động' ELSE N'Bị khóa' END AS [Trạng Thái]
FROM tbl_TaiKhoan tk
INNER JOIN tbl_NhanVien nv ON tk.FK_sMaNV = nv.sMaNV
INNER JOIN tbl_Quyen q ON tk.FK_sMaQuyen = q.sMaQuyen;
GO
