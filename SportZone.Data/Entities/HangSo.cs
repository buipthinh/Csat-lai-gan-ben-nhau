namespace SportZone.Data.Entities;

public static class TrangThaiDon
{
    public const string ChoXacNhan = "Chờ xác nhận";
    public const string DaXacNhan = "Đã xác nhận";
    public const string DangChoi = "Đang chơi";
    public const string HoanThanh = "Hoàn thành";
    public const string DaHuy = "Đã hủy";

    public static readonly string[] TatCa = { ChoXacNhan, DaXacNhan, DangChoi, HoanThanh, DaHuy };
}

public static class TrangThaiSan
{
    public const string HoatDong = "Hoạt động";
    public const string BaoTri = "Bảo trì";
}

public static class TrangThaiTaiKhoan
{
    public const string HoatDong = "Hoạt động";
    public const string DaKhoa = "Đã khóa";
}

public static class TenVaiTro
{
    public const string NhanVien = "Nhân viên";
    public const string QuanLy = "Quản lý";
}

public static class PhuongThucThanhToan
{
    public const string TienMat = "Tiền mặt";
    public const string ChuyenKhoan = "Chuyển khoản";
}
