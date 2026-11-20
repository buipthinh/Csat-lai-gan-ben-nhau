using SportZone.Business.Services;

namespace SportZone.Desktop;

// Cửa sổ chính: menu dọc bên trái lọc theo vai trò (BR10), vùng làm việc mặc định là lịch sân (UC14)
public class FrmMain : Form
{
    private readonly Panel _vungLamViec = new() { Dock = DockStyle.Fill, BackColor = Color.White };
    private readonly StatusStrip _thanhTrangThai = new();
    private readonly ToolStripStatusLabel _lblNguoiDung = new();
    private readonly ToolStripStatusLabel _lblChoXacNhan = new() { Spring = true, TextAlign = ContentAlignment.MiddleRight };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30_000 };
    private UcLichSan? _lichSan;
    private readonly Panel _khung = new() { Dock = DockStyle.Fill };
    private readonly Label _tieuDe = new()
    {
        Dock = DockStyle.Top, Height = 80,
        Font = new Font("Segoe UI", 15, FontStyle.Bold),
        ForeColor = Color.FromArgb(30, 41, 59),
        BackColor = Color.White,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(16, 0, 0, 0)
    };

    private static readonly Color MauNen = Ui.XanhDam;
    private static readonly Color MauHover = Ui.Xanh;

    public FrmMain()
    {
        Text = $"SportZone · {PhienDangNhap.HoTen} ({PhienDangNhap.TenVaiTro})";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 700);
        Font = Ui.FontThuong;

        // Thứ tự Add: Fill trước, rồi Left, rồi Bottom
        _khung.Controls.Add(_vungLamViec);
        _khung.Controls.Add(_tieuDe);
        Controls.Add(_khung);
        Controls.Add(TaoMenu());
        _thanhTrangThai.Items.AddRange(new ToolStripItem[] { _lblNguoiDung, _lblChoXacNhan });
        Controls.Add(_thanhTrangThai);

        _lblNguoiDung.Text = $"Đăng nhập: {PhienDangNhap.HoTen}";
        _timer.Tick += (_, _) => CapNhatSoDonCho();
        _timer.Start();
        CapNhatSoDonCho();

        MoLichSan();
        
    }

    private MenuStrip TaoMenu()
    {
        var menu = new MenuStrip
        {
            Font = Ui.FontThuong,
            Dock = DockStyle.Left,
            LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow,
            AutoSize = false,
            Width = 190,
            BackColor = MauNen,
            Padding = new Padding(0, 12, 0, 0)
        };

        var mDatSan = new ToolStripMenuItem("&Đặt sân");
        mDatSan.DropDownItems.Add("Lịch sân (F5)", null, (_, _) => MoLichSan());
        mDatSan.DropDownItems.Add("Đặt sân tại quầy", null, (_, _) => MoDialog(new FrmDatTaiQuay()));
        mDatSan.DropDownItems.Add("Tra cứu đơn đặt sân", null, (_, _) => Mo(new FrmDonDat()));

        var mKhach = new ToolStripMenuItem("&Khách hàng");
        mKhach.DropDownItems.Add("Quản lý khách hàng", null, (_, _) => Mo(new FrmKhachHang()));

        var mDanhMuc = new ToolStripMenuItem("Danh &mục");
        mDanhMuc.DropDownItems.Add("Loại sân", null, (_, _) => Mo(new FrmLoaiSan()));
        mDanhMuc.DropDownItems.Add("Sân", null, (_, _) => Mo(new FrmSan()));
        mDanhMuc.DropDownItems.Add("Khung giờ", null, (_, _) => Mo(new FrmKhungGio()));
        mDanhMuc.DropDownItems.Add("Bảng giá", null, (_, _) => Mo(new FrmBangGia()));
        mDanhMuc.DropDownItems.Add("Dịch vụ", null, (_, _) => Mo(new FrmDichVu()));
        mDanhMuc.DropDownItems.Add("Tài khoản nhân viên", null, (_, _) => Mo(new FrmNhanVien()));

        var mThongKe = new ToolStripMenuItem("&Thống kê");
        mThongKe.DropDownItems.Add("Doanh thu", null, (_, _) => Mo(new FrmThongKe()));

        var mHeThong = new ToolStripMenuItem("&Hệ thống");
        mHeThong.DropDownItems.Add("Đổi mật khẩu", null, (_, _) => MoDialog(new FrmDoiMatKhau()));
        mHeThong.DropDownItems.Add("Thoát", null, (_, _) => Close());

        menu.Items.AddRange(new ToolStripItem[] { mDatSan, mKhach });
        // Danh mục, bảng giá, thống kê chỉ dành cho quản lý (UC23–UC29)
        if (PhienDangNhap.LaQuanLy) menu.Items.AddRange(new ToolStripItem[] { mDanhMuc, mThongKe });
        menu.Items.Add(mHeThong);

        foreach (ToolStripItem it in menu.Items)
        {
            if (it is not ToolStripMenuItem mi) continue;
            mi.AutoSize = false;
            mi.Size = new Size(188, 44);
            mi.TextAlign = ContentAlignment.MiddleLeft;
            mi.Padding = new Padding(10, 0, 0, 0);
            mi.ForeColor = Color.Black;
            mi.MouseEnter += (_, _) => { if (mi.HasDropDownItems && !mi.DropDown.Visible) mi.ShowDropDown(); };
            mi.MouseUp += (_, _) => { if (mi.HasDropDownItems) menu.BeginInvoke(() => mi.ShowDropDown()); };
            foreach (ToolStripItem con in mi.DropDownItems)
            {
                con.ForeColor = Color.Black;
            }
        }
        menu.Paint += (_, e) => e.Graphics.DrawLine(Pens.Gainsboro, menu.Width - 1, 0, menu.Width - 1, menu.Height);
        menu.Renderer = new MenuRenderer();

        //chữ trên thanh bar màu trắng, in đậm; menu con giữ chữ thường
        foreach (ToolStripItem item in menu.Items) {
            item.ForeColor = Color.White;
            item.Font = new Font(Ui.FontThuong, FontStyle.Bold);
            if (item is ToolStripMenuItem m) m.DropDown.Font = Ui.FontThuong;
        }
        MainMenuStrip = menu;
        return menu;
    }

    //Tạo màu cho thanh menu
    private class MauMenu : ProfessionalColorTable
    {
        public override Color MenuItemSelected => MauHover;
        public override Color MenuItemSelectedGradientBegin => MauHover;
        public override Color MenuItemSelectedGradientEnd => MauHover;
        public override Color MenuItemPressedGradientBegin => MauHover;
        public override Color MenuItemPressedGradientEnd => MauHover;
        public override Color MenuItemBorder => MauHover;
        public override Color MenuStripGradientBegin => MauNen;
        public override Color MenuStripGradientEnd => MauNen;
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
    }

    private void MoLichSan()
    {
        _tieuDe.Text = "Lịch sân";
        _vungLamViec.Controls.Clear();
        _lichSan = new UcLichSan { Dock = DockStyle.Fill };
        _lichSan.DonThayDoi += CapNhatSoDonCho;
        _vungLamViec.Controls.Add(_lichSan);
    }

    private void Mo(Form f)
    {
        _tieuDe.Text = f.Text;
        _vungLamViec.Controls.Clear();
        f.TopLevel = false;
        f.FormBorderStyle = FormBorderStyle.None;
        f.Dock = DockStyle.Fill;
        _vungLamViec.Controls.Add(f);
        f.Show();
    }

    private void MoDialog(Form f)
    {
        using (f) f.ShowDialog(this);
        _lichSan?.TaiLai();
        CapNhatSoDonCho();
    }

    private void CapNhatSoDonCho()
    {
        var so = Program.Lay<DatSanService>().SoDonChoXacNhan();
        _lblChoXacNhan.Text = so > 0 ? $"⏳ {so} đơn chờ xác nhận" : "Không có đơn chờ xác nhận";
        _lblChoXacNhan.ForeColor = so > 0 ? Color.FromArgb(166, 108, 0) : SystemColors.ControlText;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5) { MoLichSan(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private class MenuRenderer : ToolStripProfessionalRenderer {
        private static readonly Color XanhNhat = Color.FromArgb(76, 175, 120);
        private static readonly Color XamHover = Color.FromArgb(229, 229, 229);

        public MenuRenderer() : base(new MauMenu()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if (!e.Item.Selected && !e.Item.Pressed) {
                base.OnRenderMenuItemBackground(e);
                return;
            }
            using var co = new SolidBrush(e.Item.IsOnDropDown ? XamHover : XanhNhat);
            e.Graphics.FillRectangle(co, new Rectangle(Point.Empty, e.Item.Size));
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            e.TextColor = e.Item.IsOnDropDown ? Color.Black : Color.White;
            base.OnRenderItemText(e);
        }
    }
}
