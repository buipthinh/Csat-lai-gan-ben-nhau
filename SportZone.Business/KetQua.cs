namespace SportZone.Business;

// Kết quả trả về cho tầng giao diện: thành công hoặc thông báo lỗi nghiệp vụ dễ hiểu.
public class KetQua
{
    public bool ThanhCong { get; init; }
    public string ThongBao { get; init; } = "";

    public static KetQua Ok(string thongBao = "") => new() { ThanhCong = true, ThongBao = thongBao };
    public static KetQua Loi(string thongBao) => new() { ThanhCong = false, ThongBao = thongBao };
}

public class KetQua<T> : KetQua
{
    public T? DuLieu { get; init; }

    public static KetQua<T> Ok(T duLieu, string thongBao = "") => new() { ThanhCong = true, DuLieu = duLieu, ThongBao = thongBao };
    public static new KetQua<T> Loi(string thongBao) => new() { ThanhCong = false, ThongBao = thongBao };
}
