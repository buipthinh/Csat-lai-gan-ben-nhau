using System.ComponentModel.DataAnnotations;
using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Web.Models;

public class TrangChuVM
{
    public List<LoaiSan> LoaiSans { get; set; } = new();
    public List<San> Sans { get; set; } = new();
    public Dictionary<int, decimal> GiaTu { get; set; } = new();
}

public class DanhSachSanVM
{
    public int? MaLoai { get; set; }
    public List<LoaiSan> LoaiSans { get; set; } = new();
    public List<San> Sans { get; set; } = new();
    public Dictionary<int, decimal> GiaTu { get; set; } = new();
}

public class ONhoGio
{
    public TimeOnly Gio { get; set; }
    public bool DaDat { get; set; }
    public bool DaQua { get; set; }
}

public class ChiTietSanVM
{
    public San San { get; set; } = null!;
    public DateOnly Ngay { get; set; }
    public List<ONhoGio> CacO { get; set; } = new();
    public List<BangGia> BangGia { get; set; } = new();
    public List<KhungGio> KhungGios { get; set; } = new();
}

public class TimSanTrongVM
{
    public int? MaLoai { get; set; }
    public DateOnly Ngay { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public List<LoaiSan> LoaiSans { get; set; } = new();
    public List<San>? KetQua { get; set; }
    public string? Loi { get; set; }
}

public class XacNhanDatVM
{
    public San San { get; set; } = null!;
    public DateOnly Ngay { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public BaoGia? BaoGia { get; set; }
    public string? Loi { get; set; }
}

public class DangKyVM
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên"), Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại"), Display(Name = "Số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 số, bắt đầu bằng 0")]
    public string SDT { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email không hợp lệ"), Display(Name = "Email (không bắt buộc)")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";
    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")]
    public string NhapLai { get; set; } = "";
}

public class DangNhapVM
{
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại hoặc email"), Display(Name = "Số điện thoại hoặc email")]
    public string TaiKhoan { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class ThongTinVM
{
    public string SDT { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập họ tên"), Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";
    [EmailAddress(ErrorMessage = "Email không hợp lệ"), Display(Name = "Email")]
    public string? Email { get; set; }
}

public class DoiMatKhauVM
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại"), DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")]
    public string MatKhauCu { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới"), MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [DataType(DataType.Password), Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = "";
    [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu mới")]
    public string NhapLai { get; set; } = "";
}
