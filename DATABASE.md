# CƠ SỞ DỮ LIỆU CẮT LỚP VẬT LÝ (PHYSICAL DATABASE SCHEMA)
> **Dự án**: Hệ thống Quản lý Nhân sự, Chấm công & Tiền lương Tocotoco (HRM)  
> **Chương**: 4 - Thiết kế Hệ thống (Pha D)  
> **Đơn vị xây dựng**: Phân hệ DT - Nhóm SE.G02  
> **Phiên bản Baseline**: v1.0 (Đã nghiệm thu qua FTR 3 lần)  
> **Cơ sở Dữ liệu Quan hệ**: SQL Server T-SQL (`db_HRM_Tocotoco`)

---

## 1. QUY ƯỚC ĐẶT TÊN (NAMING CONVENTIONS)
Toàn bộ CSDL tuân thủ Phụ lục A chuẩn ISO/IEC 25010 & ISO 12207:
- **Tên CSDL**: `db_HRM_Tocotoco`
- **Tên Bảng (Table)**: Pre-fix `tbl_` + *PascalCase* (Ví dụ: `tbl_NhanVien`, `tbl_ChamCong`).
- **Khóa chính (Primary Key)**: Pre-fix `PK_` (Ví dụ: `PK_sMaNV`).
- **Khóa ngoại (Foreign Key)**: Pre-fix `FK_` (Ví dụ: `FK_sMaPB`, `FK_sMaCV`).
- **Chỉ mục (Index)**: Pre-fix `IX_` (Ví dụ: `IX_tbl_ChamCong_NV_Ngay`).
- **Tiền tố Ký hiệu Kiểu dữ liệu**:
  - `s` : Chuỗi (`VARCHAR`, `NVARCHAR`)
  - `d` : Ngày tháng / Thời gian (`DATE`, `DATETIME`)
  - `i` : Số nguyên (`INT`)
  - `f` : Số thực / Tiền tệ (`DECIMAL(18,0)`)
  - `b` : Boolean / Trạng thái (`BIT`)

---

## 2. DANH SÁCH 9 BẢNG CSDL VẬT LÝ

| STT | Tên Bảng CSDL | Tên Thực Thể Ban Đầu | Mô Tả Chức Năng |
| :---: | :--- | :--- | :--- |
| **1** | `tbl_PhongBan` | PHÒNG BAN | Danh mục Chi nhánh cửa hàng & Phòng ban |
| **2** | `tbl_ChucVu` | CHỨC VỤ | Danh mục Chức danh & Phụ cấp chức vụ |
| **3** | `tbl_Quyen` | QUYỀN | Danh mục Phân quyền người dùng (RBAC) |
| **4** | `tbl_NhanVien` | NHÂN VIÊN | Hồ sơ cá nhân & Thông tin nhân sự |
| **5** | `tbl_TaiKhoan` | TÀI KHOẢN | Tài khoản đăng nhập & Mật khẩu mã hóa Hash |
| **6** | `tbl_ChamCong` | CHẤM CÔNG | Lịch sử Check-in/out & Số giờ làm việc |
| **7** | `tbl_Luong` | LƯƠNG | Kỳ lương, Lương cơ bản, Thưởng, Phạt & Thực lĩnh |
| **8** | `tbl_HopDongLaoDong` | HỢP ĐỒNG | Hợp đồng lao động & Thời hạn hiệu lực |
| **9** | `tbl_DonTu` | ĐƠN TỪ | Đơn xin nghỉ phép, Đổi ca quầy POS |

---

## 3. ĐẶC TẢ CHI TIẾT DỮ LIỆU (DATA DICTIONARY)

### 3.1. Bảng Phòng Ban (`tbl_PhongBan`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaPB` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã phòng ban / Chi nhánh |
| `sTenPB` | `NVARCHAR(100)` | `NOT NULL` | Tên phòng ban / Tên cửa hàng |
| `sGhiChu` | `NVARCHAR(255)` | `NULL` | Ghi chú thêm |

### 3.2. Bảng Chức Vụ (`tbl_ChucVu`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaCV` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã chức vụ |
| `sTenCV` | `NVARCHAR(100)` | `NOT NULL` | Tên chức vụ (Cửa hàng trưởng, Thu ngân, Pha chế...) |
| `fPhuCap` | `DECIMAL(18,0)` | `DEFAULT 0` | Số tiền phụ cấp chức vụ |

### 3.3. Bảng Phân Quyền (`tbl_Quyen`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaQuyen` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã quyền (Q_ADMIN, Q_MANAGER, Q_STAFF) |
| `sTenQuyen` | `NVARCHAR(50)` | `NOT NULL` | Tên quyền hạn |

### 3.4. Bảng Nhân Viên (`tbl_NhanVien`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaNV` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã nhân viên |
| `sTenNV` | `NVARCHAR(100)` | `NOT NULL` | Họ và tên nhân viên |
| `dNgaySinh` | `DATETIME` | `NULL` | Ngày tháng năm sinh |
| `sCCCD` | `VARCHAR(12)` | `UNIQUE`, `NOT NULL` | Số Căn cước công dân |
| `sGioiTinh` | `NVARCHAR(10)` | `NULL` | Giới tính (Nam / Nữ / Khác) |
| `sDiaChi` | `NVARCHAR(255)` | `NULL` | Địa chỉ thường trú |
| `sSDT` | `VARCHAR(15)` | `NULL` | Số điện thoại liên hệ |
| `sEmail` | `VARCHAR(100)` | `NULL` | Thư điện tử |
| `dNgayVaoLam` | `DATETIME` | `DEFAULT GETDATE()` | Ngày chính thức vào làm |
| `sTrangThai` | `NVARCHAR(50)` | `DEFAULT N'Đang làm việc'` | Trạng thái nhân sự |
| `FK_sMaCV` | `VARCHAR(20)` | `FK` ➔ `tbl_ChucVu` | Mã chức vụ |
| `FK_sMaPB` | `VARCHAR(20)` | `FK` ➔ `tbl_PhongBan` | Mã phòng ban |
| `fLuongcb` | `DECIMAL(18,0)` | `DEFAULT 0` | Lương cơ bản |

### 3.5. Bảng Tài Khoản (`tbl_TaiKhoan`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaTK` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã tài khoản |
| `FK_sMaNV` | `VARCHAR(20)` | `FK`, `UNIQUE` ➔ `tbl_NhanVien` | Mã nhân viên sở hữu tài khoản |
| `sTenDangNhap`| `VARCHAR(50)` | `UNIQUE`, `NOT NULL` | Tên đăng nhập hệ thống |
| `sMatKhauHash`| `VARCHAR(255)`| `NOT NULL` | Mật khẩu đã mã hóa Hash |
| `FK_sMaQuyen` | `VARCHAR(20)` | `FK` ➔ `tbl_Quyen` | Quyền truy cập |
| `bTrangThai` | `BIT` | `DEFAULT 1` | Trạng thái kích hoạt (1: Active, 0: Lock) |

### 3.6. Bảng Chấm Công (`tbl_ChamCong`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaChamCong`| `VARCHAR(20)` | `PK`, `NOT NULL` | Mã bản ghi chấm công |
| `FK_sMaNV` | `VARCHAR(20)` | `FK` ➔ `tbl_NhanVien` | Mã nhân viên |
| `dNgayChamCong`| `DATE` | `NOT NULL` | Ngày ghi nhận chấm công |
| `dGioVao` | `DATETIME` | `NULL` | Thời điểm Check-in |
| `dGioRa` | `DATETIME` | `NULL` | Thời điểm Check-out |
| `iSoGioLam` | `INT` | `DEFAULT 0` | Tổng số giờ làm việc thực tế |
| `sTrangThai` | `NVARCHAR(50)` | `NULL` | Trạng thái (Đúng giờ, Đi muộn, Về sớm) |
| `sGhiChu` | `NVARCHAR(255)` | `NULL` | Ghi chú thêm |
*(Ràng buộc duy nhất: `UNIQUE (FK_sMaNV, dNgayChamCong)`; Chỉ mục: `IX_tbl_ChamCong_NV_Ngay`)*

### 3.7. Bảng Lương (`tbl_Luong`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaLuong` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã bản ghi lương |
| `FK_sMaNV` | `VARCHAR(20)` | `FK` ➔ `tbl_NhanVien` | Mã nhân viên |
| `dKyLuong` | `DATE` | `NOT NULL` | Kỳ tính lương (Tháng/Năm, dạng YYYY-MM-01) |
| `iSoGioLam` | `INT` | `DEFAULT 0` | Tổng giờ làm trong kỳ |
| `fLuongCoBan` | `DECIMAL(18,0)` | `DEFAULT 0` | Lương cơ bản |
| `fThuong` | `DECIMAL(18,0)` | `DEFAULT 0` | Tiền thưởng |
| `fPhat` | `DECIMAL(18,0)` | `DEFAULT 0` | Tiền phạt / Trừ lương |
| `fThucLinh` | `DECIMAL(18,0)` | `AS (fLuongCoBan + fThuong - fPhat) PERSISTED` | **Cột tự động tính toán thực lĩnh** |
| `dNgayTinhLuong`| `DATETIME` | `DEFAULT GETDATE()` | Ngày chốt sổ lương |
*(Ràng buộc duy nhất: `UNIQUE (FK_sMaNV, dKyLuong)`; Chỉ mục: `IX_tbl_Luong_NV_Ky`)*

### 3.8. Bảng Hợp Đồng Lao Động (`tbl_HopDongLaoDong`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaHD` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã hợp đồng |
| `FK_sMaNV` | `VARCHAR(20)` | `FK` ➔ `tbl_NhanVien` | Mã nhân viên |
| `sLoaiHD` | `NVARCHAR(50)` | `NOT NULL` | Loại hợp đồng (Thử việc, 1 năm, Không xác định) |
| `dNgayKy` | `DATE` | `NOT NULL` | Ngày ký hợp đồng |
| `dNgayHetHan` | `DATE` | `NULL` | Ngày hết hạn hợp đồng |
| `sTrangThai` | `NVARCHAR(50)` | `DEFAULT N'Hiệu lực'` | Trạng thái hợp đồng |

### 3.9. Bảng Đơn Từ (`tbl_DonTu`)
| Tên Cột | Kiểu Dữ Liệu | Ràng Buộc | Mô Tả |
| :--- | :--- | :--- | :--- |
| `sMaDon` | `VARCHAR(20)` | `PK`, `NOT NULL` | Mã đơn |
| `FK_sMaNV` | `VARCHAR(20)` | `FK` ➔ `tbl_NhanVien` | Mã nhân viên gửi đơn |
| `sLoaiDon` | `NVARCHAR(50)` | `NOT NULL` | Loại đơn (Nghỉ phép, Đổi ca, Giải trình) |
| `dNgayGui` | `DATETIME` | `DEFAULT GETDATE()` | Thời điểm gửi đơn |
| `sLyDo` | `NVARCHAR(500)` | `NULL` | Lý do xin nghỉ/đổi ca |
| `sTrangThai` | `NVARCHAR(50)` | `DEFAULT N'Chờ duyệt'` | Trạng thái (Chờ duyệt, Đã duyệt, Từ chối) |
| `sNguoiDuyet` | `VARCHAR(20)` | `NULL` | Mã quản lý phê duyệt |

---

## 4. HƯỚNG DẪN THIẾT LẬP & CẬP NHẬT CSDL

### 4.1. Khởi tạo Database trên máy Local (dành cho Coder / Tester):
1. Mở **SQL Server Management Studio (SSMS)**.
2. Vào **File ➔ Open ➔ File...** và mở tệp `schema.sql` tại thư mục gốc dự án.
3. Nhấn **F5 (Execute)** để chạy script khởi tạo CSDL `db_HRM_Tocotoco` và tự động chèn dữ liệu mẫu.
4. Mở tệp `appsettings.json` trong dự án ASP.NET Core MVC và cấu hình chuỗi kết nối:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=db_HRM_Tocotoco;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

### 4.2. Lệnh Git cập nhật CSDL & Tài liệu lên GitHub:
Mở Terminal / Git Bash tại thư mục dự án và thực hiện các lệnh:
```bash
# 1. Thêm tệp CSDL và tệp tài liệu đặc tả
git add schema.sql DATABASE.md

# 2. Commit lưu vết phiên bản CSDL
git commit -m "feat(database): Update physical SQL schema and DATABASE.md Baseline v1.0"

# 3. Đẩy dữ liệu lên nhánh chính của Repository
git push origin main
```
