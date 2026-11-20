using SportZone.Data.Entities;

namespace SportZone.Desktop;

//UC23 Quản lý loại sân
public class FrmLoaiSan : FrmCrudBase<LoaiSan>
{
    private readonly TextBox _ten = new() { Left = 130, Top = 14, Width = 250, Font = Ui.FontThuong };
    private readonly TextBox _moTa = new() { Left = 130, Top = 48, Width = 450, Height = 60, Multiline = true, Font = Ui.FontThuong };
    private readonly Label _luuY = new() {
        Text = "* Lưu ý:\n"
                + "- Thêm loại sân thì nhấn vào Thêm mới.\n"
                + "- Nếu muốn thay đổi loại sân, nhấn vào một loại sân nào đó, sau khi thay đổi nhấn Lưu.",
        Left = 600, Top = 14, Width = 800, Height = 80,
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 13)
    };

    //Constructor
    public FrmLoaiSan() : base("Quản lý loại sân")
    {
        ONhap.Controls.AddRange(new Control[]
        {
            Ui.Nhan("Tên loại sân", 12, 14), _ten,
            Ui.Nhan("Mô tả", 12, 48), _moTa,
            _luuY
        });
        KhoiDong();
    }

    protected override void TaoCot()
    {
        ThemCot("Mã loại", 60);
        ThemCot("Tên loại sân", 160);
        ThemCot("Số sân", 60);
        ThemCot("Mô tả", 250);
        ThemCot("Hiển thị", 80); 
    }

    protected override void TaiLai()
    {
        var sans = Program.Lay<Business.Services.DanhMucService>().DanhSachSan(caAn: true);
        Luoi.Rows.Clear();
        foreach (var l in Program.Lay<Business.Services.DanhMucService>().DanhSachLoaiSan(caAn: true))
        {
            var i = Luoi.Rows.Add(l.MaLoai, l.TenLoai, sans.Count(s => s.MaLoai == l.MaLoai), l.MoTa, ChuHienThi(l.DaAn));
            Luoi.Rows[i].Tag = l;
            ToMauDongAn(i, l.DaAn);
        }
    }

    protected override void DoVaoONhap(LoaiSan l) { _ten.Text = l.TenLoai; _moTa.Text = l.MoTa; }
    protected override void XoaONhap() { _ten.Clear(); _moTa.Clear(); }

    protected override bool Luu()
    {
        var kq = Program.Lay<Business.Services.DanhMucService>().LuuLoaiSan(DangChon?.MaLoai, _ten.Text, _moTa.Text);
        kq.HienKetQua();
        return kq.ThanhCong;
    }

    protected override string ChuNutXoa => "Ẩn / Hiện";
    protected override string CauHoiXoa => HoiAnHien(DangChon?.DaAn == true, $"loại sân {DangChon?.TenLoai}",
        "Khách và nhân viên sẽ không thấy, không đặt được các sân thuộc loại này.");

    protected override bool Xoa()
    {
        var kq = Program.Lay<Business.Services.DanhMucService>().AnHienLoaiSan(DangChon!.MaLoai);
        kq.HienKetQua();
        return kq.ThanhCong;
    }
}
