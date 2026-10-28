using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC21 Thanh toán và in hóa đơn
public class FrmThanhToan : Form
{
    private readonly int _maDon;
    private readonly Label _lblTien = new() { Left = 14, Top = 50, Width = 400, Height = 130, Font = Ui.FontThuong };
    private readonly RadioButton _tienMat = new() { Text = PhuongThucThanhToan.TienMat, Left = 14, Top = 195, Width = 140, Checked = true, Font = Ui.FontThuong };
    private readonly RadioButton _chuyenKhoan = new() { Text = PhuongThucThanhToan.ChuyenKhoan, Left = 160, Top = 195, Width = 160, Font = Ui.FontThuong };

    public FrmThanhToan(int maDon)
    {
        _maDon = maDon;
        Text = $"Thanh toán đơn #DS{maDon}";
        ClientSize = new Size(440, 290);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        BackColor = Color.White;

        var t = Program.Lay<ThanhToanService>().TinhTamTinh(maDon);
        if (t == null) { Ui.CanhBao("Không tìm thấy đơn."); return; }
        _lblTien.Text =
            $"Tiền sân:            {t.TienSan.Vnd()}\n" +
            $"Tiền dịch vụ:        {t.TienDichVu.Vnd()}\n" +
            $"Tổng giá trị:        {t.TongGiaTri.Vnd()}\n" +
            $"Đã cọc:              {t.TienCoc.Vnd()}\n\n" +
            $"KHÁCH PHẢI TRẢ:      {t.PhaiTra.Vnd()}";

        var nutTT = Ui.Nut("Thanh toán & in", 270, 240, 150, chinh: true);
        nutTT.Click += (_, _) => ThanhToan();
        var nutDong = Ui.Nut("Đóng", 170, 240, 90);
        nutDong.Click += (_, _) => Close();

        Controls.AddRange(new Control[]
        {
            Ui.TieuDe($"Thanh toán đơn #DS{maDon}"), _lblTien,
            Ui.Nhan("Phương thức", 14, 170, 120), _tienMat, _chuyenKhoan, nutTT, nutDong
        });
    }

    private void ThanhToan()
    {
        var pt = _tienMat.Checked ? PhuongThucThanhToan.TienMat : PhuongThucThanhToan.ChuyenKhoan;
        var kq = Program.Lay<ThanhToanService>().ThanhToan(_maDon, PhienDangNhap.MaNV, pt);
        if (!kq.ThanhCong) { kq.HienKetQua(); return; }
        InHoaDon.Xem(_maDon);
        DialogResult = DialogResult.OK;
        Close();
    }
}
