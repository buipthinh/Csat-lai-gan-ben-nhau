using SportZone.Data.Entities;

namespace SportZone.Desktop;

// Nhân viên đang đăng nhập, dùng để lọc menu theo vai trò và ghi mã NV vào đơn, hóa đơn.
internal static class PhienDangNhap
{
    public static NhanVien? NhanVien { get; set; }
    public static int MaNV => NhanVien!.MaNV;
    public static string HoTen => NhanVien?.HoTen ?? "";
    public static string TenVaiTro => NhanVien?.VaiTro?.TenVaiTro ?? "";
    public static bool LaQuanLy => TenVaiTro == Data.Entities.TenVaiTro.QuanLy;
}
