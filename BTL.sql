USE db_HRM_Tocotoco;

-- Ki?m tra ?úng SQL Server và database
SELECT
    @@SERVERNAME AS ServerName,
    DB_NAME() AS DatabaseName;

-- Ki?m tra tài kho?n Admin
SELECT
    sMaTK,
    FK_sMaNV,
    sTenDangNhap,
    sMatKhauHash,
    FK_sMaQuyen,
    bTrangThai,
    LEN(sMatKhauHash) AS DoDaiHash
FROM tbl_TaiKhoan
WHERE sTenDangNhap = 'admin';