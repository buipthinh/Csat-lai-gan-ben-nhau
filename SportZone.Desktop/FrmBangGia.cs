using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

//UC26 Quản lý bảng giá: xem giá mọi loại sân theo khung giờ, lọc theo loại, bấm đúp vào một dòng để sửa
public class FrmBangGia : Form {
    private const string TatCa = "Tất cả loại sân";
    private readonly ComboBox _loc = new() { Left = 90, Top = 12, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.FontThuong };
    private readonly DataGridView _luoi = Ui.LuoiChiDoc();
    private readonly Label _luuY = new() {
        Text = "* Lưu ý:\n"
                + "- Nhấn chuột 2 lần để sửa giá.\n"
                + "- Sau khi thay đổi nhấn Lưu.",
        Left = 600, Top = 14, Width = 800, Height = 80,
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 13)
    };

    public FrmBangGia() {
        Text = "Quản lý bảng giá";
        ClientSize = new Size(900, 520);
        BackColor = Color.White;

        //bộ lọc dòng đầu là Tất cả loại sân, sau đó là từng loại
        _loc.Items.Add(TatCa);
        foreach (var l in Program.Lay<DanhMucService>().DanhSachLoaiSan()) _loc.Items.Add(l);
        _loc.FormattingEnabled = true;
        _loc.Format += (_, e) => { if (e.ListItem is LoaiSan l) e.Value = l.TenLoai; };
        _loc.SelectedIndexChanged += (_, _) => TaiLai();

        var thanhLoc = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        thanhLoc.Controls.AddRange(new Control[] {
            Ui.Nhan("Loại sân", 0, 12, 80), _loc });

        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Loại sân", FillWeight = 140 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khung giờ", FillWeight = 120 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giờ", FillWeight = 100 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá ngày thường (đ/giờ)", FillWeight = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá cuối tuần (đ/giờ)", FillWeight = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _luoi.Cursor = Cursors.Default;
        _luoi.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) SuaDong(_luoi.Rows[e.RowIndex]); };

        //khung chứa có lề để lưới không dính sát mép
        var khung = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 8, 24, 16), BackColor = Color.White };
        khung.Controls.Add(_luoi);
        khung.Controls.Add(thanhLoc);
        Controls.Add(khung);

        _loc.SelectedIndex = 0;
    }

    private void TaiLai() {
        var dm = Program.Lay<DanhMucService>();
        var loais = _loc.SelectedItem is LoaiSan chon ? new List<LoaiSan> { chon } : dm.DanhSachLoaiSan();
        var khungGios = dm.DanhSachKhungGio();
        var gias = dm.TatCaBangGia();

        _luoi.Rows.Clear();
        foreach (var l in loais) {
            foreach (var kg in khungGios) {
                var dong = new DongGia(l, kg, LayGia(gias, l, kg, false), LayGia(gias, l, kg, true));
                var i = _luoi.Rows.Add(l.TenLoai, kg.TenKhungGio, $"{kg.GioBatDau.Gio()}–{kg.GioKetThuc.Gio()}",
                    dong.Thuong.ToString("#,##0", Ui.Vi), dong.CuoiTuan.ToString("#,##0", Ui.Vi));
                _luoi.Rows[i].Tag = dong;
            }
        }
        _luoi.ClearSelection();
    }

    private static decimal LayGia(List<BangGia> gias, LoaiSan l, KhungGio kg, bool cuoiTuan) =>
        gias.FirstOrDefault(g => g.MaLoai == l.MaLoai && g.MaKhungGio == kg.MaKhungGio && g.LaCuoiTuan == cuoiTuan)?.GiaMoiGio ?? 0;

    //mở cửa sổ sửa giá cho dòng được bấm đúp, lưu xong thì tải lại lưới
    private void SuaDong(DataGridViewRow r) {
        if (r.Tag is not DongGia d) return;
        using var f = new FrmSuaGia(d.Loai, d.Khung, d.Thuong, d.CuoiTuan);
        if (f.ShowDialog(TopLevelControl ?? this) == DialogResult.OK) TaiLai();
    }

    private sealed record DongGia(LoaiSan Loai, KhungGio Khung, decimal Thuong, decimal CuoiTuan);
}