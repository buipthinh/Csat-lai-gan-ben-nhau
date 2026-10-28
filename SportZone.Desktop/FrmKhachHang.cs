using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC22 Quản lý khách hàng, kèm xem lịch sử đặt sân của khách
public class FrmKhachHang : FrmCrudBase<KhachHang>
{
    private readonly TextBox _tuKhoa = new() { Left = 80, Top = 12, Width = 200, Font = Ui.FontThuong };
    private readonly TextBox _hoTen = new() { Left = 130, Top = 14, Width = 220, Font = Ui.FontThuong };
    private readonly TextBox _sdt = new() { Left = 130, Top = 48, Width = 140, Font = Ui.FontThuong };
    private readonly TextBox _email = new() { Left = 130, Top = 82, Width = 220, Font = Ui.FontThuong };

    public FrmKhachHang() : base("Quản lý khách hàng")
    {
        var thanh = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        var nutTim = Ui.Nut("Tìm", 290, 11, 80);
        nutTim.Click += (_, _) => TaiLai();
        thanh.Controls.AddRange(new Control[] { Ui.Nhan("Tìm khách", 12, 12, 70), _tuKhoa, nutTim });
        Controls.Add(thanh);
        _tuKhoa.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) TaiLai(); };

        var nutLichSu = Ui.Nut("Lịch sử đặt sân", 400, 14, 150);
        nutLichSu.Click += (_, _) => XemLichSu();

        ONhap.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Họ tên", 12, 14), _hoTen,
            Ui.Nhan("Số điện thoại", 12, 48), _sdt,
            Ui.Nhan("Email", 12, 82), _email, nutLichSu
        });
        KhoiDong();
    }

    protected override void TaoCot()
    {
        ThemCot("Mã KH", 60);
        ThemCot("Họ tên", 180);
        ThemCot("Số điện thoại", 110);
        ThemCot("Email", 180);
        ThemCot("Tài khoản web", 100);
        ThemCot("Số đơn", 70);
    }

    protected override void TaiLai()
    {
        var dons = Program.Lay<DatSanService>();
        Luoi.Rows.Clear();
        foreach (var k in Program.Lay<KhachHangService>().TimKiem(_tuKhoa.Text))
        {
            var i = Luoi.Rows.Add(k.MaKH, k.HoTen, k.SDT, k.Email,
                k.MatKhauHash == null ? "Chưa có" : "Đã có", dons.LichSuCuaKhach(k.MaKH).Count);
            Luoi.Rows[i].Tag = k;
        }
    }

    protected override void DoVaoONhap(KhachHang k)
    {
        _hoTen.Text = k.HoTen; _sdt.Text = k.SDT; _email.Text = k.Email;
    }

    protected override void XoaONhap() { _hoTen.Clear(); _sdt.Clear(); _email.Clear(); }

    protected override bool Luu()
    {
        var kq = Program.Lay<KhachHangService>().Luu(DangChon?.MaKH, _hoTen.Text, _sdt.Text, _email.Text);
        kq.HienKetQua();
        return kq.ThanhCong;
    }

    protected override bool Xoa()
    {
        var kq = Program.Lay<KhachHangService>().Xoa(DangChon!.MaKH);
        kq.HienKetQua();
        return kq.ThanhCong;
    }

    private void XemLichSu()
    {
        if (DangChon == null) { Ui.CanhBao("Chọn một khách hàng trước."); return; }
        var dons = Program.Lay<DatSanService>().LichSuCuaKhach(DangChon.MaKH);
        var f = new Form
        {
            Text = $"Lịch sử đặt sân · {DangChon.HoTen}", ClientSize = new Size(700, 420),
            StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, Font = Ui.FontThuong
        };
        var luoi = Ui.LuoiChiDoc();
        luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã đơn", FillWeight = 70 });
        luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sân", FillWeight = 120 });
        luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày", FillWeight = 90 });
        luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Giờ", FillWeight = 100 });
        luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tiền sân", FillWeight = 90 });
        luoi.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 100 });
        foreach (var d in dons)
            luoi.Rows.Add($"#DS{d.MaDatSan}", d.San?.TenSan, d.NgaySuDung.ToString("dd/MM/yyyy"),
                $"{d.GioBatDau.Gio()}–{d.GioKetThuc.Gio()}", d.TienSan.Vnd(), d.TrangThai);
        f.Controls.Add(luoi);
        using (f) f.ShowDialog(this);
    }
}
