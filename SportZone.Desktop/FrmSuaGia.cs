using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

//Cửa sổ sửa giá của một loại sân trong một khung giờ (mở từ FrmBangGia)
public class FrmSuaGia : Form {
    private readonly LoaiSan _loai;
    private readonly KhungGio _khung;
    private readonly NumericUpDown _thuong = OGia(140, 96);
    private readonly NumericUpDown _cuoiTuan = OGia(140, 132);

    public FrmSuaGia(LoaiSan loai, KhungGio khung, decimal giaThuong, decimal giaCuoiTuan) {
        _loai = loai;
        _khung = khung;
        Text = "Sửa giá";
        ClientSize = new Size(400, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        Font = Ui.FontThuong;

        _thuong.Value = Math.Min(giaThuong, _thuong.Maximum);
        _cuoiTuan.Value = Math.Min(giaCuoiTuan, _cuoiTuan.Maximum);

        var nutLuu = Ui.Nut("Lưu", 140, 180, 100, chinh: true);
        var nutHuy = Ui.Nut("Hủy", 250, 180, 100);
        nutLuu.Click += (_, _) => Luu();
        nutHuy.DialogResult = DialogResult.Cancel;
        AcceptButton = nutLuu;
        CancelButton = nutHuy;

        Controls.AddRange(new Control[] {
            Ui.Nhan("Loại sân", 20, 20), Ui.Nhan(loai.TenLoai, 140, 20, 240),
            Ui.Nhan("Khung giờ", 20, 56), Ui.Nhan($"{khung.TenKhungGio} ({khung.GioBatDau.Gio()}–{khung.GioKetThuc.Gio()})", 140, 56, 240),
            Ui.Nhan("Ngày thường", 20, 96), _thuong, Ui.Nhan("đ/giờ", 300, 96, 60),
            Ui.Nhan("Cuối tuần", 20, 132), _cuoiTuan, Ui.Nhan("đ/giờ", 300, 132, 60),
            nutLuu, nutHuy
        });
    }

    //ô nhập giá: chỉ nhận số, bước 10.000, có dấu ngăn cách hàng nghìn
    private static NumericUpDown OGia(int x, int y) => new() {
        Left = x, Top = y, Width = 150, Maximum = 100_000_000, Increment = 10_000,
        ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right, Font = Ui.FontThuong
    };

    private void Luu() {
        var dm = Program.Lay<DanhMucService>();
        var kq = dm.LuuGia(_loai.MaLoai, _khung.MaKhungGio, false, _thuong.Value);
        if (kq.ThanhCong) kq = dm.LuuGia(_loai.MaLoai, _khung.MaKhungGio, true, _cuoiTuan.Value);
        if (!kq.ThanhCong) {
            kq.HienKetQua();
            return;
        }
        MessageBox.Show("Đã lưu bảng giá.", "SportZone", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
    }
}