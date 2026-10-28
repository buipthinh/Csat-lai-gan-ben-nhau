using SportZone.Business;
using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC15 Đặt sân tại quầy: tìm khách theo SĐT (tạo nhanh nếu chưa có), chọn sân và giờ, xem tiền rồi tạo đơn.
public class FrmDatTaiQuay : Form
{
    private readonly TextBox _txtSdt = new() { Left = 130, Top = 48, Width = 130, Font = Ui.FontThuong };
    private readonly TextBox _txtHoTen = new() { Left = 130, Top = 82, Width = 230, Font = Ui.FontThuong };
    private readonly Label _lblKhach = new() { Left = 270, Top = 51, Width = 230, AutoSize = false, ForeColor = Color.Gray, Font = Ui.FontThuong };
    private readonly ComboBox _cbSan = new() { Left = 130, Top = 126, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.FontThuong };
    private readonly DateTimePicker _ngay = Ui.ChonNgay(130, 160);
    private readonly ComboBox _bd = Ui.ComboGio(130, 194);
    private readonly ComboBox _kt = Ui.ComboGio(270, 194, themGioDongCua: true);
    private readonly Label _lblTien = new() { Left = 14, Top = 240, Width = 480, Height = 60, Font = Ui.FontThuong };
    private KhachHang? _khach;

    public FrmDatTaiQuay() : this(null, null, null) { }

    public FrmDatTaiQuay(int? maSan, DateOnly? ngay, TimeOnly? gioBatDau)
    {
        Text = "Đặt sân tại quầy";
        ClientSize = new Size(520, 350);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        BackColor = Color.White;

        var nutTim = Ui.Nut("Tìm khách", 370, 46, 90);
        nutTim.Click += (_, _) => TimKhach();
        var nutTao = Ui.Nut("Tạo đơn", 370, 310, 130, chinh: true);
        nutTao.Click += (_, _) => TaoDon();
        var nutDong = Ui.Nut("Đóng", 270, 310, 90);
        nutDong.Click += (_, _) => Close();

        foreach (var s in Program.Lay<DanhMucService>().DanhSachSan(chiHoatDong: true)) _cbSan.Items.Add(s);
        _cbSan.FormattingEnabled = true;
        _cbSan.Format += (_, e) => { if (e.ListItem is San s) e.Value = $"{s.TenSan} — {s.LoaiSan?.TenLoai}"; };
        if (_cbSan.Items.Count > 0) _cbSan.SelectedIndex = 0;
        if (maSan != null)
        {
            var i = _cbSan.Items.Cast<San>().ToList().FindIndex(s => s.MaSan == maSan);
            if (i >= 0) _cbSan.SelectedIndex = i;
        }
        _ngay.Value = (ngay ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue);
        if (gioBatDau != null)
        {
            _bd.SelectedItem = gioBatDau.Value;
            _kt.SelectedItem = gioBatDau.Value.AddHours(1);
        }

        Controls.AddRange(new Control[]
        {
            Ui.TieuDe("Đặt sân tại quầy"),
            Ui.Nhan("Số điện thoại", 14, 48), _txtSdt, nutTim, _lblKhach,
            Ui.Nhan("Họ tên khách", 14, 82), _txtHoTen,
            Ui.Nhan("Sân", 14, 126), _cbSan,
            Ui.Nhan("Ngày", 14, 160), _ngay,
            Ui.Nhan("Từ giờ", 14, 194), _bd, Ui.Nhan("Đến", 230, 194, 40), _kt,
            _lblTien, nutTao, nutDong
        });

        _cbSan.SelectedIndexChanged += (_, _) => TinhLaiTien();
        _ngay.ValueChanged += (_, _) => TinhLaiTien();
        _bd.SelectedIndexChanged += (_, _) => TinhLaiTien();
        _kt.SelectedIndexChanged += (_, _) => TinhLaiTien();
        TinhLaiTien();
    }

    private void TimKhach()
    {
        var sdt = TaiKhoanService.ChuanHoaSdt(_txtSdt.Text);
        _khach = Program.Lay<KhachHangService>().TimKiem(sdt).FirstOrDefault(k => k.SDT == sdt);
        if (_khach == null)
        {
            _lblKhach.Text = "Khách mới, nhập họ tên để tạo nhanh.";
            _txtHoTen.Clear();
            _txtHoTen.Focus();
        }
        else
        {
            _lblKhach.Text = $"Khách cũ: {_khach.HoTen}";
            _txtHoTen.Text = _khach.HoTen;
        }
    }

    private void TinhLaiTien()
    {
        if (_cbSan.SelectedItem is not San san) return;
        var ngay = _ngay.NgayDangChon();
        var bd = _bd.GioDangChon();
        var kt = _kt.GioDangChon();
        var loi = QuyTac.KiemTraKhungGio(ngay, bd, kt, DateTime.Now);
        if (loi != null) { _lblTien.Text = loi; _lblTien.ForeColor = Color.Firebrick; return; }
        var bg = Program.Lay<GiaService>().TinhTien(san.MaSan, ngay, bd, kt);
        if (!bg.ThanhCong) { _lblTien.Text = bg.ThongBao; _lblTien.ForeColor = Color.Firebrick; return; }
        var trung = Program.Lay<DatSanService>().CoTrungLich(san.MaSan, ngay, bd, kt);
        _lblTien.ForeColor = trung ? Color.Firebrick : Color.Black;
        _lblTien.Text = trung
            ? "Khung giờ này đã có người đặt, vui lòng chọn giờ khác."
            : $"Tiền sân: {bg.DuLieu!.TienSan.Vnd()} ({(kt - bd).TotalHours:0.#} giờ"
              + (bg.DuLieu.LaCuoiTuan ? ", giá cuối tuần)" : ")")
              + "\nĐặt tại quầy không thu cọc, khách thanh toán khi chơi xong.";
    }

    private void TaoDon()
    {
        if (_cbSan.SelectedItem is not San san) return;
        if (string.IsNullOrWhiteSpace(_txtSdt.Text) || string.IsNullOrWhiteSpace(_txtHoTen.Text))
        {
            Ui.CanhBao("Nhập số điện thoại và họ tên khách.");
            return;
        }
        // Lưu khách (tạo mới nếu chưa có) rồi tạo đơn đã xác nhận
        var kqKhach = Program.Lay<KhachHangService>().Luu(_khach?.MaKH, _txtHoTen.Text, _txtSdt.Text, null);
        if (!kqKhach.ThanhCong) { kqKhach.HienKetQua(); return; }
        _khach = kqKhach.DuLieu;

        var kq = Program.Lay<DatSanService>().DatTaiQuay(
            _khach!.MaKH, san.MaSan, _ngay.NgayDangChon(), _bd.GioDangChon(), _kt.GioDangChon(), PhienDangNhap.MaNV);
        kq.HienKetQua();
        if (kq.ThanhCong) { DialogResult = DialogResult.OK; Close(); }
    }
}
