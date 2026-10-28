using SportZone.Business;

namespace SportZone.Tests;

// UC33, BR03: giá tính theo từng block 30 phút nên một đơn vắt qua giờ vàng phải cộng dồn hai mức giá.
public class GiaTests
{
    [Fact]
    public void Gio_thuong_tinh_theo_don_gia_thuong()
    {
        using var csdl = new CsdlThu();
        var bg = csdl.DichVuGia().TinhTien(csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 30));
        Assert.True(bg.ThanhCong);
        Assert.Equal(450_000m, bg.DuLieu!.TienSan);   // 1,5 giờ × 300.000
        Assert.False(bg.DuLieu.LaCuoiTuan);
    }

    [Fact]
    public void Don_vat_qua_gio_vang_cong_don_hai_muc_gia()
    {
        using var csdl = new CsdlThu();
        var bg = csdl.DichVuGia().TinhTien(csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(16, 0), new(18, 0));
        // 1 giờ thường (300.000) + 1 giờ vàng (450.000)
        Assert.Equal(750_000m, bg.DuLieu!.TienSan);
        Assert.Equal(2, bg.DuLieu.ChiTiet.Count);
    }

    [Fact]
    public void Cuoi_tuan_dung_bang_gia_cuoi_tuan()
    {
        using var csdl = new CsdlThu();
        var bg = csdl.DichVuGia().TinhTien(csdl.MaSanBongDa5, CsdlThu.NgayCuoiTuan, new(18, 0), new(19, 0));
        Assert.Equal(500_000m, bg.DuLieu!.TienSan);
        Assert.True(bg.DuLieu.LaCuoiTuan);
    }

    [Fact]
    public void Tien_coc_la_30_phan_tram_lam_tron_nghin()
    {
        Assert.Equal(135_000m, QuyTac.TinhCoc(450_000m));
        Assert.Equal(54_000m, QuyTac.TinhCoc(180_000m));
    }

    [Theory]
    [InlineData(4, 0, 5, 0, "Sân chỉ mở cửa")]        // trước giờ mở cửa
    [InlineData(22, 0, 23, 30, "Sân chỉ mở cửa")]     // sau giờ đóng cửa
    [InlineData(9, 15, 10, 15, "block 30 phút")]      // không đúng block
    [InlineData(9, 0, 9, 30, "tối thiểu 1 giờ")]      // ngắn hơn 1 giờ
    public void Khung_gio_khong_hop_le_bi_tu_choi(int bdGio, int bdPhut, int ktGio, int ktPhut, string mongDoi)
    {
        var loi = QuyTac.KiemTraKhungGio(CsdlThu.NgayThuong, new(bdGio, bdPhut), new(ktGio, ktPhut), CsdlThu.BayGio);
        Assert.NotNull(loi);
        Assert.Contains(mongDoi, loi);
    }

    [Fact]
    public void Khong_dat_duoc_gio_da_qua()
    {
        var homNay = DateOnly.FromDateTime(CsdlThu.BayGio);
        var loi = QuyTac.KiemTraKhungGio(homNay, new(7, 0), new(8, 0), CsdlThu.BayGio);
        Assert.Equal("Giờ bắt đầu đã qua.", loi);
    }
}
