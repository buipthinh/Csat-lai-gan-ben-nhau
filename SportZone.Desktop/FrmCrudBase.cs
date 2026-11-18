namespace SportZone.Desktop;

// Khung chung cho các form danh mục: lưới bên trên, các ô nhập bên dưới, nút Thêm/Lưu/Xóa.
// Mỗi form con chỉ cần khai báo cột, ô nhập và cách lưu.
public abstract class FrmCrudBase<T> : Form where T : class
{
    protected readonly DataGridView Luoi = Ui.LuoiChiDoc();
    protected readonly Panel ONhap = new() { Dock = DockStyle.Bottom, Height = 150, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8) };
    protected T? DangChon;
    private readonly Label _lblCheDo = new() {
        Dock = DockStyle.Bottom, Height = 36, Font = Ui.FontTieuDe,
        Padding = new Padding(12, 8, 0, 0), BackColor = Color.White
    };
    private readonly string _tenDoiTuong;

    protected FrmCrudBase(string tieuDe)
    {
        Text = tieuDe;
        _tenDoiTuong = tieuDe.Replace("Quản lý ", "");
        ClientSize = new Size(1000, 600);
        BackColor = Color.White;
        Padding = new Padding(24, 8, 24, 16);

        var thanh = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.White };
        var nutThem = Ui.Nut("Thêm mới", 12, 8, 100);
        var nutLuu = Ui.Nut("Lưu", 118, 8, 100, chinh: true);
        var nutXoa = Ui.Nut("Xóa", 224, 8, 100);
        nutThem.Click += (_, _) => { DangChon = null; Luoi.ClearSelection(); XoaONhap(); CapNhatCheDo(); };
        nutLuu.Click += (_, _) => { if (Luu()) { TaiLai(); Luoi.ClearSelection(); DangChon = null; XoaONhap(); CapNhatCheDo(); } };
        nutXoa.Click += (_, _) => { if (DangChon != null && Ui.HoiXacNhan("Xóa dòng đang chọn?") && Xoa()) { DangChon = null; XoaONhap(); CapNhatCheDo(); TaiLai(); } };
        thanh.Controls.AddRange(new Control[] { nutThem, nutLuu, nutXoa });

        Luoi.SelectionChanged += (_, _) => {
            //chỉ nhận khi thật sự có dòng được chọn (ClearSelection cũng bắn sự kiện này)
            if (Luoi.SelectedRows.Count > 0 && Luoi.CurrentRow?.Tag is T item) {
                DangChon = item;
                DoVaoONhap(item);
                CapNhatCheDo();
            }
        };

        Controls.Add(Luoi);
        Controls.Add(_lblCheDo);
        Controls.Add(ONhap);
        Controls.Add(thanh);
    }

    // Gọi ở cuối constructor của form con, sau khi đã tạo cột và ô nhập
    protected void KhoiDong()
    {
        TaoCot();
        TaiLai();
        Luoi.ClearSelection();
        DangChon = null;
        XoaONhap();
        CapNhatCheDo();
    }

    protected abstract void TaoCot();
    protected abstract void TaiLai();
    protected abstract void DoVaoONhap(T item);
    protected abstract void XoaONhap();
    protected abstract bool Luu();
    protected virtual bool Xoa() => false;

    protected void CapNhatCheDo() {
        if (DangChon == null) {
            _lblCheDo.Text = $"Thêm mới {_tenDoiTuong}";
            _lblCheDo.ForeColor = Ui.XanhDam;
        } else {
            _lblCheDo.Text = $"Sửa {_tenDoiTuong}: {LayMa(DangChon)}";
            _lblCheDo.ForeColor = Color.FromArgb(200, 100, 0);
        }
    }

    protected virtual object? LayMa(T item) =>
        typeof(T).GetProperties().FirstOrDefault(p => p.Name.StartsWith("Ma"))?.GetValue(item);

    protected void ThemCot(string tieuDe, int rong)
        => Luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = tieuDe, FillWeight = rong });
}
