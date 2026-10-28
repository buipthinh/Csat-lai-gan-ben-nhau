using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC20 Thêm dịch vụ vào đơn đang chơi
public class FrmDichVuDon : Form
{
    private readonly int _maDon;
    private readonly DataGridView _luoi = Ui.LuoiChiDoc();
    private readonly Label _lblTong = new() { Dock = DockStyle.Bottom, Height = 28, Font = Ui.FontTieuDe, TextAlign = ContentAlignment.MiddleRight };
    private readonly List<DichVu> _dichVus;

    public FrmDichVuDon(int maDon)
    {
        _maDon = maDon;
        _dichVus = Program.Lay<DanhMucService>().DanhSachDichVu();
        Text = $"Dịch vụ của đơn #DS{maDon}";
        ClientSize = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        _luoi.ReadOnly = false;
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dịch vụ", ReadOnly = true, FillWeight = 180 });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đơn giá", ReadOnly = true, FillWeight = 90, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số lượng", FillWeight = 70, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thành tiền", ReadOnly = true, FillWeight = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });

        var don = Program.Lay<DatSanService>().ChiTiet(maDon)!;
        foreach (var dv in _dichVus)
        {
            var sl = don.ChiTietDichVus.FirstOrDefault(c => c.MaDV == dv.MaDV)?.SoLuong ?? 0;
            var i = _luoi.Rows.Add(dv.TenDV + $" ({dv.DonVi})", dv.DonGia.Vnd(), sl, (sl * dv.DonGia).Vnd());
            _luoi.Rows[i].Tag = dv;
        }
        _luoi.CellEndEdit += (_, e) => CapNhatDong(e.RowIndex);

        var thanh = new Panel { Dock = DockStyle.Bottom, Height = 46 };
        var nutLuu = Ui.Nut("Lưu dịch vụ", 290, 8, 120, chinh: true);
        nutLuu.Click += (_, _) => Luu();
        var nutDong = Ui.Nut("Đóng", 418, 8, 80);
        nutDong.Click += (_, _) => Close();
        thanh.Controls.AddRange(new Control[] { nutLuu, nutDong });

        Controls.Add(_luoi);
        Controls.Add(_lblTong);
        Controls.Add(thanh);
        TinhTong();
    }

    private void CapNhatDong(int dong)
    {
        if (dong < 0 || _luoi.Rows[dong].Tag is not DichVu dv) return;
        var sl = int.TryParse(Convert.ToString(_luoi.Rows[dong].Cells[2].Value), out var n) && n >= 0 ? n : 0;
        _luoi.Rows[dong].Cells[2].Value = sl;
        _luoi.Rows[dong].Cells[3].Value = (sl * dv.DonGia).Vnd();
        TinhTong();
    }

    private Dictionary<int, int> SoLuongDangNhap() =>
        _luoi.Rows.Cast<DataGridViewRow>()
            .Where(r => r.Tag is DichVu)
            .ToDictionary(r => ((DichVu)r.Tag!).MaDV,
                          r => int.TryParse(Convert.ToString(r.Cells[2].Value), out var n) ? n : 0);

    private void TinhTong()
    {
        var tong = SoLuongDangNhap().Sum(kv => kv.Value * _dichVus.First(d => d.MaDV == kv.Key).DonGia);
        _lblTong.Text = "Tổng dịch vụ: " + tong.Vnd() + "   ";
    }

    private void Luu()
    {
        var kq = Program.Lay<ThanhToanService>().CapNhatDichVu(_maDon, SoLuongDangNhap());
        kq.HienKetQua();
        if (kq.ThanhCong) { DialogResult = DialogResult.OK; Close(); }
    }
}
