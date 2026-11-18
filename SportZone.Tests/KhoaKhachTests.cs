using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Tests;

public class KhoaKhachTests
{
    [Fact]
    public void Khach_bi_khoa_khong_dang_nhap_va_khong_dat_san_duoc_mo_lai_thi_binh_thuong()
    {
        using var csdl = new CsdlThu();
        var tk = csdl.DichVuTaiKhoan();
        var kh = tk.DangKy("Khách Thử", "0988777666", null, "matkhau123").DuLieu!;
        var dvKhach = new KhachHangService(csdl);
        var dv = csdl.DichVuDatSan();

        var khoa = dvKhach.KhoaMoKhach(kh.MaKH);
        Assert.True(khoa.ThanhCong);
        Assert.Contains("Đã khóa", khoa.ThongBao);
        Assert.False(tk.DangNhapKhach("0988777666", "matkhau123").ThanhCong);
        Assert.False(dv.DatSanOnline(kh.MaKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).ThanhCong);

        Assert.Contains("Đã mở lại", dvKhach.KhoaMoKhach(kh.MaKH).ThongBao);
        Assert.Equal(TrangThaiTaiKhoan.HoatDong, tk.LayKhach(kh.MaKH)!.TrangThai);
        Assert.True(tk.DangNhapKhach("0988777666", "matkhau123").ThanhCong);
        Assert.True(dv.DatSanOnline(kh.MaKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).ThanhCong);
    }
}
