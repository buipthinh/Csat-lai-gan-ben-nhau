using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC28 Quản lý tài khoản nhân viên (chỉ quản lý thấy menu này)
public class FrmNhanVien : FrmCrudBase<NhanVien>
{
    private readonly TextBox _hoTen = new() { Left = 130, Top = 14, Width = 220, Font = Ui.FontThuong };
    private readonly TextBox _tenDangNhap = new() { Left = 130, Top = 48, Width = 160, Font = Ui.FontThuong };
    private readonly ComboBox _vaiTro = new() { Left = 130, Top = 82, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.FontThuong };
    private readonly TextBox _matKhau = new() { Left = 480, Top = 14, Width = 160, UseSystemPasswordChar = true, Font = Ui.FontThuong };

    public FrmNhanVien() : base("Quản lý tài khoản nhân viên")
    {
        foreach (var v in Program.Lay<TaiKhoanService>().DanhSachVaiTro()) _vaiTro.Items.Add(v);
        _vaiTro.FormattingEnabled = true;
        _vaiTro.Format += (_, e) => { if (e.ListItem is VaiTro v) e.Value = v.TenVaiTro; };

        var nutKhoa = Ui.Nut("Khóa / Mở khóa", 480, 48, 160);
        nutKhoa.Click += (_, _) =>
        {
            if (DangChon == null) { Ui.CanhBao("Chọn một tài khoản trước."); return; }
            var kq = Program.Lay<TaiKhoanService>().KhoaMoNhanVien(DangChon.MaNV, PhienDangNhap.MaNV);
            kq.HienKetQua();
            TaiLai();
        };

        ONhap.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Họ tên", 12, 14), _hoTen,
            Ui.Nhan("Tên đăng nhập", 12, 48), _tenDangNhap,
            Ui.Nhan("Vai trò", 12, 82), _vaiTro,
            Ui.Nhan("Mật khẩu mới", 370, 14, 105), _matKhau, nutKhoa,
            new Label { Text = "Để trống mật khẩu nếu không muốn đổi.", Left = 480, Top = 86, Width = 260, ForeColor = Color.Gray, Font = Ui.FontThuong }
        });
        KhoiDong();
    }

    protected override void TaoCot()
    {
        ThemCot("Mã NV", 60);
        ThemCot("Họ tên", 180);
        ThemCot("Tên đăng nhập", 120);
        ThemCot("Vai trò", 100);
        ThemCot("Trạng thái", 100);
    }

    protected override void TaiLai()
    {
        Luoi.Rows.Clear();
        foreach (var n in Program.Lay<TaiKhoanService>().DanhSachNhanVien())
        {
            var i = Luoi.Rows.Add(n.MaNV, n.HoTen, n.TenDangNhap, n.VaiTro?.TenVaiTro, n.TrangThai);
            Luoi.Rows[i].Tag = n;
            if (n.TrangThai == TrangThaiTaiKhoan.DaKhoa) Luoi.Rows[i].Cells[4].Style.BackColor = Color.FromArgb(248, 215, 218);
        }
    }

    protected override void DoVaoONhap(NhanVien n)
    {
        _hoTen.Text = n.HoTen;
        _tenDangNhap.Text = n.TenDangNhap;
        _vaiTro.SelectedItem = _vaiTro.Items.Cast<VaiTro>().FirstOrDefault(v => v.MaVaiTro == n.MaVaiTro);
        _matKhau.Clear();
    }

    protected override void XoaONhap()
    {
        _hoTen.Clear(); _tenDangNhap.Clear(); _matKhau.Clear();
        if (_vaiTro.Items.Count > 0) _vaiTro.SelectedIndex = _vaiTro.Items.Count - 1;
    }

    protected override bool Luu()
    {
        if (_vaiTro.SelectedItem is not VaiTro v) { Ui.CanhBao("Chọn vai trò."); return false; }
        var kq = Program.Lay<TaiKhoanService>().LuuNhanVien(DangChon?.MaNV, _hoTen.Text, _tenDangNhap.Text, v.MaVaiTro,
            string.IsNullOrEmpty(_matKhau.Text) ? null : _matKhau.Text);
        kq.HienKetQua();
        return kq.ThanhCong;
    }
}
