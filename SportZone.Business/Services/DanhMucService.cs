using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

// UC23–UC27: loại sân, sân, khung giờ, bảng giá, dịch vụ
public class DanhMucService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    public DanhMucService(IDbContextFactory<SportZoneDbContext> dbf) => _dbf = dbf;

    // Loại sân
    public List<LoaiSan> DanhSachLoaiSan(bool caAn = false)
    {
        using var db = _dbf.CreateDbContext();
        return db.LoaiSans.AsNoTracking().Where(l => caAn || !l.DaAn).OrderBy(l => l.MaLoai).ToList();
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

    public KetQua AnHienLoaiSan(int ma)
    {
        using var db = _dbf.CreateDbContext();
        var l = db.LoaiSans.FirstOrDefault(x => x.MaLoai == ma);
        if (l == null) return KetQua.Loi("Không tìm thấy loại sân.");
        if (!l.DaAn && CoDonSapToi(db, d => d.San!.MaLoai == ma))
            return KetQua.Loi("Loại sân này còn đơn đặt chưa diễn ra. Xử lý xong các đơn đó rồi mới ẩn được.");
        l.DaAn = !l.DaAn;
        db.SaveChanges();
        return KetQua.Ok(l.DaAn ? $"Đã ẩn loại sân {l.TenLoai}." : $"Đã hiện lại loại sân {l.TenLoai}.");
    }

    // Sân
    public List<San> DanhSachSan(int? maLoai = null, bool chiHoatDong = false, bool caAn = false)
    {
        using var db = _dbf.CreateDbContext();
        return db.Sans.AsNoTracking().Include(s => s.LoaiSan)
            .Where(s => (maLoai == null || s.MaLoai == maLoai) && (!chiHoatDong || s.TrangThai == TrangThaiSan.HoatDong))
            .Where(s => caAn || (!s.DaAn && !s.LoaiSan!.DaAn))
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

    // Ẩn/hiện sân. Sân ẩn không hiện trên Web, lịch sân và form đặt tại quầy; đơn cũ vẫn giữ.
    public KetQua AnHienSan(int maSan)
    {
        using var db = _dbf.CreateDbContext();
        var s = db.Sans.FirstOrDefault(x => x.MaSan == maSan);
        if (s == null) return KetQua.Loi("Không tìm thấy sân.");
        if (!s.DaAn && CoDonSapToi(db, d => d.MaSan == maSan))
            return KetQua.Loi("Sân này còn đơn đặt chưa diễn ra. Xử lý xong các đơn đó (hoặc chuyển Bảo trì) rồi mới ẩn được.");
        s.DaAn = !s.DaAn;
        db.SaveChanges();
        return KetQua.Ok(s.DaAn ? $"Đã ẩn {s.TenSan}." : $"Đã hiện lại {s.TenSan}.");
    }

    // Còn đơn chưa hủy, chưa hoàn thành từ hôm nay trở đi thì không cho ẩn, tránh đơn biến mất khỏi lịch sân
    private static bool CoDonSapToi(SportZoneDbContext db, System.Linq.Expressions.Expression<Func<DatSan, bool>> dieuKien)
    {
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        return db.DatSans.Where(dieuKien).Any(d => d.NgaySuDung >= homNay
            && (d.TrangThai == TrangThaiDon.ChoXacNhan || d.TrangThai == TrangThaiDon.DaXacNhan || d.TrangThai == TrangThaiDon.DangChoi));
    }

    // Khung giờ
    public List<KhungGio> DanhSachKhungGio(bool caAn = false)
    {
        using var db = _dbf.CreateDbContext();
        return db.KhungGios.AsNoTracking().Where(k => caAn || !k.DaAn).OrderBy(k => k.GioBatDau).ToList();
    }

    public KetQua LuuKhungGio(int? ma, string ten, TimeOnly batDau, TimeOnly ketThuc)
    {
        if (string.IsNullOrWhiteSpace(ten)) return KetQua.Loi("Vui lòng nhập tên khung giờ.");
        if (ketThuc <= batDau) return KetQua.Loi("Giờ kết thúc phải sau giờ bắt đầu.");
        if (batDau.Minute % 30 != 0 || ketThuc.Minute % 30 != 0) return KetQua.Loi("Khung giờ phải theo block 30 phút.");
        using var db = _dbf.CreateDbContext();
                if (db.KhungGios.Any(k => k.MaKhungGio != (ma ?? 0) && !k.DaAn && batDau < k.GioKetThuc && ketThuc > k.GioBatDau))
            return KetQua.Loi("Khung giờ bị chồng lên một khung giờ khác.");
        var k = ma == null ? db.KhungGios.Add(new KhungGio()).Entity : db.KhungGios.First(x => x.MaKhungGio == ma);
        k.TenKhungGio = ten.Trim(); k.GioBatDau = batDau; k.GioKetThuc = ketThuc;
        db.SaveChanges();
        return KetQua.Ok("Đã lưu khung giờ.");
    }

    // Ẩn/hiện khun giờ. Khung ẩn không dùng để tính giá nên giờ đó không đặt được; bảng giá vẫn giữ.
    public KetQua AnHienKhungGio(int ma)
    {
        using var db = _dbf.CreateDbContext();
        var k = db.KhungGios.FirstOrDefault(x => x.MaKhungGio == ma);
        if (k == null) return KetQua.Loi("Không tìm thấy khung giờ.");
        if (k.DaAn && db.KhungGios.Any(x => x.MaKhungGio != ma && !x.DaAn && k.GioBatDau < x.GioKetThuc && k.GioKetThuc > x.GioBatDau))
            return KetQua.Loi("Không hiện lại được vì khung giờ này chồng lên một khung giờ đang dùng.");
        k.DaAn = !k.DaAn;
        db.SaveChanges();
        return KetQua.Ok(k.DaAn ? $"Đã ẩn khung giờ {k.TenKhungGio}." : $"Đã hiện lại khung giờ {k.TenKhungGio}.");
    }

    // Bảng giá
    public List<BangGia> BangGiaCuaLoai(int maLoai)
    {
        using var db = _dbf.CreateDbContext();
                return db.BangGias.AsNoTracking().Where(g => !g.KhungGio!.DaAn).ToList();
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

    // Dịch vụ
    public List<DichVu> DanhSachDichVu(bool caAn = false)
    {
        using var db = _dbf.CreateDbContext();
        return db.DichVus.AsNoTracking().Where(d => caAn || !d.DaAn).OrderBy(d => d.TenDV).ToList();
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

    // Ẩn/hiện dịch vụ. Dịch vụ ẩn không thêm mới vào đơn được; đơn đã có dịch vụ đó vẫn giữ nguyên.
    public KetQua AnHienDichVu(int ma)
    {
        using var db = _dbf.CreateDbContext();
        var d = db.DichVus.FirstOrDefault(x => x.MaDV == ma);
        if (d == null) return KetQua.Loi("Không tìm thấy dịch vụ.");
        d.DaAn = !d.DaAn;
        db.SaveChanges();
        return KetQua.Ok(d.DaAn ? $"Đã ẩn dịch vụ {d.TenDV}." : $"Đã hiện lại dịch vụ {d.TenDV}.");
    }
}
