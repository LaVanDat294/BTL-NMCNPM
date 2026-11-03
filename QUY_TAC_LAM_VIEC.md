# QUY TẮC LÀM VIỆC GIT & GITHUB (NHÓM SE.G02)

> **Dự án**: Quản lý Nhân sự Tocotoco (HRM) | **Repository**: `LaVanDat294/BTL-NMCNPM`

---

## 1. QUY ĐỊNH NHÁNH (BRANCHING)
- **`main`**: Nhánh nộp bài. Khóa push trực tiếp, chỉ gộp code đã duyệt.
- **`develop`**: Nhánh tích hợp chung của cả nhóm.
- **Nhánh cá nhân**: Tạo từ `develop` để làm việc:
  - **Lập trình**: `feature/ten-chuc-nang` (VD: `feature/cham-cong`)
  - **Tài liệu/Test**: `docs/ten-tai-lieu` hoặc `test/ten-kiem-thu`
  - **Sửa lỗi**: `bugfix/ten-loi`

---

## 2. QUY CHUẨN COMMIT
- **Cú pháp**: `<loai>: <Noi dung ngat gon>`
  - `feat`: Code chức năng mới
  - `fix`: Sửa lỗi code / CSDL
  - `docs`: Viết/sửa tài liệu, báo cáo
  - `test`: Thêm kịch bản kiểm thử
- **Lưu ý**: Không đẩy các file rác của Visual Studio (`bin/`, `obj/`, `.vs/`).

---

## 3. QUY TRÌNH PULL REQUEST & DỰỆT CODE (APPROVAL)
1. **Kiểm tra local**: Chạy `dotnet build` đạt `0 Error` trước khi mở PR.
2. **Cập nhật code**: Kéo code `develop` mới nhất về nhánh cá nhân để xử lý xung đột (nếu có).
3. **Mở PR**: Tạo Pull Request về nhánh `develop`.
4. **Kiểm duyệt (Require Approval)**: Cần **ít nhất 01 phê duyệt (Approved)** từ Trưởng nhóm hoặc Reviewer thì mới được bấm Merge.

---

## 4. VỊ TRÍ LƯU THƯ MỤC
- **Code C#**: `Controllers/`, `Models/`, `Views/`
- **Tài liệu & Biên bản**: `docs/` (VD: `docs/FTR/`, `docs/TestPlans/`)
- **Script CSDL & Đặc tả**: `schema.sql`, `DATABASE.md` (ở thư mục gốc)
