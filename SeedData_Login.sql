-- ====================================================================
-- SCRIPT TẠO DỮ LIỆU TÀI KHOẢN VÀ NHÂN VIÊN PHỤC VỤ ĐĂNG NHẬP HỆ THỐNG
-- CSDL: db_HRM_Tocotoco
-- MẬT KHẨU MẶC ĐỊNH CHO TẤT CẢ CÁC TÀI KHOẢN: 123456
-- ====================================================================

USE db_HRM_Tocotoco;
GO

-- 1. ĐẢM BẢO DỮ LIỆU BẢNG tbl_Quyen ĐÃ CÓ ĐỦ 3 QUYỀN
IF NOT EXISTS (SELECT 1 FROM tbl_Quyen WHERE sMaQuyen = 'Q_ADMIN')
    INSERT INTO tbl_Quyen (sMaQuyen, sTenQuyen) VALUES ('Q_ADMIN', N'Quản trị hệ thống (Admin)');

IF NOT EXISTS (SELECT 1 FROM tbl_Quyen WHERE sMaQuyen = 'Q_MANAGER')
    INSERT INTO tbl_Quyen (sMaQuyen, sTenQuyen) VALUES ('Q_MANAGER', N'Quản lý cửa hàng (Store Manager)');

IF NOT EXISTS (SELECT 1 FROM tbl_Quyen WHERE sMaQuyen = 'Q_STAFF')
    INSERT INTO tbl_Quyen (sMaQuyen, sTenQuyen) VALUES ('Q_STAFF', N'Nhân viên thu ngân / Pha chế (Staff)');
GO

-- 2. ĐẢM BẢO PHÒNG BAN VÀ CHỨC VỤ
IF NOT EXISTS (SELECT 1 FROM tbl_PhongBan WHERE sMaPB = 'PB_HR')
    INSERT INTO tbl_PhongBan (sMaPB, sTenPB, sGhiChu) VALUES ('PB_HR', N'Phòng Nhân sự', N'Trụ sở chính');

IF NOT EXISTS (SELECT 1 FROM tbl_PhongBan WHERE sMaPB = 'PB_CH01')
    INSERT INTO tbl_PhongBan (sMaPB, sTenPB, sGhiChu) VALUES ('PB_CH01', N'Cửa hàng Tocotoco Cầu Giấy', N'Chi nhánh 1');

IF NOT EXISTS (SELECT 1 FROM tbl_ChucVu WHERE sMaCV = 'CV_ADMIN')
    INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES ('CV_ADMIN', N'Chuyên viên CNTT / QTHT', 2000000);

IF NOT EXISTS (SELECT 1 FROM tbl_ChucVu WHERE sMaCV = 'CV_QLCH')
    INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES ('CV_QLCH', N'Cửa hàng trưởng', 1500000);

IF NOT EXISTS (SELECT 1 FROM tbl_ChucVu WHERE sMaCV = 'CV_TN')
    INSERT INTO tbl_ChucVu (sMaCV, sTenCV, fPhuCap) VALUES ('CV_TN', N'Thu ngân POS', 500000);
GO

-- 3. THÊM CÁC NHÂN VIÊN MẪU (NẾU CHƯA CÓ)
-- Nhân viên Quản trị hệ thống (Admin)
IF NOT EXISTS (SELECT 1 FROM tbl_NhanVien WHERE sMaNV = 'NV_ADMIN')
    INSERT INTO tbl_NhanVien (sMaNV, sTenNV, dNgaySinh, sCCCD, sGioiTinh, sDiaChi, sSDT, sEmail, FK_sMaCV, FK_sMaPB, fLuongcb)
    VALUES ('NV_ADMIN', N'Quản trị viên', '2000-01-01', '012345678901', N'Nam', N'Hà Nội', '0900000000', 'admin@tocotoco.vn', 'CV_ADMIN', 'PB_HR', 15000000);

-- Nhân viên Quản lý cửa hàng (Manager)
IF NOT EXISTS (SELECT 1 FROM tbl_NhanVien WHERE sMaNV = 'NV_QL01')
    INSERT INTO tbl_NhanVien (sMaNV, sTenNV, dNgaySinh, sCCCD, sGioiTinh, sDiaChi, sSDT, sEmail, FK_sMaCV, FK_sMaPB, fLuongcb)
    VALUES ('NV_QL01', N'Trần Quản Lý', '1998-05-15', '012345678902', N'Nữ', N'Hà Nội', '0911111111', 'quanly@tocotoco.vn', 'CV_QLCH', 'PB_CH01', 10000000);

-- Nhân viên Thu ngân / Phục vụ (Staff)
IF NOT EXISTS (SELECT 1 FROM tbl_NhanVien WHERE sMaNV = 'NV_NV01')
    INSERT INTO tbl_NhanVien (sMaNV, sTenNV, dNgaySinh, sCCCD, sGioiTinh, sDiaChi, sSDT, sEmail, FK_sMaCV, FK_sMaPB, fLuongcb)
    VALUES ('NV_NV01', N'Nguyễn Thu Ngân', '2003-08-20', '012345678903', N'Nữ', N'Hà Nội', '0922222222', 'thungan@tocotoco.vn', 'CV_TN', 'PB_CH01', 6000000);
GO

-- 4. THÊM TÀI KHOẢN CHO 3 VAI TRÒ (MẬT KHẨU BĂM PBKDF2 CHUẨN CỦA PROJECT CHO '123456')
-- Mã hash tương ứng mật khẩu "123456":
-- PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=

-- 4.1. Tài khoản Admin (Q_ADMIN)
IF NOT EXISTS (SELECT 1 FROM tbl_TaiKhoan WHERE sTenDangNhap = 'admin')
BEGIN
    INSERT INTO tbl_TaiKhoan (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
    VALUES (
        'TK_ADMIN',
        'NV_ADMIN',
        'admin',
        'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=',
        'Q_ADMIN',
        1
    );
END
ELSE
BEGIN
    UPDATE tbl_TaiKhoan
    SET sMatKhauHash = 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=',
        bTrangThai = 1
    WHERE sTenDangNhap = 'admin';
END
GO

-- 4.2. Tài khoản Quản lý cửa hàng (Q_MANAGER)
IF NOT EXISTS (SELECT 1 FROM tbl_TaiKhoan WHERE sTenDangNhap = 'quanly')
BEGIN
    INSERT INTO tbl_TaiKhoan (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
    VALUES (
        'TK_QL01',
        'NV_QL01',
        'quanly',
        'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=',
        'Q_MANAGER',
        1
    );
END
ELSE
BEGIN
    UPDATE tbl_TaiKhoan
    SET sMatKhauHash = 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=',
        bTrangThai = 1
    WHERE sTenDangNhap = 'quanly';
END
GO

-- 4.3. Tài khoản Nhân viên (Q_STAFF)
IF NOT EXISTS (SELECT 1 FROM tbl_TaiKhoan WHERE sTenDangNhap = 'nhanvien')
BEGIN
    INSERT INTO tbl_TaiKhoan (sMaTK, FK_sMaNV, sTenDangNhap, sMatKhauHash, FK_sMaQuyen, bTrangThai)
    VALUES (
        'TK_NV01',
        'NV_NV01',
        'nhanvien',
        'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=',
        'Q_STAFF',
        1
    );
END
ELSE
BEGIN
    UPDATE tbl_TaiKhoan
    SET sMatKhauHash = 'PBKDF2$100000$n6ebWubN6S/iIh3e2HxlIg==$KZ0sEabZ6L/eIrM+vsqnOHTJ/vqjQng+edA1vK91n6k=',
        bTrangThai = 1
    WHERE sTenDangNhap = 'nhanvien';
END
GO

-- 5. KIỂM TRA LẠI KẾT QUẢ DANH SÁCH TÀI KHOẢN
SELECT 
    tk.sMaTK AS [Mã TK],
    tk.sTenDangNhap AS [Tên Đăng Nhập],
    nv.sTenNV AS [Họ Tên NV],
    q.sTenQuyen AS [Tên Quyền],
    tk.FK_sMaQuyen AS [Mã Quyền],
    CASE WHEN tk.bTrangThai = 1 THEN N'Hoạt động' ELSE N'Bị khóa' END AS [Trạng Thái]
FROM tbl_TaiKhoan tk
INNER JOIN tbl_NhanVien nv ON tk.FK_sMaNV = nv.sMaNV
INNER JOIN tbl_Quyen q ON tk.FK_sMaQuyen = q.sMaQuyen;
GO
