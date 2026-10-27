using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business.Services;

public class TaiKhoanService
{
    private readonly IDbContextFactory<SportZoneDbContext> _dbf;
    public TaiKhoanService(IDbContextFactory<SportZoneDbContext> dbf) => _dbf = dbf;

    public static string BamMatKhau(string matKhau) => BCrypt.Net.BCrypt.HashPassword(matKhau);
    public static bool KiemTraMatKhau(string matKhau, string? hash) =>
        !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(matKhau, hash);

    private static string? KiemTraMatKhauMoi(string matKhau) =>
        matKhau.Length < 6 ? "Mật khẩu phải có ít nhất 6 ký tự." : null;

    // ---------- Khách hàng (Web) ----------

    // UC05. Nếu SĐT đã được nhân viên tạo tại quầy mà chưa có mật khẩu thì cho kích hoạt tài khoản đó.
    public KetQua<KhachHang> DangKy(string hoTen, string sdt, string? email, string matKhau)
    {
        hoTen = hoTen.Trim(); sdt = ChuanHoaSdt(sdt);
        if (hoTen.Length == 0) return KetQua<KhachHang>.Loi("Vui lòng nhập họ tên.");
        if (sdt.Length < 9) return KetQua<KhachHang>.Loi("Số điện thoại không hợp lệ.");
        if (KiemTraMatKhauMoi(matKhau) is { } loi) return KetQua<KhachHang>.Loi(loi);

        using var db = _dbf.CreateDbContext();
        var kh = db.KhachHangs.FirstOrDefault(k => k.SDT == sdt);
        if (kh != null && kh.MatKhauHash != null) return KetQua<KhachHang>.Loi("Số điện thoại này đã có tài khoản.");
        if (!string.IsNullOrWhiteSpace(email) && db.KhachHangs.Any(k => k.Email == email && k.SDT != sdt))
            return KetQua<KhachHang>.Loi("Email này đã được dùng.");

        kh ??= db.KhachHangs.Add(new KhachHang { SDT = sdt }).Entity;
        kh.HoTen = hoTen;
        kh.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        kh.MatKhauHash = BamMatKhau(matKhau);
        db.SaveChanges();
        return KetQua<KhachHang>.Ok(kh, "Đăng ký thành công.");
    }

    // UC06: đăng nhập bằng SĐT hoặc email
    public KetQua<KhachHang> DangNhapKhach(string sdtHoacEmail, string matKhau)
    {
        using var db = _dbf.CreateDbContext();
        var k = sdtHoacEmail.Trim();
        var sdt = ChuanHoaSdt(k);
        var kh = db.KhachHangs.AsNoTracking().FirstOrDefault(x => x.SDT == sdt || x.Email == k);
        if (kh == null || !KiemTraMatKhau(matKhau, kh.MatKhauHash))
            return KetQua<KhachHang>.Loi("Sai số điện thoại/email hoặc mật khẩu.");
        return KetQua<KhachHang>.Ok(kh);
    }

    public KhachHang? LayKhach(int maKH)
    {
        using var db = _dbf.CreateDbContext();
        return db.KhachHangs.AsNoTracking().FirstOrDefault(k => k.MaKH == maKH);
    }

    // UC07
    public KetQua CapNhatThongTin(int maKH, string hoTen, string? email)
    {
        using var db = _dbf.CreateDbContext();
        var kh = db.KhachHangs.FirstOrDefault(k => k.MaKH == maKH);
        if (kh == null) return KetQua.Loi("Không tìm thấy tài khoản.");
        if (string.IsNullOrWhiteSpace(hoTen)) return KetQua.Loi("Vui lòng nhập họ tên.");
        if (!string.IsNullOrWhiteSpace(email) && db.KhachHangs.Any(x => x.Email == email && x.MaKH != maKH))
            return KetQua.Loi("Email này đã được dùng.");
        kh.HoTen = hoTen.Trim();
        kh.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        db.SaveChanges();
        return KetQua.Ok("Đã lưu thông tin.");
    }

    public KetQua DoiMatKhauKhach(int maKH, string matKhauCu, string matKhauMoi)
    {
        using var db = _dbf.CreateDbContext();
        var kh = db.KhachHangs.FirstOrDefault(k => k.MaKH == maKH);
        if (kh == null || !KiemTraMatKhau(matKhauCu, kh.MatKhauHash)) return KetQua.Loi("Mật khẩu hiện tại không đúng.");
        if (KiemTraMatKhauMoi(matKhauMoi) is { } loi) return KetQua.Loi(loi);
        kh.MatKhauHash = BamMatKhau(matKhauMoi);
        db.SaveChanges();
        return KetQua.Ok("Đã đổi mật khẩu.");
    }

    // ---------- Nhân viên (Desktop) ----------

    // UC12
    public KetQua<NhanVien> DangNhapNhanVien(string tenDangNhap, string matKhau)
    {
        using var db = _dbf.CreateDbContext();
        var nv = db.NhanViens.AsNoTracking().Include(n => n.VaiTro).FirstOrDefault(n => n.TenDangNhap == tenDangNhap.Trim());
        if (nv == null || !KiemTraMatKhau(matKhau, nv.MatKhauHash))
            return KetQua<NhanVien>.Loi("Sai tên đăng nhập hoặc mật khẩu.");
        if (nv.TrangThai == TrangThaiTaiKhoan.DaKhoa) return KetQua<NhanVien>.Loi("Tài khoản đã bị khóa."); // BR10
        return KetQua<NhanVien>.Ok(nv);
    }

    // UC13
    public KetQua DoiMatKhauNhanVien(int maNV, string matKhauCu, string matKhauMoi)
    {
        using var db = _dbf.CreateDbContext();
        var nv = db.NhanViens.FirstOrDefault(n => n.MaNV == maNV);
        if (nv == null || !KiemTraMatKhau(matKhauCu, nv.MatKhauHash)) return KetQua.Loi("Mật khẩu hiện tại không đúng.");
        if (KiemTraMatKhauMoi(matKhauMoi) is { } loi) return KetQua.Loi(loi);
        nv.MatKhauHash = BamMatKhau(matKhauMoi);
        db.SaveChanges();
        return KetQua.Ok("Đã đổi mật khẩu.");
    }

    // UC28 Quản lý tài khoản nhân viên
    public List<NhanVien> DanhSachNhanVien()
    {
        using var db = _dbf.CreateDbContext();
        return db.NhanViens.AsNoTracking().Include(n => n.VaiTro).OrderBy(n => n.MaNV).ToList();
    }

    public List<VaiTro> DanhSachVaiTro()
    {
        using var db = _dbf.CreateDbContext();
        return db.VaiTros.AsNoTracking().OrderBy(v => v.MaVaiTro).ToList();
    }

    public KetQua LuuNhanVien(int? maNV, string hoTen, string tenDangNhap, int maVaiTro, string? matKhauMoi)
    {
        if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(tenDangNhap))
            return KetQua.Loi("Vui lòng nhập họ tên và tên đăng nhập.");
        using var db = _dbf.CreateDbContext();
        if (db.NhanViens.Any(n => n.TenDangNhap == tenDangNhap.Trim() && n.MaNV != (maNV ?? 0)))
            return KetQua.Loi("Tên đăng nhập đã tồn tại.");
        NhanVien nv;
        if (maNV == null)
        {
            if (string.IsNullOrEmpty(matKhauMoi)) return KetQua.Loi("Nhập mật khẩu ban đầu cho tài khoản mới.");
            nv = db.NhanViens.Add(new NhanVien()).Entity;
        }
        else nv = db.NhanViens.First(n => n.MaNV == maNV);
        if (!string.IsNullOrEmpty(matKhauMoi))
        {
            if (KiemTraMatKhauMoi(matKhauMoi) is { } loi) return KetQua.Loi(loi);
            nv.MatKhauHash = BamMatKhau(matKhauMoi);
        }
        nv.HoTen = hoTen.Trim(); nv.TenDangNhap = tenDangNhap.Trim(); nv.MaVaiTro = maVaiTro;
        db.SaveChanges();
        return KetQua.Ok("Đã lưu tài khoản.");
    }

    public KetQua KhoaMoNhanVien(int maNV, int maNVDangDung)
    {
        if (maNV == maNVDangDung) return KetQua.Loi("Không thể tự khóa tài khoản đang đăng nhập.");
        using var db = _dbf.CreateDbContext();
        var nv = db.NhanViens.FirstOrDefault(n => n.MaNV == maNV);
        if (nv == null) return KetQua.Loi("Không tìm thấy tài khoản.");
        nv.TrangThai = nv.TrangThai == TrangThaiTaiKhoan.DaKhoa ? TrangThaiTaiKhoan.HoatDong : TrangThaiTaiKhoan.DaKhoa;
        db.SaveChanges();
        return KetQua.Ok(nv.TrangThai == TrangThaiTaiKhoan.DaKhoa ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản.");
    }

    public static string ChuanHoaSdt(string sdt) => new string(sdt.Where(char.IsDigit).ToArray());
}
