using SportZone.Business;
using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC17 xác nhận/từ chối, UC18 check-in, UC19 hủy tại quầy, UC20 thêm dịch vụ, UC21 thanh toán
public class FrmChiTietDon : Form
{
    private readonly int _maDon;
    private readonly Label _lblTieuDe = new() { Font = Ui.FontTieuDe, Left = 14, Top = 12, AutoSize = true };
    private readonly Label _lblTrangThai = new() { Left = 14, Top = 44, AutoSize = true, Font = Ui.FontThuong };
    private readonly ListView _thongTin = new()
    {
        Left = 14, Top = 70, Width = 520, Height = 230, View = View.Details, FullRowSelect = true,
        GridLines = false, HeaderStyle = ColumnHeaderStyle.None, Font = Ui.FontThuong
    };
    private readonly Panel _nut = new() { Left = 14, Top = 310, Width = 520, Height = 80 };
    private DatSan? _don;

    public FrmChiTietDon(int maDon)
    {
        _maDon = maDon;
        Text = $"Chi tiết đơn #DS{maDon}";
        ClientSize = new Size(548, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        BackColor = Color.White;

        _thongTin.Columns.Add("", 150);
        _thongTin.Columns.Add("", 350);
        Controls.AddRange(new Control[] { _lblTieuDe, _lblTrangThai, _thongTin, _nut });
        TaiLai();
    }

    private void TaiLai()
    {
        _don = Program.Lay<DatSanService>().ChiTiet(_maDon);
        if (_don == null) { Ui.CanhBao("Không tìm thấy đơn."); Close(); return; }

        var tamTinh = Program.Lay<ThanhToanService>().TinhTamTinh(_maDon)!;
        _lblTieuDe.Text = $"Đơn #DS{_don.MaDatSan} · {_don.San?.TenSan}";
        _lblTrangThai.Text = $"Trạng thái: {_don.TrangThai}";
        _lblTrangThai.BackColor = Ui.MauTrangThai(_don.TrangThai);

        _thongTin.Items.Clear();
        void Dong(string a, string b) => _thongTin.Items.Add(new ListViewItem(new[] { a, b }));
        Dong("Khách hàng", $"{_don.KhachHang?.HoTen} · {_don.KhachHang?.SDT}");
        Dong("Sân", $"{_don.San?.TenSan} ({_don.San?.LoaiSan?.TenLoai})");
        Dong("Ngày sử dụng", _don.NgaySuDung.ToString("dd/MM/yyyy"));
        Dong("Giờ", $"{_don.GioBatDau.Gio()} – {_don.GioKetThuc.Gio()} ({_don.SoGio:0.#} giờ)");
        Dong("Ngày tạo đơn", _don.NgayTao.ToString("dd/MM/yyyy HH:mm"));
        Dong("Nhân viên xác nhận", _don.NhanVienXacNhan?.HoTen ?? "—");
        Dong("Tiền sân", tamTinh.TienSan.Vnd());
        foreach (var c in _don.ChiTietDichVus)
            Dong("  " + c.DichVu?.TenDV, $"{c.SoLuong} {c.DichVu?.DonVi} × {c.DonGia.Vnd()} = {(c.SoLuong * c.DonGia).Vnd()}");
        Dong("Tổng giá trị", tamTinh.TongGiaTri.Vnd());
        Dong("Đã cọc", tamTinh.TienCoc.Vnd());
        Dong("Còn phải trả", tamTinh.PhaiTra.Vnd());
        if (_don.HoaDon != null)
            Dong("Hóa đơn", $"#HD{_don.HoaDon.MaHD} · {_don.HoaDon.PhuongThucTT} · {_don.HoaDon.NgayLap:dd/MM/yyyy HH:mm}");

        VeNut();
    }

    private void VeNut()
    {
        _nut.Controls.Clear();
        var x = 0;
        void Them(string chu, Action hanhDong, bool chinh = false)
        {
            var b = Ui.Nut(chu, x, 0, 125, chinh);
            b.Click += (_, _) => hanhDong();
            _nut.Controls.Add(b);
            x += 130;
        }

        switch (_don!.TrangThai)
        {
            case TrangThaiDon.ChoXacNhan:
                Them("Xác nhận đơn", () => Lam(Program.Lay<DatSanService>().XacNhan(_maDon, PhienDangNhap.MaNV)), chinh: true);
                Them("Từ chối đơn", () =>
                {
                    if (Ui.HoiXacNhan("Từ chối đơn này? Đơn sẽ chuyển sang trạng thái Đã hủy."))
                        Lam(Program.Lay<DatSanService>().TuChoi(_maDon, PhienDangNhap.MaNV));
                });
                break;
            case TrangThaiDon.DaXacNhan:
                Them("Check-in", () => Lam(Program.Lay<DatSanService>().CheckIn(_maDon)), chinh: true);
                Them("Hủy đơn", HuyDon);
                break;
            case TrangThaiDon.DangChoi:
                Them("Thêm dịch vụ", () => MoForm(new FrmDichVuDon(_maDon)));
                Them("Thanh toán", () => MoForm(new FrmThanhToan(_maDon)), chinh: true);
                break;
            case TrangThaiDon.HoanThanh:
                if (_don.HoaDon != null) Them("In lại hóa đơn", () => InHoaDon.Xem(_maDon));
                break;
        }
        var dong = Ui.Nut("Đóng", x, 0, 90);
        dong.Click += (_, _) => Close();
        _nut.Controls.Add(dong);
    }

    private void HuyDon()
    {
        if (!Ui.HoiXacNhan("Hủy đơn này tại quầy?")) return;
        Lam(Program.Lay<DatSanService>().HuyTaiQuay(_maDon));
    }

    private void Lam(KetQua kq)
    {
        kq.HienKetQua();
        if (kq.ThanhCong) TaiLai();
    }

    private void MoForm(Form f)
    {
        using (f) f.ShowDialog(this);
        TaiLai();
    }
}
