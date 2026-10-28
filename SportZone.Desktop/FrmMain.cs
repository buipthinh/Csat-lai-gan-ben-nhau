using SportZone.Business.Services;

namespace SportZone.Desktop;

// Cửa sổ chính: menu lọc theo vai trò (BR10), vùng làm việc mặc định là lịch sân (UC14)
public class FrmMain : Form
{
    private readonly Panel _vungLamViec = new() { Dock = DockStyle.Fill, BackColor = Color.White };
    private readonly StatusStrip _thanhTrangThai = new();
    private readonly ToolStripStatusLabel _lblNguoiDung = new();
    private readonly ToolStripStatusLabel _lblChoXacNhan = new() { Spring = true, TextAlign = ContentAlignment.MiddleRight };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30_000 };
    private UcLichSan? _lichSan;

    public FrmMain()
    {
        Text = $"SportZone · {PhienDangNhap.HoTen} ({PhienDangNhap.TenVaiTro})";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 700);
        Font = Ui.FontThuong;

        Controls.Add(_vungLamViec);
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
        var menu = new MenuStrip { Font = Ui.FontThuong };

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
        MainMenuStrip = menu;
        return menu;
    }

    private void MoLichSan()
    {
        _vungLamViec.Controls.Clear();
        _lichSan = new UcLichSan { Dock = DockStyle.Fill };
        _lichSan.DonThayDoi += CapNhatSoDonCho;
        _vungLamViec.Controls.Add(_lichSan);
    }

    private void Mo(Form f)
    {
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
}
