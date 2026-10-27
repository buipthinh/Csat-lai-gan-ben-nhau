using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

// UC23–UC27: loại sân, sân, khung giờ, bảng giá, dịch vụ
public class DanhMucService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    public DanhMucService(IDbContextFactory<SportZoneDbContext> dbf) => _dbf = dbf;

    // ---------- Loại sân ----------
    public List<LoaiSan> DanhSachLoaiSan()
    {
        using var db = _dbf.CreateDbContext();
        return db.LoaiSans.AsNoTracking().OrderBy(l => l.MaLoai).ToList();
    }

    public KetQua LuuLoaiSan(int? ma, string ten, string? moTa)
    {
        if (string.IsNullOrWhiteSpace(ten)) return KetQua.Loi("Vui lòng nhập tên loại sân.");
        using var db = _dbf.CreateDbContext();
        var l = ma == null ? db.LoaiSans.Add(new LoaiSan()).Entity : db.LoaiSans.First(x => x.MaLoai == ma);
        l.TenLoai = ten.Trim(); l.MoTa = moTa;
        db.SaveChanges();
        return KetQua.Ok("Đã lưu loại sân.");
    }

    public KetQua XoaLoaiSan(int ma)
    {
        using var db = _dbf.CreateDbContext();
        if (db.Sans.Any(s => s.MaLoai == ma)) return KetQua.Loi("Loại sân đang có sân nên không xóa được.");
        db.BangGias.RemoveRange(db.BangGias.Where(g => g.MaLoai == ma));
        var l = db.LoaiSans.FirstOrDefault(x => x.MaLoai == ma);
        if (l != null) db.LoaiSans.Remove(l);
        db.SaveChanges();
        return KetQua.Ok("Đã xóa loại sân.");
    }

    // ---------- Sân ----------
    public List<San> DanhSachSan(int? maLoai = null, bool chiHoatDong = false)
    {
        using var db = _dbf.CreateDbContext();
        return db.Sans.AsNoTracking().Include(s => s.LoaiSan)
            .Where(s => (maLoai == null || s.MaLoai == maLoai) && (!chiHoatDong || s.TrangThai == TrangThaiSan.HoatDong))
            .OrderBy(s => s.MaLoai).ThenBy(s => s.TenSan).ToList();
    }

    public San? LaySan(int maSan)
    {
        using var db = _dbf.CreateDbContext();
        return db.Sans.AsNoTracking().Include(s => s.LoaiSan).FirstOrDefault(s => s.MaSan == maSan);
    }

    public KetQua LuuSan(int? ma, string ten, int maLoai, string? hinhAnh, string? moTa, string trangThai)
    {
        if (string.IsNullOrWhiteSpace(ten)) return KetQua.Loi("Vui lòng nhập tên sân.");
        using var db = _dbf.CreateDbContext();
        var s = ma == null ? db.Sans.Add(new San()).Entity : db.Sans.First(x => x.MaSan == ma);
        s.TenSan = ten.Trim(); s.MaLoai = maLoai; s.HinhAnh = hinhAnh; s.MoTa = moTa; s.TrangThai = trangThai;
        db.SaveChanges();
        return KetQua.Ok("Đã lưu sân.");
    }

    public KetQua DoiTrangThaiSan(int maSan)
    {
        using var db = _dbf.CreateDbContext();
        var s = db.Sans.FirstOrDefault(x => x.MaSan == maSan);
        if (s == null) return KetQua.Loi("Không tìm thấy sân.");
        s.TrangThai = s.TrangThai == TrangThaiSan.HoatDong ? TrangThaiSan.BaoTri : TrangThaiSan.HoatDong;
        db.SaveChanges();
        return KetQua.Ok($"{s.TenSan}: {s.TrangThai}.");
    }

    public KetQua XoaSan(int maSan)
    {
        using var db = _dbf.CreateDbContext();
        if (db.DatSans.Any(d => d.MaSan == maSan)) return KetQua.Loi("Sân đã có đơn đặt, hãy chuyển sang Bảo trì thay vì xóa.");
        var s = db.Sans.FirstOrDefault(x => x.MaSan == maSan);
        if (s != null) { db.Sans.Remove(s); db.SaveChanges(); }
        return KetQua.Ok("Đã xóa sân.");
    }

    // ---------- Khung giờ ----------
    public List<KhungGio> DanhSachKhungGio()
    {
        using var db = _dbf.CreateDbContext();
        return db.KhungGios.AsNoTracking().OrderBy(k => k.GioBatDau).ToList();
    }

    public KetQua LuuKhungGio(int? ma, string ten, TimeOnly batDau, TimeOnly ketThuc)
    {
        if (string.IsNullOrWhiteSpace(ten)) return KetQua.Loi("Vui lòng nhập tên khung giờ.");
        if (ketThuc <= batDau) return KetQua.Loi("Giờ kết thúc phải sau giờ bắt đầu.");
        if (batDau.Minute % 30 != 0 || ketThuc.Minute % 30 != 0) return KetQua.Loi("Khung giờ phải theo block 30 phút.");
        using var db = _dbf.CreateDbContext();
        if (db.KhungGios.Any(k => k.MaKhungGio != (ma ?? 0) && batDau < k.GioKetThuc && ketThuc > k.GioBatDau))
            return KetQua.Loi("Khung giờ bị chồng lên một khung giờ khác.");
        var k = ma == null ? db.KhungGios.Add(new KhungGio()).Entity : db.KhungGios.First(x => x.MaKhungGio == ma);
        k.TenKhungGio = ten.Trim(); k.GioBatDau = batDau; k.GioKetThuc = ketThuc;
        db.SaveChanges();
        return KetQua.Ok("Đã lưu khung giờ.");
    }

    public KetQua XoaKhungGio(int ma)
    {
        using var db = _dbf.CreateDbContext();
        db.BangGias.RemoveRange(db.BangGias.Where(g => g.MaKhungGio == ma));
        var k = db.KhungGios.FirstOrDefault(x => x.MaKhungGio == ma);
        if (k != null) db.KhungGios.Remove(k);
        db.SaveChanges();
        return KetQua.Ok("Đã xóa khung giờ và giá đi kèm.");
    }

    // ---------- Bảng giá ----------
    public List<BangGia> BangGiaCuaLoai(int maLoai)
    {
        using var db = _dbf.CreateDbContext();
        return db.BangGias.AsNoTracking().Include(g => g.KhungGio)
            .Where(g => g.MaLoai == maLoai).OrderBy(g => g.LaCuoiTuan).ToList();
    }

    public List<BangGia> TatCaBangGia()
    {
        using var db = _dbf.CreateDbContext();
        return db.BangGias.AsNoTracking().ToList();
    }

    public KetQua LuuGia(int maLoai, int maKhungGio, bool laCuoiTuan, decimal gia)
    {
        if (gia < 0) return KetQua.Loi("Giá không được âm.");
        using var db = _dbf.CreateDbContext();
        var g = db.BangGias.FirstOrDefault(x => x.MaLoai == maLoai && x.MaKhungGio == maKhungGio && x.LaCuoiTuan == laCuoiTuan);
        if (g == null) db.BangGias.Add(new BangGia { MaLoai = maLoai, MaKhungGio = maKhungGio, LaCuoiTuan = laCuoiTuan, GiaMoiGio = gia });
        else g.GiaMoiGio = gia;
        db.SaveChanges();
        return KetQua.Ok("Đã lưu giá.");
    }

    // ---------- Dịch vụ ----------
    public List<DichVu> DanhSachDichVu()
    {
        using var db = _dbf.CreateDbContext();
        return db.DichVus.AsNoTracking().OrderBy(d => d.TenDV).ToList();
    }

    public KetQua LuuDichVu(int? ma, string ten, string donVi, decimal donGia)
    {
        if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(donVi)) return KetQua.Loi("Vui lòng nhập tên và đơn vị.");
        if (donGia < 0) return KetQua.Loi("Đơn giá không được âm.");
        using var db = _dbf.CreateDbContext();
        var d = ma == null ? db.DichVus.Add(new DichVu()).Entity : db.DichVus.First(x => x.MaDV == ma);
        d.TenDV = ten.Trim(); d.DonVi = donVi.Trim(); d.DonGia = donGia;
        db.SaveChanges();
        return KetQua.Ok("Đã lưu dịch vụ.");
    }

    public KetQua XoaDichVu(int ma)
    {
        using var db = _dbf.CreateDbContext();
        if (db.ChiTietDichVus.Any(c => c.MaDV == ma)) return KetQua.Loi("Dịch vụ đã được bán trong đơn nên không xóa được.");
        var d = db.DichVus.FirstOrDefault(x => x.MaDV == ma);
        if (d != null) { db.DichVus.Remove(d); db.SaveChanges(); }
        return KetQua.Ok("Đã xóa dịch vụ.");
    }
}
