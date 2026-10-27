namespace SportZone.Business;

// Quy tắc nghiệp vụ BR01–BR10 trong PRD, gom một chỗ để Web và Desktop dùng chung.
public static class QuyTac
{
    public static readonly TimeOnly GioMoCua = new(5, 0);       // BR01
    public static readonly TimeOnly GioDongCua = new(23, 0);    // BR01
    public const int PhutMoiBlock = 30;                          // BR02
    public const int SoPhutToiThieu = 60;                        // BR02
    public const double SoGioHuyToiThieu = 3;                    // BR07
    public const decimal TiLeCoc = 0.3m;                         // BR09

    public static bool LaCuoiTuan(DateOnly ngay) =>
        ngay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday; // BR03

    // BR01, BR02 và không đặt giờ đã qua. Trả về null nếu hợp lệ.
    public static string? KiemTraKhungGio(DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc, DateTime bayGio)
    {
        if (batDau < GioMoCua || ketThuc > GioDongCua || ketThuc <= batDau)
            return "Sân chỉ mở cửa từ 5:00 đến 23:00.";
        if (batDau.Minute % PhutMoiBlock != 0 || ketThuc.Minute % PhutMoiBlock != 0 || batDau.Second != 0 || ketThuc.Second != 0)
            return "Giờ đặt phải theo block 30 phút (phút 00 hoặc 30).";
        if ((ketThuc - batDau).TotalMinutes < SoPhutToiThieu)
            return "Phải đặt tối thiểu 1 giờ.";
        if (ngay.ToDateTime(batDau) <= bayGio)
            return "Giờ bắt đầu đã qua.";
        return null;
    }

    public static decimal TinhCoc(decimal tienSan) =>
        Math.Round(tienSan * TiLeCoc / 1000m, MidpointRounding.AwayFromZero) * 1000m;

    // Danh sách các block 30 phút trong ngày (5:00, 5:30, ..., 22:30)
    public static IEnumerable<TimeOnly> CacBlockTrongNgay()
    {
        for (var t = GioMoCua; t < GioDongCua; t = t.AddMinutes(PhutMoiBlock))
            yield return t;
    }
}
