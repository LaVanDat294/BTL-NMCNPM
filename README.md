# TocoToco HRM - ASP.NET Core MVC

## Yêu cầu
- Visual Studio 2022
- .NET 8 SDK
- SQL Server

## Chạy dự án
1. Mở `TocoTocoHRM.csproj` bằng Visual Studio.
2. Chạy script tạo database và bảng bạn đã cung cấp trong SQL Server.
3. Mở `appsettings.json`, sửa `Server=.;` thành tên SQL Server instance trên máy (ví dụ `DESKTOP-...\\SQLEXPRESS`).
4. Mở Package Manager Console, chạy `Restore` nếu Visual Studio chưa tự tải package.
5. Nhấn F5.

## Tạo tài khoản quản trị ban đầu
Script `Database/02_tao_tai_khoan_admin.sql` thêm nhân viên quản trị và tài khoản đăng nhập.
Tài khoản: `admin`
Mật khẩu ban đầu: `Admin@123`
Hãy đổi mật khẩu ngay sau lần đăng nhập đầu tiên. Ứng dụng dùng PBKDF2 SHA-256 để xác minh hash.

## Màu giao diện
- Sidebar: #103D65
- Nút chính: #0878F9
- Nền: #F3F6FA
- Panel: #FFFFFF

## Lưu ý
- Các trang phòng ban, chấm công, hợp đồng trên menu là mục điều hướng giao diện; phạm vi code hiện tại tập trung vào đăng nhập, trang chủ và CRUD nhân viên.
- Không xóa được nhân viên đã có tài khoản, chấm công, lương, hợp đồng hoặc đơn từ do khóa ngoại NO ACTION trong database.
