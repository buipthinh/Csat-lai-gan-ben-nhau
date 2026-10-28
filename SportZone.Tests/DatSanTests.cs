using SportZone.Data.Entities;

namespace SportZone.Tests;

public class DatSanTests
{
    [Fact]
    public void Dat_san_online_tao_don_cho_xac_nhan_va_thu_coc()
    {
        using var csdl = new CsdlThu();
        var kq = csdl.DichVuDatSan().DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0));
        Assert.True(kq.ThanhCong);
        Assert.Equal(TrangThaiDon.ChoXacNhan, kq.DuLieu!.TrangThai);
        Assert.Equal(300_000m, kq.DuLieu.TienSan);
        Assert.Equal(90_000m, kq.DuLieu.TienCoc);     // BR09: cọc 30%
    }

    [Fact]
    public void Dat_tai_quay_xac_nhan_ngay_va_khong_thu_coc()
    {
        using var csdl = new CsdlThu();
        var kq = csdl.DichVuDatSan().DatTaiQuay(csdl.MaKhachDauTien(), csdl.MaSanCauLong, CsdlThu.NgayThuong, new(9, 0), new(10, 0), csdl.MaNhanVien());
        Assert.Equal(TrangThaiDon.DaXacNhan, kq.DuLieu!.TrangThai);
        Assert.Equal(0m, kq.DuLieu.TienCoc);
    }

    // BR05, BR06, UC32
    [Theory]
    [InlineData(9, 0, 10, 0)]     // trùng khít
    [InlineData(9, 30, 10, 30)]   // chồng một phần cuối
    [InlineData(8, 30, 9, 30)]    // chồng một phần đầu
    [InlineData(8, 0, 11, 0)]     // bao trùm
    public void Khong_dat_duoc_khung_gio_bi_chong(int bdGio, int bdPhut, int ktGio, int ktPhut)
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var maKH = csdl.MaKhachDauTien();
        Assert.True(dv.DatSanOnline(maKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).ThanhCong);

        var kq = dv.DatSanOnline(maKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(bdGio, bdPhut), new(ktGio, ktPhut));
        Assert.False(kq.ThanhCong);
        Assert.Contains("đã có người đặt", kq.ThongBao);
    }

    [Fact]
    public void Dat_duoc_khung_gio_lien_ke_khong_chong()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var maKH = csdl.MaKhachDauTien();
        dv.DatSanOnline(maKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0));
        Assert.True(dv.DatSanOnline(maKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(10, 0), new(11, 0)).ThanhCong);
    }

    [Fact]
    public void Khong_dat_duoc_san_dang_bao_tri()   // BR08
    {
        using var csdl = new CsdlThu();
        var kq = csdl.DichVuDatSan().DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBaoTri, CsdlThu.NgayThuong, new(9, 0), new(10, 0));
        Assert.False(kq.ThanhCong);
        Assert.Contains("bảo trì", kq.ThongBao);
    }

    [Fact]
    public void Quy_trinh_xac_nhan_check_in_chay_dung_thu_tu()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var maNV = csdl.MaNhanVien();
        var don = dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;

        Assert.False(dv.CheckIn(don.MaDatSan).ThanhCong);          // chưa xác nhận thì chưa check-in được
        Assert.True(dv.XacNhan(don.MaDatSan, maNV).ThanhCong);
        Assert.False(dv.XacNhan(don.MaDatSan, maNV).ThanhCong);    // không xác nhận hai lần
        Assert.True(dv.CheckIn(don.MaDatSan).ThanhCong);
        Assert.Equal(TrangThaiDon.DangChoi, csdl.LayDon(don.MaDatSan).TrangThai);
    }

    // BR07: khách chỉ tự hủy khi còn hơn 3 giờ trước giờ chơi
    [Fact]
    public void Khach_huy_duoc_khi_con_hon_3_gio()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var maKH = csdl.MaKhachDauTien();
        var don = dv.DatSanOnline(maKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        Assert.True(dv.HuyBoiKhach(don.MaDatSan, maKH).ThanhCong);
        Assert.Equal(TrangThaiDon.DaHuy, csdl.LayDon(don.MaDatSan).TrangThai);
    }

    [Fact]
    public void Khach_khong_huy_duoc_khi_con_duoi_3_gio()
    {
        using var csdl = new CsdlThu();
        var maKH = csdl.MaKhachDauTien();
        // Tạo đơn lúc 7:00 hôm trước, rồi coi "bây giờ" là 2 giờ trước giờ chơi
        var dvSom = new SportZone.Business.Services.DatSanService(csdl, () => CsdlThu.NgayThuong.ToDateTime(new(6, 0)));
        var don = dvSom.DatSanOnline(maKH, csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;

        var dvSat = new SportZone.Business.Services.DatSanService(csdl, () => CsdlThu.NgayThuong.ToDateTime(new(7, 30)));
        var kq = dvSat.HuyBoiKhach(don.MaDatSan, maKH);
        Assert.False(kq.ThanhCong);
        Assert.Contains("3 giờ", kq.ThongBao);
    }

    [Fact]
    public void Khach_khong_huy_duoc_don_cua_nguoi_khac()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var don = dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        Assert.False(dv.HuyBoiKhach(don.MaDatSan, csdl.MaKhachDauTien() + 1).ThanhCong);
    }

    [Fact]
    public void Tim_san_trong_bo_san_da_co_don_va_san_bao_tri()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0));

        var trong = dv.TimSanTrong(null, CsdlThu.NgayThuong, new(9, 0), new(10, 0));
        Assert.DoesNotContain(csdl.MaSanBongDa5, trong.Select(s => s.MaSan));
        Assert.DoesNotContain(csdl.MaSanBaoTri, trong.Select(s => s.MaSan));
        Assert.Contains(csdl.MaSanCauLong, trong.Select(s => s.MaSan));
    }
}
