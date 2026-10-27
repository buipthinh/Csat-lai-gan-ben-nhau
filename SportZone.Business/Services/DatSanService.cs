using System.Data;
using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

public class DatSanService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    private readonly Func<DateTime> _bayGio;

    public DatSanService(IDbContextFactory<SportZoneDbContext> dbf, Func<DateTime>? dongHo = null)
    {
        _dbf = dbf;
        _bayGio = dongHo ?? (() => DateTime.Now);
    }

    // ---------- Kiểm tra trùng lịch (UC32, BR05) ----------

    internal static bool CoTrungLich(SportZoneDbContext db, int maSan, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc, int? boQuaMaDon = null) =>
        db.DatSans.Any(d => d.MaSan == maSan
                            && d.NgaySuDung == ngay
                            && d.TrangThai != TrangThaiDon.DaHuy
                            && d.MaDatSan != (boQuaMaDon ?? 0)
                            && batDau < d.GioKetThuc
                            && ketThuc > d.GioBatDau);

    public bool CoTrungLich(int maSan, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc)
    {
        using var db = _dbf.CreateDbContext();
        return CoTrungLich(db, maSan, ngay, batDau, ketThuc);
    }

    // ---------- Tra cứu ----------

    // Các đơn chưa hủy của một ngày, dùng cho lưới lịch sân (Desktop) và lưới giờ trống (Web)
    public List<DatSan> DonTrongNgay(DateOnly ngay, int? maSan = null)
    {
        using var db = _dbf.CreateDbContext();
        return db.DatSans.AsNoTracking()
            .Include(d => d.KhachHang)
            .Where(d => d.NgaySuDung == ngay && d.TrangThai != TrangThaiDon.DaHuy && (maSan == null || d.MaSan == maSan))
            .OrderBy(d => d.MaSan).ThenBy(d => d.GioBatDau)
            .ToList();
    }

    // UC03 Tìm sân trống: sân đang hoạt động, đúng loại, không vướng đơn nào trong khoảng giờ
    public List<San> TimSanTrong(int? maLoai, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc)
    {
        using var db = _dbf.CreateDbContext();
        return db.Sans.AsNoTracking().Include(s => s.LoaiSan)
            .Where(s => s.TrangThai == TrangThaiSan.HoatDong && (maLoai == null || s.MaLoai == maLoai))
            .Where(s => !db.DatSans.Any(d => d.MaSan == s.MaSan && d.NgaySuDung == ngay
                                             && d.TrangThai != TrangThaiDon.DaHuy
                                             && batDau < d.GioKetThuc && ketThuc > d.GioBatDau))
            .OrderBy(s => s.MaLoai).ThenBy(s => s.TenSan)
            .ToList();
    }

    public DatSan? ChiTiet(int maDon)
    {
        using var db = _dbf.CreateDbContext();
        return db.DatSans.AsNoTracking()
            .Include(d => d.KhachHang)
            .Include(d => d.San).ThenInclude(s => s!.LoaiSan)
            .Include(d => d.NhanVienXacNhan)
            .Include(d => d.ChiTietDichVus).ThenInclude(c => c.DichVu)
            .Include(d => d.HoaDon).ThenInclude(h => h!.NhanVien)
            .FirstOrDefault(d => d.MaDatSan == maDon);
    }

    public List<DatSan> LichSuCuaKhach(int maKH)
    {
        using var db = _dbf.CreateDbContext();
        return db.DatSans.AsNoTracking()
            .Include(d => d.San).ThenInclude(s => s!.LoaiSan)
            .Include(d => d.HoaDon)
            .Where(d => d.MaKH == maKH)
            .OrderByDescending(d => d.NgaySuDung).ThenByDescending(d => d.GioBatDau)
            .ToList();
    }

    // UC16 Tra cứu đơn đặt
    public List<DatSan> TraCuu(string? trangThai, DateOnly? tuNgay, DateOnly? denNgay, string? tuKhoa)
    {
        using var db = _dbf.CreateDbContext();
        var q = db.DatSans.AsNoTracking().Include(d => d.KhachHang).Include(d => d.San).AsQueryable();
        if (!string.IsNullOrWhiteSpace(trangThai)) q = q.Where(d => d.TrangThai == trangThai);
        if (tuNgay != null) q = q.Where(d => d.NgaySuDung >= tuNgay);
        if (denNgay != null) q = q.Where(d => d.NgaySuDung <= denNgay);
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            q = q.Where(d => d.KhachHang!.HoTen.Contains(k) || d.KhachHang.SDT.Contains(k) || d.San!.TenSan.Contains(k));
        }
        return q.OrderByDescending(d => d.NgaySuDung).ThenBy(d => d.GioBatDau).ToList();
    }

    public int SoDonChoXacNhan()
    {
        using var db = _dbf.CreateDbContext();
        return db.DatSans.Count(d => d.TrangThai == TrangThaiDon.ChoXacNhan);
    }

    // ---------- Đặt sân (UC08 trên Web, UC15 tại quầy) ----------

    public KetQua<DatSan> DatSanOnline(int maKH, int maSan, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc) =>
        TaoDon(maKH, maSan, ngay, batDau, ketThuc, TrangThaiDon.ChoXacNhan, maNV: null, thuCoc: true);

    // Đặt tại quầy: nhân viên tạo nên xác nhận luôn, khách trả tiền khi chơi xong nên không thu cọc
    public KetQua<DatSan> DatTaiQuay(int maKH, int maSan, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc, int maNV) =>
        TaoDon(maKH, maSan, ngay, batDau, ketThuc, TrangThaiDon.DaXacNhan, maNV, thuCoc: false);

    private KetQua<DatSan> TaoDon(int maKH, int maSan, DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc,
                                  string trangThai, int? maNV, bool thuCoc)
    {
        var loi = QuyTac.KiemTraKhungGio(ngay, batDau, ketThuc, _bayGio());
        if (loi != null) return KetQua<DatSan>.Loi(loi);

        using var db = _dbf.CreateDbContext();
        try
        {
            // BR06: kiểm tra trùng và lưu đơn trong cùng một transaction mức Serializable.
            // Nếu hai người đặt cùng lúc, chỉ một transaction commit được; người còn lại nhận thông báo trùng.
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);

            var san = db.Sans.FirstOrDefault(s => s.MaSan == maSan);
            if (san == null) return KetQua<DatSan>.Loi("Không tìm thấy sân.");
            if (san.TrangThai != TrangThaiSan.HoatDong) return KetQua<DatSan>.Loi("Sân đang bảo trì, tạm không nhận đặt."); // BR08
            if (!db.KhachHangs.Any(k => k.MaKH == maKH)) return KetQua<DatSan>.Loi("Không tìm thấy khách hàng.");
            if (CoTrungLich(db, maSan, ngay, batDau, ketThuc))
                return KetQua<DatSan>.Loi("Khung giờ này đã có người đặt. Vui lòng chọn giờ khác.");

            var gia = GiaService.TinhTien(db, maSan, ngay, batDau, ketThuc);
            if (!gia.ThanhCong) return KetQua<DatSan>.Loi(gia.ThongBao);

            var don = new DatSan
            {
                MaKH = maKH, MaSan = maSan, NgaySuDung = ngay, GioBatDau = batDau, GioKetThuc = ketThuc,
                TienSan = gia.DuLieu!.TienSan,
                TienCoc = thuCoc ? gia.DuLieu.TienCoc : 0,
                TrangThai = trangThai, NgayTao = _bayGio(), MaNVXacNhan = maNV
            };
            db.DatSans.Add(don);
            db.SaveChanges();

            // Kiểm tra lại trước khi commit (phòng trường hợp CSDL không hỗ trợ đủ mức cô lập)
            if (CoTrungLich(db, maSan, ngay, batDau, ketThuc, don.MaDatSan))
            {
                tx.Rollback();
                return KetQua<DatSan>.Loi("Khung giờ này vừa có người đặt. Vui lòng chọn giờ khác.");
            }
            tx.Commit();
            return KetQua<DatSan>.Ok(don, $"Đã tạo đơn #DS{don.MaDatSan}.");
        }
        catch (Exception ex) when (ex is DbUpdateException || ex.GetType().Name == "SqlException")
        {
            // Deadlock hoặc xung đột giữa hai transaction đặt cùng lúc
            return KetQua<DatSan>.Loi("Khung giờ này vừa có người đặt. Vui lòng chọn giờ khác.");
        }
    }

    // ---------- Chuyển trạng thái đơn ----------

    private KetQua DoiTrangThai(int maDon, string[] tuTrangThai, Action<DatSan> capNhat, string thongBao)
    {
        using var db = _dbf.CreateDbContext();
        var don = db.DatSans.FirstOrDefault(d => d.MaDatSan == maDon);
        if (don == null) return KetQua.Loi("Không tìm thấy đơn.");
        if (!tuTrangThai.Contains(don.TrangThai))
            return KetQua.Loi($"Đơn đang ở trạng thái \"{don.TrangThai}\", không thực hiện được thao tác này.");
        capNhat(don);
        db.SaveChanges();
        return KetQua.Ok(thongBao);
    }

    // UC17
    public KetQua XacNhan(int maDon, int maNV) =>
        DoiTrangThai(maDon, new[] { TrangThaiDon.ChoXacNhan },
            d => { d.TrangThai = TrangThaiDon.DaXacNhan; d.MaNVXacNhan = maNV; }, "Đã xác nhận đơn.");

    public KetQua TuChoi(int maDon, int maNV) =>
        DoiTrangThai(maDon, new[] { TrangThaiDon.ChoXacNhan },
            d => { d.TrangThai = TrangThaiDon.DaHuy; d.MaNVXacNhan = maNV; }, "Đã từ chối đơn.");

    // UC18
    public KetQua CheckIn(int maDon) =>
        DoiTrangThai(maDon, new[] { TrangThaiDon.DaXacNhan },
            d => d.TrangThai = TrangThaiDon.DangChoi, "Đã check-in, khách đang chơi.");

    // UC19: hủy tại quầy, không bị giới hạn 3 giờ
    public KetQua HuyTaiQuay(int maDon) =>
        DoiTrangThai(maDon, new[] { TrangThaiDon.ChoXacNhan, TrangThaiDon.DaXacNhan },
            d => d.TrangThai = TrangThaiDon.DaHuy, "Đã hủy đơn.");

    // UC11, BR07: khách chỉ tự hủy đơn của mình khi còn hơn 3 giờ trước giờ chơi
    public bool KhachDuocHuy(DatSan don) =>
        (don.TrangThai == TrangThaiDon.ChoXacNhan || don.TrangThai == TrangThaiDon.DaXacNhan)
        && (don.ThoiDiemBatDau - _bayGio()).TotalHours > QuyTac.SoGioHuyToiThieu;

    public KetQua HuyBoiKhach(int maDon, int maKH)
    {
        using var db = _dbf.CreateDbContext();
        var don = db.DatSans.FirstOrDefault(d => d.MaDatSan == maDon && d.MaKH == maKH);
        if (don == null) return KetQua.Loi("Không tìm thấy đơn.");
        if (!KhachDuocHuy(don))
            return KetQua.Loi("Chỉ hủy được đơn chưa chơi và còn hơn 3 giờ trước giờ bắt đầu. Vui lòng gọi sân để được hỗ trợ.");
        don.TrangThai = TrangThaiDon.DaHuy;
        db.SaveChanges();
        return KetQua.Ok("Đã hủy đơn.");
    }
}
