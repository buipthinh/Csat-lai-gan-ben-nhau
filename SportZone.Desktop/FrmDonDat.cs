using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC16 Tra cứu đơn đặt sân theo trạng thái, khoảng ngày và từ khóa (tên hoặc SĐT khách)
public class FrmDonDat : Form
{
    private readonly ComboBox _trangThai = new() { Left = 80, Top = 12, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.FontThuong };
    private readonly DateTimePicker _tuNgay = Ui.ChonNgay(260, 12);
    private readonly DateTimePicker _denNgay = Ui.ChonNgay(430, 12);
    private readonly TextBox _tuKhoa = new() { Left = 640, Top = 12, Width = 170, Font = Ui.FontThuong };
    private readonly DataGridView _luoi = Ui.LuoiChiDoc();
    private const string MoiTrangThai = "Tất cả";

    public FrmDonDat()
    {
        Text = "Tra cứu đơn đặt sân";
        ClientSize = new Size(1000, 600);
        BackColor = Color.White;

        _trangThai.Items.Add(MoiTrangThai);
        foreach (var tt in TrangThaiDon.TatCa) _trangThai.Items.Add(tt);
        _trangThai.SelectedIndex = 0;
        _tuNgay.Value = DateTime.Today.AddDays(-7);
        _denNgay.Value = DateTime.Today.AddDays(7);

        var nutTim = Ui.Nut("Tìm", 820, 11, 80, chinh: true);
        nutTim.Click += (_, _) => TaiLai();
        var nutChiTiet = Ui.Nut("Chi tiết", 906, 11, 80);
        nutChiTiet.Click += (_, _) => MoChiTiet();

        var thanh = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        thanh.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Trạng thái", 12, 12, 70), _trangThai,
            Ui.Nhan("Từ ngày", 220, 12, 45), _tuNgay,
            Ui.Nhan("Đến ngày", 395, 12, 45), _denNgay,
            Ui.Nhan("Khách", 590, 12, 45), _tuKhoa,
            nutTim, nutChiTiet
        });

        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã đơn", FillWeight = 60 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khách hàng", FillWeight = 150 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Điện thoại", FillWeight = 90 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sân", FillWeight = 110 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày", FillWeight = 80 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giờ", FillWeight = 90 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tiền sân", FillWeight = 90, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 100 });
        _luoi.CellDoubleClick += (_, _) => MoChiTiet();

        Controls.Add(_luoi);
        Controls.Add(thanh);
        _tuKhoa.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) TaiLai(); };
        TaiLai();
    }

    private void TaiLai()
    {
        var tt = (string)_trangThai.SelectedItem!;
        var dons = Program.Lay<DatSanService>().TraCuu(
            tt == MoiTrangThai ? null : tt, _tuNgay.NgayDangChon(), _denNgay.NgayDangChon(), _tuKhoa.Text);
        _luoi.Rows.Clear();
        foreach (var d in dons)
        {
            var i = _luoi.Rows.Add($"#DS{d.MaDatSan}", d.KhachHang?.HoTen, d.KhachHang?.SDT, d.San?.TenSan,
                d.NgaySuDung.ToString("dd/MM/yyyy"), $"{d.GioBatDau.Gio()}–{d.GioKetThuc.Gio()}", d.TienSan.Vnd(), d.TrangThai);
            _luoi.Rows[i].Tag = d;
            _luoi.Rows[i].Cells[7].Style.BackColor = Ui.MauTrangThai(d.TrangThai);
        }
        Text = $"Tra cứu đơn đặt sân ({dons.Count} đơn)";
    }

    private void MoChiTiet()
    {
        if (_luoi.CurrentRow?.Tag is not DatSan d) return;
        using var f = new FrmChiTietDon(d.MaDatSan);
        f.ShowDialog(this);
        TaiLai();
    }
}
