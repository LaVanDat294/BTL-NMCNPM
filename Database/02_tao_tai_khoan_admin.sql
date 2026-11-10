USE db_HRM_Tocotoco;
GO

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
