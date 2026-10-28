using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

public record DoanhThuMoc(string Nhan, decimal DoanhThu, int SoHoaDon);

// UC29 Thống kê doanh thu
public class ThongKeService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    public ThongKeService(IDbContextFactory<SportZoneDbContext> dbf) => _dbf = dbf;

    private List<HoaDon> HoaDonTrong(SportZoneDbContext db, DateTime tu, DateTime den) =>
        // Tính tiền phía ứng dụng để chạy được cả trên SQL Server lẫn SQLite
        db.HoaDons.AsNoTracking().Where(h => h.NgayLap >= tu && h.NgayLap < den).ToList();

    public List<DoanhThuMoc> DoanhThuTheoNgay(DateOnly tuNgay, DateOnly denNgay)
    {
        using var db = _dbf.CreateDbContext();
        var hds = HoaDonTrong(db, tuNgay.ToDateTime(TimeOnly.MinValue), denNgay.AddDays(1).ToDateTime(TimeOnly.MinValue));
        var kq = new List<DoanhThuMoc>();
        for (var d = tuNgay; d <= denNgay; d = d.AddDays(1))
        {
            var trongNgay = hds.Where(h => DateOnly.FromDateTime(h.NgayLap) == d).ToList();
            kq.Add(new DoanhThuMoc(d.ToString("dd/MM"), trongNgay.Sum(h => h.TongTien), trongNgay.Count));
        }
        return kq;
    }

    public List<DoanhThuMoc> DoanhThuTheoThang(int nam)
    {
        using var db = _dbf.CreateDbContext();
        var hds = HoaDonTrong(db, new DateTime(nam, 1, 1), new DateTime(nam + 1, 1, 1));
        return Enumerable.Range(1, 12).Select(m =>
        {
            var t = hds.Where(h => h.NgayLap.Month == m).ToList();
            return new DoanhThuMoc($"T{m}", t.Sum(h => h.TongTien), t.Count);
        }).ToList();
    }
}
