using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

// UC22 Quản lý khách hàng (tại quầy)
public class KhachHangService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    public KhachHangService(IDbContextFactory<SportZoneDbContext> dbf) => _dbf = dbf;

    public List<KhachHang> TimKiem(string? tuKhoa)
    {
        using var db = _dbf.CreateDbContext();
        var q = db.KhachHangs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            q = q.Where(x => x.HoTen.Contains(k) || x.SDT.Contains(k) || (x.Email != null && x.Email.Contains(k)));
        }
        return q.OrderBy(x => x.HoTen).ToList();
    }

    public KetQua<KhachHang> Luu(int? maKH, string hoTen, string sdt, string? email)
    {
        hoTen = hoTen.Trim(); sdt = TaiKhoanService.ChuanHoaSdt(sdt);
        if (hoTen.Length == 0) return KetQua<KhachHang>.Loi("Vui lòng nhập họ tên.");
        if (sdt.Length < 9) return KetQua<KhachHang>.Loi("Số điện thoại không hợp lệ.");
        using var db = _dbf.CreateDbContext();
        if (db.KhachHangs.Any(k => k.SDT == sdt && k.MaKH != (maKH ?? 0)))
            return KetQua<KhachHang>.Loi("Số điện thoại đã thuộc về khách khác.");
        var kh = maKH == null ? db.KhachHangs.Add(new KhachHang()).Entity : db.KhachHangs.First(k => k.MaKH == maKH);
        kh.HoTen = hoTen; kh.SDT = sdt; kh.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        db.SaveChanges();
        return KetQua<KhachHang>.Ok(kh, "Đã lưu khách hàng.");
    }

    public KetQua Xoa(int maKH)
    {
        using var db = _dbf.CreateDbContext();
        if (db.DatSans.Any(d => d.MaKH == maKH)) return KetQua.Loi("Khách đã có đơn đặt sân nên không xóa được.");
        var kh = db.KhachHangs.FirstOrDefault(k => k.MaKH == maKH);
        if (kh == null) return KetQua.Loi("Không tìm thấy khách hàng.");
        db.KhachHangs.Remove(kh);
        db.SaveChanges();
        return KetQua.Ok("Đã xóa khách hàng.");
    }
}
