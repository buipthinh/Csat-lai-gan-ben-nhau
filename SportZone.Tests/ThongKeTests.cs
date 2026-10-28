using SportZone.Data.Entities;

namespace SportZone.Tests;

public class ThongKeTests
{
    [Fact]
    public void Doanh_thu_theo_ngay_cong_dung_cac_hoa_don()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var maNV = csdl.MaNhanVien();
        var don = dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        dv.XacNhan(don.MaDatSan, maNV);
        dv.CheckIn(don.MaDatSan);
        csdl.DichVuThanhToan().ThanhToan(don.MaDatSan, maNV, PhuongThucThanhToan.TienMat);

        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var moc = csdl.DichVuThongKe().DoanhThuTheoNgay(homNay, homNay);
        Assert.Single(moc);
        Assert.Equal(300_000m, moc[0].DoanhThu);
        Assert.Equal(1, moc[0].SoHoaDon);
    }
}
