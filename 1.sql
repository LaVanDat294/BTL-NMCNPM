CREATE DATABASE db_HRM_Tocotoco;
USE db_HRM_Tocotoco;

CREATE TABLE tbl_PhongBan (
    sMaPB VARCHAR(20) NOT NULL,
    sTenPB NVARCHAR(100) NOT NULL,
    sGhiChu NVARCHAR(255) NULL,
    CONSTRAINT PK_tbl_PhongBan PRIMARY KEY (sMaPB)
);

-- 2. BẢNG CHỨC VỤ (tbl_ChucVu)
CREATE TABLE tbl_ChucVu (
    sMaCV VARCHAR(20) NOT NULL,
    sTenCV NVARCHAR(100) NOT NULL,
    fPhuCap DECIMAL(18,0) DEFAULT 0,
    CONSTRAINT PK_tbl_ChucVu PRIMARY KEY (sMaCV)
);

-- 3. BẢNG QUYỀN (tbl_Quyen - RBAC)
CREATE TABLE tbl_Quyen (
    sMaQuyen VARCHAR(20) NOT NULL,
    sTenQuyen NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_tbl_Quyen PRIMARY KEY (sMaQuyen)
);

-- 4. BẢNG NHÂN VIÊN (tbl_NhanVien)
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

-- 5. BẢNG TÀI KHOẢN (tbl_TaiKhoan)
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

-- 6. BẢNG CHẤM CÔNG (tbl_ChamCong)
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

-- INDEX TỐI ƯU TRUY VẤN CHẤM CÔNG THEO NGÀY & NHÂN VIÊN
CREATE NONCLUSTERED INDEX IX_tbl_ChamCong_NV_Ngay
ON tbl_ChamCong (FK_sMaNV, dNgayChamCong);

-- 7. BẢNG LƯƠNG (tbl_Luong)
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

-- INDEX TỐI ƯU TRUY VẤN TÍNH LƯƠNG KỲ
CREATE NONCLUSTERED INDEX IX_tbl_Luong_NV_Ky
ON tbl_Luong (FK_sMaNV, dKyLuong);

-- 8. BẢNG HỢP ĐỒNG LAO ĐỘNG (tbl_HopDongLaoDong)
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

-- 9. BẢNG ĐƠN TỪ (tbl_DonTu)
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

-- SEED DATA MẪU KHỞI TẠO HỆ THỐNG
INSERT INTO tbl_Quyen (sMaQuyen, sTenQuyen) VALUES 
('Q_ADMIN', N'Quản trị hệ thống (Admin)'),
('Q_MANAGER', N'Quản lý cửa hàng (Store Manager)'),
('Q_STAFF', N'Nhân viên thu ngân / Pha chế (Staff)');

INSERT INTO tbl_PhongBan (sMaPB, sTenPB, sGhiChu) VALUES 
('PB_HR', N'Phòng Nhân sự', N'Trụ sở chính'),
('PB_CH01', N'Cửa hàng Tocotoco Cầu Giấy', N'Chi nhánh 1'),
('PB_CH02', N'Cửa hàng Tocotoco Hoàn Kiếm', N'Chi nhánh 2');

INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES 
('CV_ADMIN', N'Chuyên viên CNTT / QTHT', 2000000),
('CV_QLCH', N'Cửa hàng trưởng', 1500000),
('CV_TN', N'Thu ngân POS', 500000),
('CV_PC', N'Nhân viên pha chế', 500000);

-- Tạo hồ sơ quản trị nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM tbl_NhanVien WHERE sMaNV = 'NV_ADMIN')
BEGIN
    INSERT INTO tbl_NhanVien
    (sMaNV, sTenNV, sCCCD, sGioiTinh, sEmail, sTrangThai, FK_sMaCV, FK_sMaPB, fLuongcb)
    VALUES
    ('NV_ADMIN', N'Quản trị viên', '012345678901', N'Nam',
     'admin@tocotoco.vn', N'Đang làm việc', 'CV_ADMIN', 'PB_HR', 0);
END
GO

-- Tài khoản mẫu: admin / Admin@123
IF NOT EXISTS (SELECT 1 FROM tbl_TaiKhoan WHERE sTenDangNhap = 'admin')
BEGIN
    INSERT INTO tbl_TaiKhoan
    (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
    VALUES
    ('TK_ADMIN', 'NV_ADMIN', 'admin',
     'PBKDF2$120000$ABEiM0RVZneImaq7zN3u/w==$pjHcpvaFOmL5VCnTT7YK/4R6TLJ21vl5l3A/vsPn2xk=', 'Q_ADMIN', 1);
END
GO

SELECT
    sMaTK,
    FK_sMaNV,
    sTenDangNhap,
    FK_sMaQuyen,
    bTrangThai,
    LEN(sMatKhauHash) AS DoDaiHash
FROM tbl_TaiKhoan
WHERE sTenDangNhap = 'admin';
