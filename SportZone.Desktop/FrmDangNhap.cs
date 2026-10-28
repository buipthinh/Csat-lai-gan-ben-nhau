using SportZone.Business.Services;

namespace SportZone.Desktop;

// UC12 Đăng nhập hệ thống quản lý
public class FrmDangNhap : Form
{
    private readonly TextBox _txtTen = new() { Left = 130, Top = 70, Width = 200, Font = Ui.FontThuong };
    private readonly TextBox _txtMk = new() { Left = 130, Top = 105, Width = 200, UseSystemPasswordChar = true, Font = Ui.FontThuong };

    public FrmDangNhap()
    {
        Text = "SportZone · Đăng nhập";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(360, 200);
        BackColor = Color.White;

        var tieuDe = new Label
        {
            Text = "SportZone · Phần mềm quản lý", Font = Ui.FontTieuDe, ForeColor = Ui.XanhDam,
            Left = 20, Top = 20, AutoSize = true
        };
        var nutDangNhap = Ui.Nut("Đăng nhập", 130, 145, 200, chinh: true);
        nutDangNhap.Click += (_, _) => DangNhap();

        Controls.AddRange(new Control[]
        {
            tieuDe, Ui.Nhan("Tên đăng nhập", 20, 70), _txtTen,
            Ui.Nhan("Mật khẩu", 20, 105), _txtMk, nutDangNhap
        });
        AcceptButton = nutDangNhap;
        ActiveControl = _txtTen;
    }

    private void DangNhap()
    {
        var kq = Program.Lay<TaiKhoanService>().DangNhapNhanVien(_txtTen.Text, _txtMk.Text);
        if (!kq.ThanhCong) { Ui.CanhBao(kq.ThongBao); _txtMk.Clear(); _txtMk.Focus(); return; }
        PhienDangNhap.NhanVien = kq.DuLieu;
        DialogResult = DialogResult.OK;
    }
}
