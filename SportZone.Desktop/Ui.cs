using System.Globalization;
using SportZone.Business;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// Vài hàm dựng giao diện dùng chung, để các form ngắn và nhất quán (toàn bộ UI viết bằng code, không dùng Designer).
internal static class Ui
{
    public static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
    public static readonly Color Xanh = Color.FromArgb(47, 143, 91);
    public static readonly Color XanhDam = Color.FromArgb(35, 122, 76);
    public static readonly Font FontThuong = new("Segoe UI", 9.75f);
    public static readonly Font FontTieuDe = new("Segoe UI Semibold", 12f);

    public static string Vnd(this decimal so) => so.ToString("#,##0", Vi) + " đ";
    public static string Gio(this TimeOnly t) => t.ToString("HH:mm");

    public static Color MauTrangThai(string trangThai) => trangThai switch
    {
        TrangThaiDon.ChoXacNhan => Color.FromArgb(255, 243, 205),
        TrangThaiDon.DaXacNhan => Color.FromArgb(207, 226, 255),
        TrangThaiDon.DangChoi => Color.FromArgb(207, 244, 252),
        TrangThaiDon.HoanThanh => Color.FromArgb(209, 231, 221),
        _ => Color.FromArgb(233, 236, 239)
    };

    public static Label Nhan(string chu, int x, int y, int rong = 110)
        => new() { Text = chu, Left = x, Top = y + 3, Width = rong, Font = FontThuong, AutoSize = false };

    public static Label TieuDe(string chu) =>
        new() { Text = chu, Font = FontTieuDe, AutoSize = true, Left = 12, Top = 10 };

    public static Button Nut(string chu, int x, int y, int rong = 110, bool chinh = false)
    {
        var b = new Button { Text = chu, Left = x, Top = y, Width = rong, Height = 30, Font = FontThuong };
        if (chinh)
        {
            b.BackColor = Xanh; b.ForeColor = Color.White; b.FlatStyle = FlatStyle.Flat;
        }
        return b;
    }

    public static DataGridView LuoiChiDoc()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
            BackgroundColor = Color.White, BorderStyle = BorderStyle.None, Font = FontThuong
        };
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 242);
        g.ColumnHeadersDefaultCellStyle.Font = FontThuong;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 32;
        return g;
    }

    public static void HienKetQua(this KetQua kq, string tieuDe = "SportZone")
        => MessageBox.Show(kq.ThongBao, tieuDe, MessageBoxButtons.OK,
            kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

    public static bool HoiXacNhan(string cauHoi) =>
        MessageBox.Show(cauHoi, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static void CanhBao(string chu) =>
        MessageBox.Show(chu, "SportZone", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    // ComboBox giờ theo block 30 phút trong giờ mở cửa
    public static ComboBox ComboGio(int x, int y, TimeOnly? chon = null, bool themGioDongCua = false)
    {
        var cb = new ComboBox { Left = x, Top = y, Width = 90, DropDownStyle = ComboBoxStyle.DropDownList, Font = FontThuong };
        foreach (var t in Business.QuyTac.CacBlockTrongNgay()) cb.Items.Add(t);
        if (themGioDongCua) cb.Items.Add(Business.QuyTac.GioDongCua);
        cb.FormattingEnabled = true;
        cb.Format += (_, e) => { if (e.ListItem is TimeOnly t) e.Value = t.Gio(); };
        if (chon != null) cb.SelectedItem = chon.Value;
        else if (cb.Items.Count > 0) cb.SelectedIndex = 0;
        return cb;
    }

    public static TimeOnly GioDangChon(this ComboBox cb) => (TimeOnly)cb.SelectedItem!;

    public static DateTimePicker ChonNgay(int x, int y, int rong = 130)
        => new() { Left = x, Top = y, Width = rong, Format = DateTimePickerFormat.Custom,
                   CustomFormat = "dd/MM/yyyy", Font = FontThuong };

    public static DateOnly NgayDangChon(this DateTimePicker p) => DateOnly.FromDateTime(p.Value);
}
