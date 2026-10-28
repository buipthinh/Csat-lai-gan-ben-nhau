using System.Drawing.Printing;
using SportZone.Business.Services;
using SportZone.Data.Entities;

namespace SportZone.Desktop;

// UC21: in hóa đơn bằng PrintDocument, khổ giấy nhỏ như máy in bill ở quầy
internal static class InHoaDon
{
    public static void Xem(int maDon)
    {
        var don = Program.Lay<DatSanService>().ChiTiet(maDon);
        if (don?.HoaDon == null) { Ui.CanhBao("Đơn này chưa có hóa đơn."); return; }

        using var taiLieu = TaoTaiLieu(don);
        using var xemTruoc = new PrintPreviewDialog { Document = taiLieu, Width = 700, Height = 800, Text = $"Hóa đơn #HD{don.HoaDon.MaHD}" };
        xemTruoc.ShowDialog();
    }

    private static PrintDocument TaoTaiLieu(DatSan don)
    {
        var tl = new PrintDocument();
        tl.DefaultPageSettings.PaperSize = new PaperSize("Bill", 300, 700); // 1/100 inch
        tl.PrintPage += (_, e) => Ve(e, don);
        return tl;
    }

    private static void Ve(PrintPageEventArgs e, DatSan don)
    {
        var g = e.Graphics!;
        var fTieuDe = new Font("Segoe UI", 12, FontStyle.Bold);
        var f = new Font("Consolas", 9);
        var fDam = new Font("Consolas", 9, FontStyle.Bold);
        float y = 16, le = 14, rong = 272;
        var hd = don.HoaDon!;
        var tienDv = don.ChiTietDichVus.Sum(c => c.SoLuong * c.DonGia);

        void Giua(string chu, Font font)
        {
            var w = g.MeasureString(chu, font).Width;
            g.DrawString(chu, font, Brushes.Black, le + (rong - w) / 2, y);
            y += font.GetHeight(g) + 2;
        }
        void Dong(string trai, string phai = "", Font? font = null)
        {
            font ??= f;
            g.DrawString(trai, font, Brushes.Black, le, y);
            if (phai.Length > 0)
            {
                var w = g.MeasureString(phai, font).Width;
                g.DrawString(phai, font, Brushes.Black, le + rong - w, y);
            }
            y += font.GetHeight(g) + 1;
        }
        void Ke()
        {
            g.DrawLine(Pens.Gray, le, y + 2, le + rong, y + 2);
            y += 8;
        }

        Giua("SPORTZONE", fTieuDe);
        Giua("Khu liên hợp thể thao SportZone", f);
        Giua("Hotline 0909 000 111", f);
        Ke();
        Dong($"Hóa đơn #HD{hd.MaHD}", $"{hd.NgayLap:dd/MM/yyyy HH:mm}");
        Dong($"Đơn #DS{don.MaDatSan}", $"NV: {hd.NhanVien?.HoTen}");
        Dong($"Khách: {don.KhachHang?.HoTen}", don.KhachHang?.SDT ?? "");
        Ke();
        Dong($"{don.San?.TenSan} ({don.San?.LoaiSan?.TenLoai})");
        Dong($"{don.NgaySuDung:dd/MM/yyyy} {don.GioBatDau.Gio()}-{don.GioKetThuc.Gio()}", $"{don.SoGio:0.#} giờ");
        Dong("Tiền sân", don.TienSan.Vnd());
        foreach (var c in don.ChiTietDichVus)
            Dong($" {c.DichVu?.TenDV} x{c.SoLuong}", (c.SoLuong * c.DonGia).Vnd());
        if (tienDv > 0) Dong("Tiền dịch vụ", tienDv.Vnd());
        Ke();
        Dong("TỔNG GIÁ TRỊ", hd.TongTien.Vnd(), fDam);
        Dong("Đã cọc", don.TienCoc.Vnd());
        Dong("KHÁCH TRẢ", (hd.TongTien - don.TienCoc).Vnd(), fDam);
        Dong("Phương thức", hd.PhuongThucTT);
        Ke();
        Giua("Cảm ơn quý khách, hẹn gặp lại!", f);
        e.HasMorePages = false;
    }
}
