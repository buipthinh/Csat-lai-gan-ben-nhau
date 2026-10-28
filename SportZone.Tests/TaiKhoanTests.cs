using SportZone.Data.Entities;

namespace SportZone.Tests;

public class TaiKhoanTests
{
    [Fact]
    public void Dang_ky_roi_dang_nhap_duoc_bang_sdt_va_email()
    {
        using var csdl = new CsdlThu();
        var tk = csdl.DichVuTaiKhoan();
        Assert.True(tk.DangKy("Nguyễn Văn Test", "0988777666", "test@example.com", "matkhau123").ThanhCong);
        Assert.True(tk.DangNhapKhach("0988777666", "matkhau123").ThanhCong);
        Assert.True(tk.DangNhapKhach("test@example.com", "matkhau123").ThanhCong);
        Assert.False(tk.DangNhapKhach("0988777666", "sai-mat-khau").ThanhCong);
    }

    [Fact]
    public void Khong_dang_ky_trung_so_dien_thoai_da_co_tai_khoan()
    {
        using var csdl = new CsdlThu();
        var tk = csdl.DichVuTaiKhoan();
        tk.DangKy("Khách A", "0988777666", null, "matkhau123");
        var kq = tk.DangKy("Khách B", "0988777666", null, "matkhau123");
        Assert.False(kq.ThanhCong);
        Assert.Contains("đã có tài khoản", kq.ThongBao);
    }

    [Fact]
    public void Khach_tao_tai_quay_co_the_kich_hoat_tai_khoan_web()
    {
        using var csdl = new CsdlThu();
        // "Võ Quốc Huy" trong dữ liệu mẫu chưa có mật khẩu
        var kq = csdl.DichVuTaiKhoan().DangKy("Võ Quốc Huy", "0923456789", null, "matkhau123");
        Assert.True(kq.ThanhCong);
        using var db = csdl.CreateDbContext();
        Assert.Equal(1, db.KhachHangs.Count(k => k.SDT == "0923456789"));   // không tạo thêm khách trùng
    }

    [Fact]
    public void Tai_khoan_nhan_vien_bi_khoa_khong_dang_nhap_duoc()   // BR10
    {
        using var csdl = new CsdlThu();
        var tk = csdl.DichVuTaiKhoan();
        Assert.True(tk.DangNhapNhanVien("nhanvien", "nv123456").ThanhCong);

        var quanLy = csdl.MaNhanVien("admin");
        Assert.True(tk.KhoaMoNhanVien(csdl.MaNhanVien(), quanLy).ThanhCong);
        var kq = tk.DangNhapNhanVien("nhanvien", "nv123456");
        Assert.False(kq.ThanhCong);
        Assert.Contains("khóa", kq.ThongBao);
    }

    [Fact]
    public void Khong_tu_khoa_tai_khoan_dang_dung()
    {
        using var csdl = new CsdlThu();
        var quanLy = csdl.MaNhanVien("admin");
        Assert.False(csdl.DichVuTaiKhoan().KhoaMoNhanVien(quanLy, quanLy).ThanhCong);
    }

    [Fact]
    public void Vai_tro_quan_ly_va_nhan_vien_duoc_phan_biet()
    {
        using var csdl = new CsdlThu();
        var tk = csdl.DichVuTaiKhoan();
        Assert.Equal(TenVaiTro.QuanLy, tk.DangNhapNhanVien("admin", "admin123").DuLieu!.VaiTro!.TenVaiTro);
        Assert.Equal(TenVaiTro.NhanVien, tk.DangNhapNhanVien("nhanvien", "nv123456").DuLieu!.VaiTro!.TenVaiTro);
    }
}
