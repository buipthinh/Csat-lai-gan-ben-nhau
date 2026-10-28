using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC24 Quản lý sân, kèm chuyển sân sang Bảo trì (BR08)
public class FrmSan : FrmCrudBase<San>
{
    private readonly TextBox _ten = new() { Left = 130, Top = 14, Width = 200, Font = Ui.FontThuong };
    private readonly ComboBox _loai = new() { Left = 130, Top = 48, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.FontThuong };
    private readonly ComboBox _trangThai = new() { Left = 130, Top = 82, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.FontThuong };
    private readonly TextBox _moTa = new() { Left = 450, Top = 14, Width = 400, Height = 100, Multiline = true, Font = Ui.FontThuong };

    public FrmSan() : base("Quản lý sân")
    {
        foreach (var l in Program.Lay<DanhMucService>().DanhSachLoaiSan()) _loai.Items.Add(l);
        _loai.FormattingEnabled = true;
        _loai.Format += (_, e) => { if (e.ListItem is LoaiSan l) e.Value = l.TenLoai; };
        _trangThai.Items.AddRange(new object[] { TrangThaiSan.HoatDong, TrangThaiSan.BaoTri });

        var nutDoi = Ui.Nut("Đổi trạng thái bảo trì", 130, 116, 200);
        nutDoi.Click += (_, _) =>
        {
            if (DangChon == null) { Ui.CanhBao("Chọn một sân trước."); return; }
            var kq = Program.Lay<DanhMucService>().DoiTrangThaiSan(DangChon.MaSan);
            kq.HienKetQua();
            TaiLai();
        };

        ONhap.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Tên sân", 12, 14), _ten,
            Ui.Nhan("Loại sân", 12, 48), _loai,
            Ui.Nhan("Trạng thái", 12, 82), _trangThai,
            Ui.Nhan("Mô tả", 350, 14, 90), _moTa, nutDoi
        });
        KhoiDong();
    }

    protected override void TaoCot()
    {
        ThemCot("Mã sân", 60);
        ThemCot("Tên sân", 130);
        ThemCot("Loại sân", 130);
        ThemCot("Trạng thái", 100);
        ThemCot("Mô tả", 250);
    }

    protected override void TaiLai()
    {
        Luoi.Rows.Clear();
        foreach (var s in Program.Lay<DanhMucService>().DanhSachSan())
        {
            var i = Luoi.Rows.Add(s.MaSan, s.TenSan, s.LoaiSan?.TenLoai, s.TrangThai, s.MoTa);
            Luoi.Rows[i].Tag = s;
            if (s.TrangThai == TrangThaiSan.BaoTri) Luoi.Rows[i].Cells[3].Style.BackColor = Color.FromArgb(233, 236, 239);
        }
    }

    protected override void DoVaoONhap(San s)
    {
        _ten.Text = s.TenSan;
        _loai.SelectedItem = _loai.Items.Cast<LoaiSan>().FirstOrDefault(l => l.MaLoai == s.MaLoai);
        _trangThai.SelectedItem = s.TrangThai;
        _moTa.Text = s.MoTa;
    }

    protected override void XoaONhap()
    {
        _ten.Clear(); _moTa.Clear();
        if (_loai.Items.Count > 0) _loai.SelectedIndex = 0;
        _trangThai.SelectedItem = TrangThaiSan.HoatDong;
    }

    protected override bool Luu()
    {
        if (_loai.SelectedItem is not LoaiSan l) { Ui.CanhBao("Chọn loại sân."); return false; }
        var kq = Program.Lay<DanhMucService>().LuuSan(DangChon?.MaSan, _ten.Text, l.MaLoai, null, _moTa.Text,
            (string)(_trangThai.SelectedItem ?? TrangThaiSan.HoatDong));
        kq.HienKetQua();
        return kq.ThanhCong;
    }

    protected override bool Xoa()
    {
        var kq = Program.Lay<DanhMucService>().XoaSan(DangChon!.MaSan);
        kq.HienKetQua();
        return kq.ThanhCong;
    }
}
