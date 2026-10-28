using SportZone.Business.Services;

namespace SportZone.Desktop;

// UC13 Đổi mật khẩu
public class FrmDoiMatKhau : Form
{
    private readonly TextBox _cu = new() { Left = 160, Top = 50, Width = 180, UseSystemPasswordChar = true, Font = Ui.FontThuong };
    private readonly TextBox _moi = new() { Left = 160, Top = 84, Width = 180, UseSystemPasswordChar = true, Font = Ui.FontThuong };
    private readonly TextBox _nhapLai = new() { Left = 160, Top = 118, Width = 180, UseSystemPasswordChar = true, Font = Ui.FontThuong };

    public FrmDoiMatKhau()
    {
        Text = "Đổi mật khẩu";
        ClientSize = new Size(380, 210);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        BackColor = Color.White;

        var nut = Ui.Nut("Đổi mật khẩu", 160, 160, 180, chinh: true);
        nut.Click += (_, _) => Doi();
        Controls.AddRange(new Control[]
        {
            Ui.TieuDe("Đổi mật khẩu"),
            Ui.Nhan("Mật khẩu hiện tại", 20, 50, 140), _cu,
            Ui.Nhan("Mật khẩu mới", 20, 84, 140), _moi,
            Ui.Nhan("Nhập lại mật khẩu", 20, 118, 140), _nhapLai,
            nut
        });
        AcceptButton = nut;
    }

    private void Doi()
    {
        if (_moi.Text != _nhapLai.Text) { Ui.CanhBao("Mật khẩu nhập lại không khớp."); return; }
        var kq = Program.Lay<TaiKhoanService>().DoiMatKhauNhanVien(PhienDangNhap.MaNV, _cu.Text, _moi.Text);
        kq.HienKetQua();
        if (kq.ThanhCong) Close();
    }
}
