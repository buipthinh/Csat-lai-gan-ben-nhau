using System.Globalization;
using System.Text;

namespace SportZone.Business;

// Tài khoản nhận tiền cọc, đọc từ mục "ChuyenKhoan" trong appsettings.json
public class TaiKhoanNhanCoc
{
    public string MaNganHang { get; set; } = "";   // mã BIN theo Napas, ví dụ MB Bank = 970422
    public string TenNganHang { get; set; } = "";
    public string SoTaiKhoan { get; set; } = "";
    public string TenTaiKhoan { get; set; } = "";

    public bool DayDu => MaNganHang != "" && SoTaiKhoan != "";
}

// Tạo chuỗi VietQR (chuẩn EMVCo của Napas) có sẵn số tiền và nội dung chuyển khoản.
// Khách quét bằng app ngân hàng nào cũng được, số tiền và nội dung tự điền nên không gõ sai.
public static class VietQR
{
    // Nội dung chuyển khoản để nhân viên đối chiếu với đơn, ví dụ "DS789"
    public static string NoiDungChuyenKhoan(int maDon) => $"DS{maDon}";

    public static string TaoChuoi(string maNganHang, string soTaiKhoan, decimal? soTien = null, string? noiDung = null)
    {
        var nguoiNhan = Truong("00", maNganHang) + Truong("01", soTaiKhoan);
        var thongTinTaiKhoan = Truong("00", "A000000727") + Truong("01", nguoiNhan) + Truong("02", "QRIBFTTA");

        var sb = new StringBuilder();
        sb.Append(Truong("00", "01"));
        sb.Append(Truong("01", soTien > 0 ? "12" : "11"));   // 12: mã dùng một lần có số tiền, 11: mã tĩnh
        sb.Append(Truong("38", thongTinTaiKhoan));
        sb.Append(Truong("53", "704"));                        // VND
        if (soTien > 0) sb.Append(Truong("54", decimal.Truncate(soTien.Value).ToString(CultureInfo.InvariantCulture)));
        sb.Append(Truong("58", "VN"));
        if (!string.IsNullOrWhiteSpace(noiDung)) sb.Append(Truong("62", Truong("08", noiDung)));
        sb.Append("6304");
        sb.Append(Crc16(sb.ToString()).ToString("X4"));
        return sb.ToString();
    }

    private static string Truong(string ma, string giaTri) => ma + giaTri.Length.ToString("00") + giaTri;

    // CRC-16/CCITT-FALSE, theo quy định của EMVCo
    private static ushort Crc16(string s)
    {
        ushort crc = 0xFFFF;
        foreach (var b in Encoding.ASCII.GetBytes(s))
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
                crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
        }
        return crc;
    }
}
