# SportZone — Hệ thống đặt sân thể thao

Đồ án môn học, viết bằng C# (.NET 10) theo đúng tài liệu **PRD SportZone**: web cho khách hàng,
ứng dụng desktop cho nhân viên và quản lý, dùng chung một cơ sở dữ liệu SQL Server.

## 1. Cấu trúc Solution

| Project | Loại | Nội dung |
|---|---|---|
| `SportZone.Data` | Class Library | 12 entity theo lược đồ quan hệ, `SportZoneDbContext` (EF Core), ràng buộc, migration |
| `SportZone.Business` | Class Library | Toàn bộ quy tắc nghiệp vụ BR01–BR10 và các service dùng chung cho cả hai ứng dụng |
| `SportZone.Web` | ASP.NET Core MVC | Giao diện khách hàng (UC01–UC11) |
| `SportZone.Desktop` | WinForms | Giao diện nhân viên và quản lý (UC12–UC31) |
| `SportZone.Tests` | xUnit | 40 test cho các quy tắc nghiệp vụ |
| `Database/` | Script SQL | `SportZone_Database.sql`: tạo database, 12 bảng, ràng buộc, dữ liệu mẫu, view báo cáo |

Web và Desktop **không truy cập cơ sở dữ liệu trực tiếp**, chỉ gọi service trong `SportZone.Business`,
nên một quy tắc nghiệp vụ chỉ được viết một lần (ví dụ cách tính tiền, cách kiểm tra trùng lịch).

## 2. Yêu cầu môi trường

- .NET 10 SDK
- SQL Server 2019 trở lên hoặc SQL Server LocalDB (đã cài kèm Visual Studio)
- Visual Studio 2026 (hoặc Visual Studio 2022 bản 17.14 trở lên có cài .NET 10 SDK), cần cho project WinForms

## 3. Cách chạy

1. Mở `SportZone.sln` bằng Visual Studio.
2. Kiểm tra chuỗi kết nối trong `SportZone.Web/appsettings.json` và `SportZone.Desktop/appsettings.json`:

   ```json
   "ConnectionStrings": { "SportZone": "Server=(localdb)\\MSSQLLocalDB;Database=SportZone;Trusted_Connection=True;TrustServerCertificate=True" }
   ```

3. Tạo database theo **một trong hai cách**:
   - **Cách A (khuyên dùng khi nộp bài):** mở `Database/SportZone_Database.sql` trong SQL Server Management Studio,
     kết nối `(localdb)\MSSQLLocalDB`, bấm Execute. Script tạo database `SportZone`, 12 bảng và dữ liệu mẫu,
     ngày của các đơn tính theo ngày bạn chạy script.
   - **Cách B:** bỏ qua bước này, chương trình **tự tạo cơ sở dữ liệu và nạp dữ liệu mẫu** ở lần chạy đầu.
4. Chạy `SportZone.Web` (hoặc `SportZone.Desktop`). Dữ liệu mẫu gồm: 13 sân thuộc 5 loại, bảng giá, dịch vụ, khách hàng và khoảng 30 ngày đơn đặt sân
   để màn hình thống kê có số liệu.
5. Muốn chạy hai ứng dụng cùng lúc: nhấp phải Solution → *Set Startup Projects* → *Multiple startup projects*,
   chọn `SportZone.Web` và `SportZone.Desktop`.

### Tài khoản có sẵn

| Ứng dụng | Tài khoản | Mật khẩu | Vai trò |
|---|---|---|---|
| Desktop | `admin` | `admin123` | Quản lý (thấy đủ menu danh mục và thống kê) |
| Desktop | `nhanvien` | `nv123456` | Nhân viên (chỉ đặt sân, khách hàng) |
| Web | `0901234567` | `123456` | Khách hàng Lê Minh Khoa |
| Web | `0912345678` | `123456` | Khách hàng Phạm Thu Hà |

### Chạy bằng SQLite (khi máy chưa có SQL Server)

Đổi `"DatabaseProvider"` thành `"Sqlite"` và chuỗi kết nối thành `"Data Source=sportzone.db"`.

### Chạy test

```
dotnet test SportZone.Tests
```

## 4. Quy tắc nghiệp vụ đã cài đặt

| Mã | Quy tắc | Nơi cài đặt |
|---|---|---|
| BR01 | Sân mở cửa 5:00–23:00 | `QuyTac.KiemTraKhungGio` |
| BR02 | Đặt theo block 30 phút, tối thiểu 1 giờ | `QuyTac.KiemTraKhungGio` |
| BR03 | Giá theo khung giờ, phân biệt ngày thường và cuối tuần | `GiaService.TinhTien` |
| BR04 | Không đặt giờ đã qua | `QuyTac.KiemTraKhungGio` |
| BR05, BR06 | Không trùng lịch, kiểm tra trong transaction Serializable | `DatSanService.TaoDon` |
| BR07 | Khách chỉ tự hủy khi còn hơn 3 giờ | `DatSanService.KhachDuocHuy` |
| BR08 | Sân bảo trì không nhận đặt | `DatSanService.TaoDon` |
| BR09 | Đặt online cọc 30%, làm tròn nghìn | `QuyTac.TinhCoc` |
| BR10 | Tài khoản bị khóa không đăng nhập được; menu lọc theo vai trò | `TaiKhoanService`, `FrmMain` |

Trạng thái đơn đi theo đúng vòng: **Chờ xác nhận → Đã xác nhận → Đang chơi → Hoàn thành**, hoặc **Đã hủy**.
Đơn đặt tại quầy được xác nhận ngay và không thu cọc.

## 5. Màn hình chính

**Web (khách hàng):** trang chủ kèm ô tìm sân trống, danh sách sân theo môn, chi tiết sân với lưới giờ 30 phút
(ô xám là đã có người đặt), trang xác nhận hiển thị chi tiết tiền sân từng khung giờ và tiền cọc,
lịch sử đặt sân, chi tiết đơn, hủy đơn, đăng ký, đăng nhập, cập nhật thông tin, đổi mật khẩu.

**Desktop (nhân viên):** lịch sân dạng lưới (dòng là sân, cột là block 30 phút, màu theo trạng thái,
tự làm mới mỗi 30 giây, nhấp đúp để mở đơn), đặt sân tại quầy, tra cứu đơn, xác nhận hoặc từ chối,
check-in, thêm dịch vụ, thanh toán và in hóa đơn (xem trước bằng `PrintDocument`), quản lý khách hàng.

**Desktop (thêm cho quản lý):** loại sân, sân kèm chuyển bảo trì, khung giờ, bảng giá, dịch vụ,
tài khoản nhân viên, và thống kê doanh thu, tỉ lệ lấp đầy, khung giờ đông khách (biểu đồ cột vẽ bằng GDI+).

## 6. Ghi chú kỹ thuật

- Mật khẩu băm bằng **BCrypt**, không lưu mật khẩu thô.
- Mỗi service tạo `DbContext` riêng qua `IDbContextFactory`, nhờ vậy dùng được cho cả web (nhiều request)
  và WinForms (một tiến trình lâu dài).
- Đơn giá dịch vụ được **chụp lại lúc bán** (`ChiTietDichVu.DonGia`), đổi giá sau này không làm sai hóa đơn cũ.
- Giao diện WinForms viết hoàn toàn bằng code (không dùng file Designer) để dễ đọc và dễ so sánh khi làm nhóm.
