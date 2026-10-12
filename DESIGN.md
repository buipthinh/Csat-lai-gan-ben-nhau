# Thiết kế Giao diện (Wireframe) & Luồng trải nghiệm người dùng (UX Flow)
**Dự án:** Hệ thống Thuyết minh & Quản lý Lễ hội ngoài trời
**Cập nhật lần cuối:** 12/10/2026
**Trạng thái:** Bản phác thảo (Low-fidelity Wireframe)

---

## 1. Tổng quan phương pháp thiết kế (UX Strategy)
Qua phân tích các rủi ro thực tế khi triển khai lễ hội ngoài trời (ánh sáng chói làm mờ camera, mã QR bị rách/che khuất, người dùng không biết cấp quyền camera), nhóm quyết định bổ sung tính năng **Nhập mã số gian hàng (Numpad)** bên cạnh việc Quét mã QR truyền thống.

**Nguyên tắc cốt lõi:**
1. **Truy cập thông tin mở:** Ai cũng có thể xem menu, giá cả và nghe thuyết minh (TTS) bằng cách nhập mã số từ bất kỳ đâu.
2. **Xác thực vị trí (Anti-fraud):** Hành động "Check-in lấy tem" bị ràng buộc bởi API Geolocation. Khách phải đứng cách gian hàng `< 30m` mới được hệ thống ghi nhận tem hợp lệ.

---

## 2. Phác thảo giao diện các màn hình chính (Wireframe)

### Màn hình 1: Trang chủ & Bản đồ (Home Screen)
Hiển thị ngay khi người dùng truy cập. Tập trung vào bản đồ định vị, tiến độ săn quà và nút thao tác nhanh (Floating Action Button).

```text
┌──────────────────────────────────────┐
│ 🌐 Lễ Hội Mùa Thu            [VN|EN] │
│ ──────────────────────────────────── │
│ 📍 Vị trí của bạn: Gần cổng chính    │
│ 🎫 Thẻ tem: 2/5 (Đổi quà tại T4)     │
│ ──────────────────────────────────── │
│                                      │
│      [ BẢN ĐỒ TƯƠNG TÁC Ở ĐÂY ]      │
│     (Vuốt, phóng to, thu nhỏ)        │
│    (Hiển thị các khu F, G, T...)     │
│                                      │
│ ──────────────────────────────────── │
│ 🕒 Sắp diễn ra: Múa lân (15p nữa)    │
│ ──────────────────────────────────── │
│                                      │
│                   ╭────────────────╮ │
│                   │ 🔢 Nhập mã / QR│ │
│                   ╰────────────────╯ │
└──────────────────────────────────────┘
