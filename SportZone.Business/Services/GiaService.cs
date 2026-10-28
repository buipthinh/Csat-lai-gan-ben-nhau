using Microsoft.EntityFrameworkCore;
using SportZone.Data;

namespace SportZone.Business.Services;

public record DongGia(string TenKhungGio, double SoGio, decimal DonGia, decimal ThanhTien);

public record BaoGia(decimal TienSan, decimal TienCoc, bool LaCuoiTuan, IReadOnlyList<DongGia> ChiTiet);

public class GiaService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    public GiaService(IDbContextFactory<SportZoneDbContext> dbf) => _dbf = dbf;

    public KetQua<BaoGia> TinhTien(int maSan, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc)
    {
        using var db = _dbf.CreateDbContext();
        return TinhTien(db, maSan, ngay, batDau, ketThuc);
    }

    // BR03, BR04: cắt khoảng giờ thành từng block 30 phút, mỗi block lấy giá của khung giờ chứa nó.
    // Ví dụ 16:30–18:00 = 0,5 giờ giá thường + 1 giờ giá vàng.
    internal static KetQua<BaoGia> TinhTien(SportZoneDbContext db, int maSan, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc)
    {
        var san = db.Sans.AsNoTracking().FirstOrDefault(s => s.MaSan == maSan);
        if (san == null) return KetQua<BaoGia>.Loi("Không tìm thấy sân.");

        var cuoiTuan = QuyTac.LaCuoiTuan(ngay);
        var khungGios = db.KhungGios.AsNoTracking().ToList();
        var bangGia = db.BangGias.AsNoTracking()
            .Where(g => g.MaLoai == san.MaLoai && g.LaCuoiTuan == cuoiTuan)
            .ToList();

        var theoKhung = new Dictionary<int, (string Ten, int Phut, decimal Gia)>();
        for (var t = batDau; t < ketThuc; t = t.AddMinutes(QuyTac.PhutMoiBlock))
        {
            var khung = khungGios.FirstOrDefault(k => k.GioBatDau <= t && t < k.GioKetThuc);
            if (khung == null) return KetQua<BaoGia>.Loi($"Chưa có khung giờ nào chứa {t:HH\\:mm}.");
            var gia = bangGia.FirstOrDefault(g => g.MaKhungGio == khung.MaKhungGio);
            if (gia == null) return KetQua<BaoGia>.Loi($"Chưa có giá cho khung \"{khung.TenKhungGio}\".");
            theoKhung.TryGetValue(khung.MaKhungGio, out var v);
            theoKhung[khung.MaKhungGio] = (khung.TenKhungGio, v.Phut + QuyTac.PhutMoiBlock, gia.GiaMoiGio);
        }

        var chiTiet = theoKhung.Values
            .Select(v => new DongGia(v.Ten, v.Phut / 60.0, v.Gia, Math.Round(v.Gia * v.Phut / 60m)))
            .ToList();
        var tien = chiTiet.Sum(c => c.ThanhTien);
        return KetQua<BaoGia>.Ok(new BaoGia(tien, QuyTac.TinhCoc(tien), cuoiTuan, chiTiet));
    }
}
