using SportZone.Business;
using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC14 Xem lịch sân dạng lưới: mỗi dòng là một sân, mỗi cột là một block 30 phút.
// Ô được tô màu theo trạng thái đơn; nhấp đúp vào ô có đơn để mở chi tiết (UC17–UC21).
public class UcLichSan : UserControl
{
    private readonly DateTimePicker _ngay = Ui.ChonNgay(70, 12);
    private readonly ComboBox _loai = new() { Left = 260, Top = 12, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _luoi = Ui.LuoiChiDoc();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30_000 };
    private List<San> _sans = new();
    private List<DatSan> _dons = new();

    public event Action? DonThayDoi;

    public UcLichSan()
    {
        var thanh = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        var nutHomNay = Ui.Nut("Hôm nay", 430, 11, 80);
        var nutTaiLai = Ui.Nut("Tải lại", 516, 11, 80);
        var chuDan = new Label
        {
            Text = "■ Chờ xác nhận   ■ Đã xác nhận   ■ Đang chơi   ■ Hoàn thành",
            Left = 610, Top = 17, AutoSize = true, ForeColor = Color.Gray
        };
        thanh.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Ngày", 12, 12, 55), _ngay, Ui.Nhan("Loại sân", 200, 12, 60), _loai,
            nutHomNay, nutTaiLai, chuDan
        });

        _luoi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _luoi.CellDoubleClick += (_, e) => MoDon(e.RowIndex, e.ColumnIndex);
        _luoi.CellMouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0) MoMenuChuot(e.RowIndex, e.ColumnIndex);
        };

        Controls.Add(_luoi);
        Controls.Add(thanh);

        _ngay.ValueChanged += (_, _) => TaiLai();
        _loai.SelectedIndexChanged += (_, _) => TaiLai();
        nutHomNay.Click += (_, _) => _ngay.Value = DateTime.Today;
        nutTaiLai.Click += (_, _) => TaiLai();
        _timer.Tick += (_, _) => TaiLai();   // tự làm mới mỗi 30 giây
        _timer.Start();

        NapLoaiSan();
        TaiLai();
    }

    private void NapLoaiSan()
    {
        _loai.Items.Add("Tất cả");
        foreach (var l in Program.Lay<DanhMucService>().DanhSachLoaiSan()) _loai.Items.Add(l);
        _loai.FormattingEnabled = true;
        _loai.Format += (_, e) => { if (e.ListItem is LoaiSan l) e.Value = l.TenLoai; };
        _loai.SelectedIndex = 0;
    }

    public void TaiLai()
    {
        var maLoai = _loai.SelectedItem is LoaiSan l ? l.MaLoai : (int?)null;
        _sans = Program.Lay<DanhMucService>().DanhSachSan(maLoai);
        _dons = Program.Lay<DatSanService>().DonTrongNgay(_ngay.NgayDangChon());
        VeLuoi();
    }

    private void VeLuoi()
    {
        var blocks = QuyTac.CacBlockTrongNgay().ToList();
        _luoi.SuspendLayout();
        _luoi.Rows.Clear();
        _luoi.Columns.Clear();
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sân", Width = 130, Frozen = true });
        foreach (var t in blocks)
            _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = t.Gio(), Width = 52 });

        foreach (var san in _sans)
        {
            var i = _luoi.Rows.Add();
            var dong = _luoi.Rows[i];
            dong.Tag = san;
            dong.Cells[0].Value = san.TenSan;
            dong.Cells[0].Style.BackColor = san.TrangThai == TrangThaiSan.BaoTri ? Color.FromArgb(233, 236, 239) : Color.White;
            if (san.TrangThai == TrangThaiSan.BaoTri) dong.Cells[0].ToolTipText = "Sân đang bảo trì";

            for (var c = 0; c < blocks.Count; c++)
            {
                var o = dong.Cells[c + 1];
                var don = _dons.FirstOrDefault(d => d.MaSan == san.MaSan && blocks[c] >= d.GioBatDau && blocks[c] < d.GioKetThuc);
                if (don == null)
                {
                    if (san.TrangThai == TrangThaiSan.BaoTri) { o.Style.BackColor = Color.FromArgb(248, 249, 250); o.Value = "BT"; }
                    continue;
                }
                o.Tag = don;
                o.Style.BackColor = Ui.MauTrangThai(don.TrangThai);
                o.ToolTipText = $"#DS{don.MaDatSan} · {don.KhachHang?.HoTen} · {don.GioBatDau.Gio()}–{don.GioKetThuc.Gio()} · {don.TrangThai}";
                if (blocks[c] == don.GioBatDau) o.Value = don.KhachHang?.HoTen?.Split(' ').Last();
            }
        }
        _luoi.ResumeLayout();
    }

    private DatSan? DonTaiO(int dong, int cot) =>
        dong >= 0 && cot >= 0 ? _luoi.Rows[dong].Cells[cot].Tag as DatSan : null;

    private void MoDon(int dong, int cot)
    {
        var don = DonTaiO(dong, cot);
        if (don == null) return;
        using var f = new FrmChiTietDon(don.MaDatSan);
        f.ShowDialog(this);
        TaiLai();
        DonThayDoi?.Invoke();
    }

    private void MoMenuChuot(int dong, int cot)
    {
        var menu = new ContextMenuStrip { Font = Ui.FontThuong };
        var don = DonTaiO(dong, cot);
        if (don != null)
            menu.Items.Add($"Chi tiết đơn #DS{don.MaDatSan}", null, (_, _) => MoDon(dong, cot));
        else if (_luoi.Rows[dong].Tag is San san)
        {
            var gio = TimeOnly.Parse(_luoi.Columns[cot == 0 ? 1 : cot].HeaderText);
            menu.Items.Add($"Đặt {san.TenSan} lúc {gio.Gio()}", null, (_, _) =>
            {
                using var f = new FrmDatTaiQuay(san.MaSan, _ngay.NgayDangChon(), gio);
                f.ShowDialog(this);
                TaiLai();
                DonThayDoi?.Invoke();
            });
        }
        menu.Show(_luoi, _luoi.PointToClient(Cursor.Position));
    }
}
