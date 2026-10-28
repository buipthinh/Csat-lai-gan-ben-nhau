using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SportZone.Business;
using SportZone.Business.Services;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Tests;

// Mỗi test dùng một CSDL SQLite in-memory riêng, nạp dữ liệu mẫu giống lúc chạy thật.
public sealed class CsdlThu : IDbContextFactory<SportZoneDbContext>, IDisposable
{
    private readonly SqliteConnection _ketNoi;

    public CsdlThu(bool taoDonMau = false)
    {
        _ketNoi = new SqliteConnection("Data Source=:memory:");
        _ketNoi.Open();
        using var db = CreateDbContext();
        DbSeeder.KhoiTao(db, taoDonMau);
    }

    public SportZoneDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<SportZoneDbContext>().UseSqlite(_ketNoi).Options);

    public void Dispose() => _ketNoi.Dispose();

    public int MaSanBongDa5 => LayMaSan("Sân 5A");
    public int MaSanCauLong => LayMaSan("Cầu lông 1");
    public int MaSanBaoTri => LayMaSan("Tennis 2");

    public int LayMaSan(string tenSan)
    {
        using var db = CreateDbContext();
        return db.Sans.First(s => s.TenSan == tenSan).MaSan;
    }

    public int MaKhachDauTien()
    {
        using var db = CreateDbContext();
        return db.KhachHangs.OrderBy(k => k.MaKH).First().MaKH;
    }

    public int MaNhanVien(string tenDangNhap = "nhanvien")
    {
        using var db = CreateDbContext();
        return db.NhanViens.First(n => n.TenDangNhap == tenDangNhap).MaNV;
    }

    public DatSan LayDon(int maDon)
    {
        using var db = CreateDbContext();
        return db.DatSans.AsNoTracking().First(d => d.MaDatSan == maDon);
    }

    // Thứ hai tới (ngày thường) và thứ bảy tới (cuối tuần), tính từ mốc thời gian giả lập
    public static readonly DateTime BayGio = new(2026, 1, 5, 9, 0, 0);   // thứ hai
    public static DateOnly NgayThuong => new(2026, 1, 7);                 // thứ tư
    public static DateOnly NgayCuoiTuan => new(2026, 1, 10);              // thứ bảy

    public DatSanService DichVuDatSan() => new(this, () => BayGio);
    public GiaService DichVuGia() => new(this);
    public ThanhToanService DichVuThanhToan() => new(this);
    public TaiKhoanService DichVuTaiKhoan() => new(this);
    public ThongKeService DichVuThongKe() => new(this);
    public DanhMucService DichVuDanhMuc() => new(this);
}
