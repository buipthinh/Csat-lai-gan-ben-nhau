using SportZone.Business.Services;

namespace SportZone.Desktop;

// UC29 thống kê doanh thu theo ngày. Biểu đồ cột vẽ bằng GDI+.
public class FrmThongKe : Form
{
    private readonly DateTimePicker _tuNgay = Ui.ChonNgay(70, 12);
    private readonly DateTimePicker _denNgay = Ui.ChonNgay(240, 12);
    private readonly Panel _bieuDo = new() { Dock = DockStyle.Fill, BackColor = Color.White };
    private readonly Label _tongKet = new() { Dock = DockStyle.Top, Height = 30, Font = Ui.FontTieuDe, Padding = new Padding(8, 4, 0, 0) };
    private List<DoanhThuMoc> _doanhThu = new();

    public FrmThongKe()
    {
        Text = "Thống kê doanh thu";
        ClientSize = new Size(1000, 620);
        BackColor = Color.White;

        _tuNgay.Value = DateTime.Today.AddDays(-13);
        _denNgay.Value = DateTime.Today;

        var nutXem = Ui.Nut("Xem thống kê", 430, 11, 130, chinh: true);
        nutXem.Click += (_, _) => TaiLai();
        var thanh = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        thanh.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Từ ngày", 12, 12, 55), _tuNgay, Ui.Nhan("Đến ngày", 180, 12, 55), _denNgay, nutXem
        });

        _bieuDo.Paint += (_, e) => VeBieuDoCot(e.Graphics, _bieuDo.ClientSize,
            _doanhThu.Select(d => (d.Nhan, (double)d.DoanhThu)).ToList(), Ui.Xanh, so => so.ToString("#,##0", Ui.Vi));

        // Thêm theo thứ tự: vùng Fill trước, các thanh Top sau để thanh chọn ngày nằm trên cùng
        Controls.Add(_bieuDo);
        Controls.Add(_tongKet);
        Controls.Add(thanh);
        TaiLai();
    }

    private void TaiLai()
    {
        var tk = Program.Lay<ThongKeService>();
        var tu = _tuNgay.NgayDangChon();
        var den = _denNgay.NgayDangChon();
        if (den < tu) { Ui.CanhBao("Đến ngày phải sau từ ngày."); return; }

        _doanhThu = tk.DoanhThuTheoNgay(tu, den);
        _tongKet.Text = $"Tổng doanh thu: {_doanhThu.Sum(d => d.DoanhThu).Vnd()} · {_doanhThu.Sum(d => d.SoHoaDon)} hóa đơn";
        _bieuDo.Invalidate();
    }

    // Biểu đồ cột đơn giản: không dùng thư viện ngoài, vẽ trực tiếp bằng Graphics
    private static void VeBieuDoCot(Graphics g, Size kichThuoc, List<(string Nhan, double GiaTri)> duLieu,
                                    Color mau, Func<double, string> dinhDang)
    {
        g.Clear(Color.White);
        if (duLieu.Count == 0) return;

        const int leTrai = 90, lePhai = 20, leTren = 20, leDuoi = 40;
        var rong = kichThuoc.Width - leTrai - lePhai;
        var cao = kichThuoc.Height - leTren - leDuoi;
        if (rong <= 0 || cao <= 0) return;

        var font = new Font("Segoe UI", 8.25f);
        var maxGiaTri = Math.Max(duLieu.Max(d => d.GiaTri), 1);
        // Làm tròn trục dọc lên mốc đẹp
        var buoc = Math.Pow(10, Math.Floor(Math.Log10(maxGiaTri)));
        var tran = Math.Ceiling(maxGiaTri / buoc) * buoc;

        using var butMo = new Pen(Color.FromArgb(230, 234, 232));
        for (var i = 0; i <= 4; i++)
        {
            var y = leTren + cao - (float)(cao * i / 4.0);
            g.DrawLine(butMo, leTrai, y, leTrai + rong, y);
            var nhan = dinhDang(tran * i / 4);
            g.DrawString(nhan, font, Brushes.Gray, leTrai - 6 - g.MeasureString(nhan, font).Width, y - 7);
        }
        g.DrawLine(Pens.Gray, leTrai, leTren + cao, leTrai + rong, leTren + cao);

        var rongCot = (float)rong / duLieu.Count;
        using var to = new SolidBrush(mau);
        for (var i = 0; i < duLieu.Count; i++)
        {
            var h = (float)(cao * duLieu[i].GiaTri / tran);
            var x = leTrai + i * rongCot;
            g.FillRectangle(to, x + rongCot * 0.15f, leTren + cao - h, rongCot * 0.7f, h);
            // Nhãn trục ngang: thưa dần nếu quá nhiều cột
            if (duLieu.Count <= 16 || i % (duLieu.Count / 12 + 1) == 0)
            {
                var w = g.MeasureString(duLieu[i].Nhan, font).Width;
                g.DrawString(duLieu[i].Nhan, font, Brushes.DimGray, x + rongCot / 2 - w / 2, leTren + cao + 6);
            }
        }
    }
}
