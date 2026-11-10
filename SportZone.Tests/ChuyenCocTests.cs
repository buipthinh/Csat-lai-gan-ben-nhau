using SportZone.Business;
using SportZone.Data.Entities;

namespace SportZone.Tests;

public class ChuyenCocTests
{
    [Fact]
    public void Ma_tinh_trung_voi_ma_QR_cua_ngan_hang()
    {
        // Chuỗi đọc từ mã QR thật do app MB Bank cấp cho tài khoản nhận cọc
        Assert.Equal("00020101021138540010A00000072701240006970422011003795409300208QRIBFTTA53037045802VN63044D12",
                     VietQR.TaoChuoi("970422", "0379540930"));
    }

    [Fact]
    public void Ma_chuyen_coc_co_so_tien_va_noi_dung_don()
    {
        var chuoi = VietQR.TaoChuoi("970422", "0379540930", 203000m, VietQR.NoiDungChuyenKhoan(789));
        Assert.StartsWith("000201010212", chuoi);         // mã dùng một lần
        Assert.Contains("5406203000", chuoi);             // số tiền 203.000
        Assert.Contains("62090805DS789", chuoi);          // nội dung DS789
        Assert.Matches("6304[0-9A-F]{4}$", chuoi);
    }

    [Fact]
    public void Xac_nhan_don_online_la_ghi_nhan_da_nhan_coc()
    {
        using var csdl = new CsdlThu();
        var dv = csdl.DichVuDatSan();
        var don = dv.DatSanOnline(csdl.MaKhachDauTien(), csdl.MaSanBongDa5, CsdlThu.NgayThuong, new(9, 0), new(10, 0)).DuLieu!;
        Assert.True(don.TienCoc > 0);

        var kq = dv.XacNhan(don.MaDatSan, csdl.MaNhanVien());
        Assert.True(kq.ThanhCong);
        Assert.Contains("Đã ghi nhận tiền cọc", kq.ThongBao);
        Assert.Contains($"#DS{don.MaDatSan}", kq.ThongBao);
        Assert.Equal(TrangThaiDon.DaXacNhan, dv.ChiTiet(don.MaDatSan)!.TrangThai);
    }
}
