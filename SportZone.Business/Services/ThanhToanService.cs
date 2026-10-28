using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

public record TamTinh(decimal TienSan, decimal TienDichVu, decimal TienCoc)
{
    public decimal TongGiaTri => TienSan + TienDichVu;
    public decimal PhaiTra => TongGiaTri - TienCoc;
}

public class ThanhToanService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    public ThanhToanService(IDbContextFactory<SportZoneDbContext> dbf) => _dbf = dbf;

    // UC20 Thêm dịch vụ vào đơn: soLuong theo mã dịch vụ, số lượng 0 nghĩa là bỏ dịch vụ đó
    public KetQua CapNhatDichVu(int maDon, IDictionary<int, int> soLuong)
    {
        using var db = _dbf.CreateDbContext();
        var don = db.DatSans.Include(d => d.ChiTietDichVus).FirstOrDefault(d => d.MaDatSan == maDon);
        if (don == null) return KetQua.Loi("Không tìm thấy đơn.");
        if (don.TrangThai != TrangThaiDon.DangChoi) return KetQua.Loi("Chỉ thêm dịch vụ cho đơn đang chơi.");
        if (soLuong.Values.Any(v => v < 0)) return KetQua.Loi("Số lượng không được âm.");

        var dichVus = db.DichVus.Where(v => soLuong.Keys.Contains(v.MaDV)).ToDictionary(v => v.MaDV);
        foreach (var (maDV, sl) in soLuong)
        {
            var ct = don.ChiTietDichVus.FirstOrDefault(c => c.MaDV == maDV);
            if (sl == 0) { if (ct != null) db.ChiTietDichVus.Remove(ct); continue; }
            if (!dichVus.TryGetValue(maDV, out var dv)) return KetQua.Loi("Không tìm thấy dịch vụ.");
            if (ct == null) don.ChiTietDichVus.Add(new ChiTietDichVu { MaDV = maDV, SoLuong = sl, DonGia = dv.DonGia });
            else ct.SoLuong = sl; // giữ đơn giá lúc bán lần đầu
        }
        db.SaveChanges();
        return KetQua.Ok("Đã cập nhật dịch vụ.");
    }

    public TamTinh? TinhTamTinh(int maDon)
    {
        using var db = _dbf.CreateDbContext();
        var don = db.DatSans.AsNoTracking().Include(d => d.ChiTietDichVus).FirstOrDefault(d => d.MaDatSan == maDon);
        if (don == null) return null;
        return new TamTinh(don.TienSan, don.ChiTietDichVus.Sum(c => c.SoLuong * c.DonGia), don.TienCoc);
    }

    // UC21 Thanh toán: tiền sân + dịch vụ − tiền cọc, tạo hóa đơn, đơn chuyển Hoàn thành
    public KetQua<HoaDon> ThanhToan(int maDon, int maNV, string phuongThuc)
    {
        using var db = _dbf.CreateDbContext();
        using var tx = db.Database.BeginTransaction();
        var don = db.DatSans.Include(d => d.ChiTietDichVus).Include(d => d.HoaDon).FirstOrDefault(d => d.MaDatSan == maDon);
        if (don == null) return KetQua<HoaDon>.Loi("Không tìm thấy đơn.");
        if (don.TrangThai != TrangThaiDon.DangChoi) return KetQua<HoaDon>.Loi("Chỉ thanh toán đơn đang chơi.");
        if (don.HoaDon != null) return KetQua<HoaDon>.Loi("Đơn này đã có hóa đơn.");

        var hd = new HoaDon
        {
            MaDatSan = maDon, MaNV = maNV, NgayLap = DateTime.Now, PhuongThucTT = phuongThuc,
            TongTien = don.TienSan + don.ChiTietDichVus.Sum(c => c.SoLuong * c.DonGia)
        };
        db.HoaDons.Add(hd);
        don.TrangThai = TrangThaiDon.HoanThanh;
        db.SaveChanges();
        tx.Commit();
        return KetQua<HoaDon>.Ok(hd, $"Đã lập hóa đơn #HD{hd.MaHD}.");
    }
}
