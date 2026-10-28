using SportZone.Data.Entities;

namespace SportZone.Tests;

public class DanhMucTests
{
    [Fact]
    public void Khung_gio_moi_khong_duoc_chong_khung_gio_cu()
    {
        using var csdl = new CsdlThu();
        var dm = csdl.DichVuDanhMuc();
        var kq = dm.LuuKhungGio(null, "Giờ thử", new(16, 0), new(18, 0));   // chồng giờ vàng 17–21
        Assert.False(kq.ThanhCong);
        Assert.Contains("chồng", kq.ThongBao);
    }

    [Fact]
    public void Doi_trang_thai_san_qua_lai_giua_hoat_dong_va_bao_tri()
    {
        using var csdl = new CsdlThu();
        var dm = csdl.DichVuDanhMuc();
        var maSan = csdl.MaSanBongDa5;
        dm.DoiTrangThaiSan(maSan);
        Assert.Equal(TrangThaiSan.BaoTri, dm.LaySan(maSan)!.TrangThai);
        dm.DoiTrangThaiSan(maSan);
        Assert.Equal(TrangThaiSan.HoatDong, dm.LaySan(maSan)!.TrangThai);
    }

    [Fact]
    public void Khong_xoa_san_da_co_don_dat()
    {
        using var csdl = new CsdlThu();
        csdl.DichVuDatSan().DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0));
        var kq = csdl.DichVuDanhMuc().XoaSan(csdl.MaSanBongDa5);
        Assert.False(kq.ThanhCong);
        Assert.Contains("Bảo trì", kq.ThongBao);
    }

    [Fact]
    public void Doi_gia_dich_vu_khong_lam_sai_don_cu()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var maNV = csdl.MaNhanVien();
        var don = dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanCauLong, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        dv.XacNhan(don.MaDatSan, maNV);
        dv.CheckIn(don.MaDatSan);

        int maNuoc;
        using (var db = csdl.CreateDbContext()) maNuoc = db.DichVus.First(d => d.TenDV == "Nước suối").MaDV;
        csdl.DichVuThanhToan().CapNhatDichVu(don.MaDatSan, new Dictionary<int, int> { [maNuoc] = 2 });

        csdl.DichVuDanhMuc().LuuDichVu(maNuoc, "Nước suối", "chai", 20_000);   // tăng giá sau khi đã bán
        var tam = csdl.DichVuThanhToan().TinhTamTinh(don.MaDatSan)!;
        Assert.Equal(20_000m, tam.TienDichVu);   // vẫn tính theo giá lúc bán: 2 × 10.000
    }
}
