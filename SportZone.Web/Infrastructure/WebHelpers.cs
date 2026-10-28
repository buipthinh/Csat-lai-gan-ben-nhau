using System.Globalization;
using System.Security.Claims;
using SportZone.Data.Entities;

namespace SportZone.Web.Infrastructure;

public static class WebHelpers
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public static string Vnd(this decimal so) => so.ToString("#,##0", Vi) + " đ";

    public static int? MaKH(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var ma) ? ma : null;

    public static string Thu(this DateOnly ngay) => ngay.DayOfWeek switch
    {
        DayOfWeek.Monday => "Thứ 2", DayOfWeek.Tuesday => "Thứ 3", DayOfWeek.Wednesday => "Thứ 4",
        DayOfWeek.Thursday => "Thứ 5", DayOfWeek.Friday => "Thứ 6", DayOfWeek.Saturday => "Thứ 7", _ => "Chủ nhật"
    };

    public static string NgayDep(this DateOnly ngay) => $"{ngay.Thu()}, {ngay:dd/MM/yyyy}";

    public static string MauTrangThai(string trangThai) => trangThai switch
    {
        TrangThaiDon.ChoXacNhan => "warning",
        TrangThaiDon.DaXacNhan => "primary",
        TrangThaiDon.DangChoi => "info",
        TrangThaiDon.HoanThanh => "success",
        _ => "secondary"
    };

    // Ảnh minh họa theo loại sân khi sân chưa có HinhAnh
    public static string AnhSan(San s)
    {
        if (!string.IsNullOrEmpty(s.HinhAnh)) return s.HinhAnh;
        var ten = s.LoaiSan?.TenLoai ?? "";
        return ten.Contains("Bóng") ? "/img/bongda.jpg" : ten.Contains("Cầu") ? "/img/caulong.jpg"
             : ten.Contains("Pickle") ? "/img/pickleball.jpg" : "/img/tennis.jpg";
    }
}
