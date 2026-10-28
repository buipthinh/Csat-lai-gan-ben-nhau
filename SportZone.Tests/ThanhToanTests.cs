using SportZone.Data.Entities;

namespace SportZone.Tests;

public class ThanhToanTests
{
    // Dựng một đơn đang chơi để thử dịch vụ và thanh toán
    private static int DonDangChoi(CsdlThu csdl)
    {
        var dv = csdl.DichVuDatSan();
        var don = dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        dv.XacNhan(don.MaDatSan, csdl.MaNhanVien());
        dv.CheckIn(don.MaDatSan);
        return don.MaDatSan;
    }

    private static int MaDichVu(CsdlThu csdl, string ten)
    {
        using var db = csdl.CreateDbContext();
        return db.DichVus.First(d => d.TenDV == ten).MaDV;
    }

    [Fact]
    public void Chi_them_dich_vu_cho_don_dang_choi()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var don = dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        var nuoc = MaDichVu(csdl, "Nước suối");

        var kq = csdl.DichVuThanhToan().CapNhatDichVu(don.MaDatSan, new Dictionary<int, int> { [nuoc] = 2 });
        Assert.False(kq.ThanhCong);   // đơn còn Chờ xác nhận

        dv.XacNhan(don.MaDatSan, csdl.MaNhanVien());
        dv.CheckIn(don.MaDatSan);
        Assert.True(csdl.DichVuThanhToan().CapNhatDichVu(don.MaDatSan, new Dictionary<int, int> { [nuoc] = 2 }).ThanhCong);
    }

    [Fact]
    public void Tam_tinh_gom_tien_san_cong_dich_vu_tru_coc()
    {
        using var csdl = new CsdlThu();
        var maDon = DonDangChoi(csdl);
        var tt = csdl.DichVuThanhToan();
        tt.CapNhatDichVu(maDon, new Dictionary<int, int> { [MaDichVu(csdl, "Nước suối")] = 4 });

        var tam = tt.TinhTamTinh(maDon)!;
        Assert.Equal(300_000m, tam.TienSan);
        Assert.Equal(40_000m, tam.TienDichVu);
        Assert.Equal(340_000m, tam.TongGiaTri);
        Assert.Equal(90_000m, tam.TienCoc);
        Assert.Equal(250_000m, tam.PhaiTra);     // khách trả thêm tại quầy
    }

    [Fact]
    public void Thanh_toan_lap_hoa_don_va_dong_don()
    {
        using var csdl = new CsdlThu();
        var maDon = DonDangChoi(csdl);
        var tt = csdl.DichVuThanhToan();
        tt.CapNhatDichVu(maDon, new Dictionary<int, int> { [MaDichVu(csdl, "Trà đá")] = 2 });

        var kq = tt.ThanhToan(maDon, csdl.MaNhanVien(), PhuongThucThanhToan.TienMat);
        Assert.True(kq.ThanhCong);
        Assert.Equal(310_000m, kq.DuLieu!.TongTien);                       // doanh thu = tiền sân + dịch vụ
        Assert.Equal(TrangThaiDon.HoanThanh, csdl.LayDon(maDon).TrangThai);

        Assert.False(tt.ThanhToan(maDon, csdl.MaNhanVien(), PhuongThucThanhToan.TienMat).ThanhCong); // không thanh toán hai lần
    }

    [Fact]
    public void Khong_thanh_toan_don_chua_check_in()
    {
        using var csdl = new CsdlThu();
        var don = csdl.DichVuDatSan().DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        Assert.False(csdl.DichVuThanhToan().ThanhToan(don.MaDatSan, csdl.MaNhanVien(), PhuongThucThanhToan.TienMat).ThanhCong);
    }
}
