using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC26 Quản lý bảng giá: mỗi loại sân có giá theo từng khung giờ, tách ngày thường và cuối tuần (BR03)
public class FrmBangGia : Form
{
    private readonly ComboBox _loai = new() { Left = 80, Top = 12, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.FontThuong };
    private readonly DataGridView _luoi = Ui.LuoiChiDoc();
    private List<KhungGio> _khungGios = new();

    public FrmBangGia()
    {
        Text = "Quản lý bảng giá";
        ClientSize = new Size(800, 500);
        BackColor = Color.White;

        foreach (var l in Program.Lay<DanhMucService>().DanhSachLoaiSan()) _loai.Items.Add(l);
        _loai.FormattingEnabled = true;
        _loai.Format += (_, e) => { if (e.ListItem is LoaiSan l) e.Value = l.TenLoai; };
        _loai.SelectedIndexChanged += (_, _) => TaiLai();

        var thanh = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        thanh.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Loại sân", 12, 12, 65), _loai,
            new Label { Text = "Sửa trực tiếp giá trong lưới rồi bấm Lưu bảng giá.", Left = 300, Top = 16, AutoSize = true, ForeColor = Color.Gray, Font = Ui.FontThuong }
        });

        var thanhDuoi = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.White };
        var nutLuu = Ui.Nut("Lưu bảng giá", 12, 8, 130, chinh: true);
        nutLuu.Click += (_, _) => Luu();
        thanhDuoi.Controls.Add(nutLuu);

        _luoi.ReadOnly = false;
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khung giờ", ReadOnly = true, FillWeight = 160 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giờ", ReadOnly = true, FillWeight = 110 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá ngày thường (đ/giờ)", FillWeight = 150, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giá cuối tuần (đ/giờ)", FillWeight = 150, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });

        Controls.Add(_luoi);
        Controls.Add(thanh);
        Controls.Add(thanhDuoi);

        if (_loai.Items.Count > 0) _loai.SelectedIndex = 0;
    }

    private void TaiLai()
    {
        if (_loai.SelectedItem is not LoaiSan l) return;
        _khungGios = Program.Lay<DanhMucService>().DanhSachKhungGio();
        var gias = Program.Lay<DanhMucService>().BangGiaCuaLoai(l.MaLoai);
        _luoi.Rows.Clear();
        foreach (var kg in _khungGios)
        {
            var thuong = gias.FirstOrDefault(g => g.MaKhungGio == kg.MaKhungGio && !g.LaCuoiTuan)?.GiaMoiGio ?? 0;
            var cuoiTuan = gias.FirstOrDefault(g => g.MaKhungGio == kg.MaKhungGio && g.LaCuoiTuan)?.GiaMoiGio ?? 0;
            var i = _luoi.Rows.Add(kg.TenKhungGio, $"{kg.GioBatDau.Gio()}–{kg.GioKetThuc.Gio()}",
                thuong.ToString("#,##0", Ui.Vi), cuoiTuan.ToString("#,##0", Ui.Vi));
            _luoi.Rows[i].Tag = kg;
        }
    }

    private static bool DocGia(object? o, out decimal gia) =>
        decimal.TryParse(Convert.ToString(o)?.Replace(".", "").Replace(",", "").Trim(), out gia) && gia >= 0;

    private void Luu()
    {
        if (_loai.SelectedItem is not LoaiSan l) return;
        var dm = Program.Lay<DanhMucService>();
        foreach (DataGridViewRow r in _luoi.Rows)
        {
            if (r.Tag is not KhungGio kg) continue;
            foreach (var (cot, cuoiTuan) in new[] { (2, false), (3, true) })
            {
                if (!DocGia(r.Cells[cot].Value, out var gia))
                {
                    Ui.CanhBao($"Giá của khung “{kg.TenKhungGio}” không hợp lệ.");
                    return;
                }
                var kq = dm.LuuGia(l.MaLoai, kg.MaKhungGio, cuoiTuan, gia);
                if (!kq.ThanhCong) { kq.HienKetQua(); return; }
            }
        }
        KetQuaLuuThanhCong();
    }

    private void KetQuaLuuThanhCong()
    {
        MessageBox.Show("Đã lưu bảng giá.", "SportZone", MessageBoxButtons.OK, MessageBoxIcon.Information);
        TaiLai();
    }
}
