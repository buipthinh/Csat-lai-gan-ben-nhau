using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC25 Quản lý khung giờ (không cho chồng giờ)
public class FrmKhungGio : FrmCrudBase<KhungGio>
{
    private readonly TextBox _ten = new() { Left = 130, Top = 14, Width = 200, Font = Ui.FontThuong };
    private readonly ComboBox _bd = Ui.ComboGio(130, 48, themGioDongCua: true);
    private readonly ComboBox _kt = Ui.ComboGio(300, 48, themGioDongCua: true);

    public FrmKhungGio() : base("Quản lý khung giờ")
    {
        ONhap.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Tên khung giờ", 12, 14), _ten,
            Ui.Nhan("Từ giờ", 12, 48), _bd, Ui.Nhan("Đến giờ", 240, 48, 55), _kt
        });
        KhoiDong();
    }

    protected override void TaoCot()
    {
        ThemCot("Mã", 50);
        ThemCot("Tên khung giờ", 180);
        ThemCot("Từ giờ", 80);
        ThemCot("Đến giờ", 80);
    }

    protected override void TaiLai()
    {
        Luoi.Rows.Clear();
        foreach (var k in Program.Lay<DanhMucService>().DanhSachKhungGio())
        {
            var i = Luoi.Rows.Add(k.MaKhungGio, k.TenKhungGio, k.GioBatDau.Gio(), k.GioKetThuc.Gio());
            Luoi.Rows[i].Tag = k;
        }
    }

    protected override void DoVaoONhap(KhungGio k)
    {
        _ten.Text = k.TenKhungGio;
        _bd.SelectedItem = k.GioBatDau;
        _kt.SelectedItem = k.GioKetThuc;
    }

    protected override void XoaONhap()
    {
        _ten.Clear();
        if (_bd.Items.Count > 0) { _bd.SelectedIndex = 0; _kt.SelectedIndex = 0; }
    }

    protected override bool Luu()
    {
        var kq = Program.Lay<DanhMucService>().LuuKhungGio(DangChon?.MaKhungGio, _ten.Text, _bd.GioDangChon(), _kt.GioDangChon());
        kq.HienKetQua();
        return kq.ThanhCong;
    }

    protected override bool Xoa()
    {
        var kq = Program.Lay<DanhMucService>().XoaKhungGio(DangChon!.MaKhungGio);
        kq.HienKetQua();
        return kq.ThanhCong;
    }
}
