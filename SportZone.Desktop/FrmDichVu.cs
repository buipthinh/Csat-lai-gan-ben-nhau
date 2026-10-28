using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC27 Quản lý dịch vụ
public class FrmDichVu : FrmCrudBase<DichVu>
{
    private readonly TextBox _ten = new() { Left = 130, Top = 14, Width = 220, Font = Ui.FontThuong };
    private readonly TextBox _donVi = new() { Left = 130, Top = 48, Width = 100, Font = Ui.FontThuong };
    private readonly NumericUpDown _donGia = new()
    {
        Left = 130, Top = 82, Width = 140, Maximum = 10_000_000, Increment = 1000,
        ThousandsSeparator = true, Font = Ui.FontThuong
    };

    public FrmDichVu() : base("Quản lý dịch vụ")
    {
        ONhap.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Tên dịch vụ", 12, 14), _ten,
            Ui.Nhan("Đơn vị", 12, 48), _donVi,
            Ui.Nhan("Đơn giá", 12, 82), _donGia
        });
        KhoiDong();
    }

    protected override void TaoCot()
    {
        ThemCot("Mã", 50);
        ThemCot("Tên dịch vụ", 200);
        ThemCot("Đơn vị", 80);
        ThemCot("Đơn giá", 100);
    }

    protected override void TaiLai()
    {
        Luoi.Rows.Clear();
        foreach (var d in Program.Lay<DanhMucService>().DanhSachDichVu())
        {
            var i = Luoi.Rows.Add(d.MaDV, d.TenDV, d.DonVi, d.DonGia.Vnd());
            Luoi.Rows[i].Tag = d;
        }
    }

    protected override void DoVaoONhap(DichVu d)
    {
        _ten.Text = d.TenDV; _donVi.Text = d.DonVi; _donGia.Value = d.DonGia;
    }

    protected override void XoaONhap() { _ten.Clear(); _donVi.Clear(); _donGia.Value = 0; }

    protected override bool Luu()
    {
        var kq = Program.Lay<DanhMucService>().LuuDichVu(DangChon?.MaDV, _ten.Text, _donVi.Text, _donGia.Value);
        kq.HienKetQua();
        return kq.ThanhCong;
    }

    protected override bool Xoa()
    {
        var kq = Program.Lay<DanhMucService>().XoaDichVu(DangChon!.MaDV);
        kq.HienKetQua();
        return kq.ThanhCong;
    }
}
