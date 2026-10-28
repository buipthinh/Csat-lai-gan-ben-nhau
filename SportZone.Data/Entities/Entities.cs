namespace SportZone.Data.Entities;

// Mỗi class tương ứng một bảng trong lược đồ quan hệ của PRD (12 bảng).

public class VaiTro
{
    public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; } = "";
    public ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}

public class NhanVien
{
    public int MaNV { get; set; }
    public string HoTen { get; set; } = "";
    public string TenDangNhap { get; set; } = "";
    public string MatKhauHash { get; set; } = "";
    public int MaVaiTro { get; set; }
    public string TrangThai { get; set; } = TrangThaiTaiKhoan.HoatDong;
    public VaiTro? VaiTro { get; set; }
}

public class KhachHang
{
    public int MaKH { get; set; }
    public string HoTen { get; set; } = "";
    public string SDT { get; set; } = "";
    public string? Email { get; set; }
    // NULL với khách được nhân viên tạo nhanh tại quầy (chưa có tài khoản Web)
    public string? MatKhauHash { get; set; }
    public int DiemTichLuy { get; set; }
    public ICollection<DatSan> DatSans { get; set; } = new List<DatSan>();
}

public class LoaiSan
{
    public int MaLoai { get; set; }
    public string TenLoai { get; set; } = "";
    public string? MoTa { get; set; }
    public ICollection<San> Sans { get; set; } = new List<San>();
    public ICollection<BangGia> BangGias { get; set; } = new List<BangGia>();
}

public class San
{
    public int MaSan { get; set; }
    public string TenSan { get; set; } = "";
    public int MaLoai { get; set; }
    public string? HinhAnh { get; set; }
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = TrangThaiSan.HoatDong;
    public LoaiSan? LoaiSan { get; set; }
}

public class KhungGio
{
    public int MaKhungGio { get; set; }
    public string TenKhungGio { get; set; } = "";
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
}

public class BangGia
{
    public int MaLoai { get; set; }
    public int MaKhungGio { get; set; }
    public bool LaCuoiTuan { get; set; }
    public decimal GiaMoiGio { get; set; }
    public LoaiSan? LoaiSan { get; set; }
    public KhungGio? KhungGio { get; set; }
}

public class DatSan
{
    public int MaDatSan { get; set; }
    public int MaKH { get; set; }
    public int MaSan { get; set; }
    public DateOnly NgaySuDung { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public decimal TienSan { get; set; }
    public decimal TienCoc { get; set; }
    public string TrangThai { get; set; } = TrangThaiDon.ChoXacNhan;
    public DateTime NgayTao { get; set; }
    public int? MaNVXacNhan { get; set; }
    public int? MaLichDinhKy { get; set; }

    public KhachHang? KhachHang { get; set; }
    public San? San { get; set; }
    public NhanVien? NhanVienXacNhan { get; set; }
    public LichDinhKy? LichDinhKy { get; set; }
    public HoaDon? HoaDon { get; set; }
    public ICollection<ChiTietDichVu> ChiTietDichVus { get; set; } = new List<ChiTietDichVu>();

    public DateTime ThoiDiemBatDau => NgaySuDung.ToDateTime(GioBatDau);
    public double SoGio => (GioKetThuc - GioBatDau).TotalHours;
}

// Bảng có trong lược đồ gốc; chức năng đặt định kỳ chưa nằm trong phạm vi lần này.
public class LichDinhKy
{
    public int MaLich { get; set; }
    public int MaKH { get; set; }
    public int MaSan { get; set; }
    public byte ThuTrongTuan { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public KhachHang? KhachHang { get; set; }
    public San? San { get; set; }
}

public class DichVu
{
    public int MaDV { get; set; }
    public string TenDV { get; set; } = "";
    public string DonVi { get; set; } = "";
    public decimal DonGia { get; set; }
}

public class ChiTietDichVu
{
    public int MaDatSan { get; set; }
    public int MaDV { get; set; }
    public int SoLuong { get; set; }
    // Giá chụp tại thời điểm bán, đổi giá dịch vụ sau này không làm sai đơn cũ
    public decimal DonGia { get; set; }
    public DatSan? DatSan { get; set; }
    public DichVu? DichVu { get; set; }
}

public class HoaDon
{
    public int MaHD { get; set; }
    public int MaDatSan { get; set; }
    public int MaNV { get; set; }
    public DateTime NgayLap { get; set; }
    // Tổng giá trị = tiền sân + tiền dịch vụ (doanh thu). Số khách trả thêm tại quầy = TongTien - TienCoc.
    public decimal TongTien { get; set; }
    public string PhuongThucTT { get; set; } = "";
    public DatSan? DatSan { get; set; }
    public NhanVien? NhanVien { get; set; }
}
