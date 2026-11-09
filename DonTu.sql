
USE db_HRM_Tocotoco;
GO

-- Bổ sung ngày duyệt
IF COL_LENGTH('tbl_DonTu', 'dNgayDuyet') IS NULL
BEGIN
    ALTER TABLE tbl_DonTu
    ADD dNgayDuyet DATETIME NULL;
END
GO

-- Bổ sung khóa ngoại người duyệt
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_DonTu_NguoiDuyet'
)
BEGIN
    ALTER TABLE tbl_DonTu
    ADD CONSTRAINT FK_DonTu_NguoiDuyet
    FOREIGN KEY (sNguoiDuyet)
    REFERENCES tbl_NhanVien(sMaNV);
END
GO

USE db_HRM_Tocotoco;
GO

CREATE OR ALTER VIEW vw_DanhSachDonTu
AS
SELECT
    dt.sMaDon,
    dt.FK_sMaNV,
    nv.sTenNV AS sTenNguoiTao,
    dt.sLoaiDon,
    dt.dNgayGui,
    dt.sLyDo,
    dt.sTrangThai,
    dt.sNguoiDuyet,
    nd.sTenNV AS sTenNguoiDuyet,
    dt.dNgayDuyet
FROM tbl_DonTu dt
INNER JOIN tbl_NhanVien nv
    ON dt.FK_sMaNV = nv.sMaNV
LEFT JOIN tbl_NhanVien nd
    ON dt.sNguoiDuyet = nd.sMaNV;
GO
INSERT INTO tbl_DonTu
(
    sMaDon,
    FK_sMaNV,
    sLoaiDon,
    dNgayGui,
    sLyDo,
    sTrangThai
)
VALUES
(
    'DT001',
    'NV001',
    N'Nghỉ phép',
    GETDATE(),
    N'Xin nghỉ phép việc cá nhân',
    N'Chờ duyệt'
);

USE db_HRM_Tocotoco;
GO

INSERT INTO tbl_NhanVien
(
    sMaNV,
    sTenNV,
    dNgaySinh,
    sCCCD,
    sGioiTinh,
    sDiaChi,
    sSDT,
    sEmail,
    dNgayVaoLam,
    sTrangThai,
    FK_sMaCV,
    FK_sMaPB,
    fLuongcb
)
VALUES
('NV001', N'Nguyễn Văn An', '2000-05-12', '001200000001',
 N'Nam', N'Hà Nội', '0901000001', 'an@gmail.com',
 '2024-01-10', N'Đang làm việc', 'CV_ADMIN', 'PB_HR', 15000000),

('NV002', N'Trần Thị Bình', '1999-08-20', '001200000002',
 N'Nữ', N'Hà Nội', '0901000002', 'binh@gmail.com',
 '2024-02-15', N'Đang làm việc', 'CV_QLCH', 'PB_CH01', 12000000),

('NV003', N'Lê Văn Cường', '2001-03-15', '001200000003',
 N'Nam', N'Hải Phòng', '0901000003', 'cuong@gmail.com',
 '2024-03-01', N'Đang làm việc', 'CV_TN', 'PB_CH01', 10000000),

('NV004', N'Phạm Thị Dung', '2002-07-25', '001200000004',
 N'Nữ', N'Hà Nội', '0901000004', 'dung@gmail.com',
 '2024-04-05', N'Đang làm việc', 'CV_PC', 'PB_CH01', 7000000),

('NV005', N'Hoàng Văn Đức', '2000-11-18', '001200000005',
 N'Nam', N'Hải Phòng', '0901000005', 'duc@gmail.com',
 '2024-05-12', N'Đang làm việc', 'CV_QLCH', 'PB_CH02', 12000000),

('NV006', N'Vũ Thị Hoa', '2001-09-09', '001200000006',
 N'Nữ', N'Hà Nội', '0901000006', 'hoa@gmail.com',
 '2024-06-20', N'Đang làm việc', 'CV_TN', 'PB_CH02', 10000000),

('NV007', N'Đặng Văn Hùng', '2002-02-14', '001200000007',
 N'Nam', N'Hà Nội', '0901000007', 'hung@gmail.com',
 '2024-07-01', N'Đang làm việc', 'CV_PC', 'PB_CH02', 7000000),

('NV008', N'Ngô Thị Lan', '2000-12-30', '001200000008',
 N'Nữ', N'Hải Phòng', '0901000008', 'lan@gmail.com',
 '2024-08-15', N'Đang làm việc', 'CV_PC', 'PB_CH01', 7000000);
GO
INSERT INTO tbl_DonTu
(
    sMaDon,
    FK_sMaNV,
    sLoaiDon,
    dNgayGui,
    sLyDo,
    sTrangThai
)
VALUES
('DT001', 'NV001', N'Nghỉ phép', GETDATE(),
 N'Xin nghỉ phép vì lý do cá nhân', N'Chờ duyệt');
