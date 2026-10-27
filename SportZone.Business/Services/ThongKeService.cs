using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

public record DoanhThuMoc(string Nhan, decimal DoanhThu, int SoHoaDon);
public record LapDaySan(string TenSan, double SoGioDaDat, double SoGioMoCua)
{
    public double TiLe => SoGioMoCua == 0 ? 0 : SoGioDaDat / SoGioMoCua;
}
public record LuotTheoGio(int Gio, int SoBlock);

// UC29–UC31
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

    // Tỉ lệ lấp đầy = số giờ đã đặt (đơn chưa hủy) / số giờ mở cửa (18 giờ mỗi ngày)
    public List<LapDaySan> TiLeLapDay(DateOnly tuNgay, DateOnly denNgay)
    {
        using var db = _dbf.CreateDbContext();
        var soNgay = denNgay.DayNumber - tuNgay.DayNumber + 1;
        var gioMoCua = (QuyTac.GioDongCua - QuyTac.GioMoCua).TotalHours * soNgay;
        var dons = db.DatSans.AsNoTracking()
            .Where(d => d.NgaySuDung >= tuNgay && d.NgaySuDung <= denNgay && d.TrangThai != TrangThaiDon.DaHuy)
            .ToList();
        return db.Sans.AsNoTracking().OrderBy(s => s.MaLoai).ThenBy(s => s.TenSan).ToList()
            .Select(s => new LapDaySan(s.TenSan, dons.Where(d => d.MaSan == s.MaSan).Sum(d => d.SoGio), gioMoCua))
            .ToList();
    }

    // Đếm số block 30 phút được đặt theo từng giờ trong ngày
    public List<LuotTheoGio> KhungGioDong(DateOnly tuNgay, DateOnly denNgay)
    {
        using var db = _dbf.CreateDbContext();
        var dons = db.DatSans.AsNoTracking()
            .Where(d => d.NgaySuDung >= tuNgay && d.NgaySuDung <= denNgay && d.TrangThai != TrangThaiDon.DaHuy)
            .ToList();
        var dem = new int[24];
        foreach (var d in dons)
            for (var t = d.GioBatDau; t < d.GioKetThuc; t = t.AddMinutes(QuyTac.PhutMoiBlock))
                dem[t.Hour]++;
        return Enumerable.Range(QuyTac.GioMoCua.Hour, QuyTac.GioDongCua.Hour - QuyTac.GioMoCua.Hour)
            .Select(h => new LuotTheoGio(h, dem[h])).ToList();
    }
}
