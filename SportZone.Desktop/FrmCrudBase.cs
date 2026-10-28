namespace SportZone.Desktop;

// Khung chung cho các form danh mục: lưới bên trên, các ô nhập bên dưới, nút Thêm/Lưu/Xóa.
// Mỗi form con chỉ cần khai báo cột, ô nhập và cách lưu.
public abstract class FrmCrudBase<T> : Form where T : class
{
    protected readonly DataGridView Luoi = Ui.LuoiChiDoc();
    protected readonly Panel ONhap = new() { Dock = DockStyle.Bottom, Height = 150, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8) };
    protected T? DangChon;

    protected FrmCrudBase(string tieuDe)
    {
        Text = tieuDe;
        ClientSize = new Size(1000, 600);
        BackColor = Color.White;

        var thanh = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.White };
        var nutThem = Ui.Nut("Thêm mới", 12, 8, 100);
        var nutLuu = Ui.Nut("Lưu", 118, 8, 100, chinh: true);
        var nutXoa = Ui.Nut("Xóa", 224, 8, 100);
        nutThem.Click += (_, _) => { DangChon = null; Luoi.ClearSelection(); XoaONhap(); };
        nutLuu.Click += (_, _) => { if (Luu()) TaiLai(); };
        nutXoa.Click += (_, _) => { if (DangChon != null && Ui.HoiXacNhan("Xóa dòng đang chọn?") && Xoa()) { DangChon = null; XoaONhap(); TaiLai(); } };
        thanh.Controls.AddRange(new Control[] { nutThem, nutLuu, nutXoa });

        Luoi.SelectionChanged += (_, _) =>
        {
            if (Luoi.CurrentRow?.Tag is T item) { DangChon = item; DoVaoONhap(item); }
        };

        Controls.Add(Luoi);
        Controls.Add(ONhap);
        Controls.Add(thanh);
    }

    // Gọi ở cuối constructor của form con, sau khi đã tạo cột và ô nhập
    protected void KhoiDong()
    {
        TaoCot();
        TaiLai();
        XoaONhap();
    }

    protected abstract void TaoCot();
    protected abstract void TaiLai();
    protected abstract void DoVaoONhap(T item);
    protected abstract void XoaONhap();
    protected abstract bool Luu();
    protected virtual bool Xoa() => false;

    protected void ThemCot(string tieuDe, int rong)
        => Luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = tieuDe, FillWeight = rong });
}
