using Microsoft.EntityFrameworkCore;

namespace SportZone.Data;

public static class DbConfig
{
    public const string ChuoiKetNoiMacDinh =
        "Server=(localdb)\\MSSQLLocalDB;Database=SportZone;Trusted_Connection=True;TrustServerCertificate=True";

    // provider: "SqlServer" (mặc định, dùng khi nộp bài) hoặc "Sqlite" (chạy thử không cần cài SQL Server)
    public static void CauHinh(DbContextOptionsBuilder b, string? provider, string? chuoiKetNoi)
    {
        if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            b.UseSqlite(chuoiKetNoi ?? "Data Source=sportzone.db");
        else
            b.UseSqlServer(chuoiKetNoi ?? ChuoiKetNoiMacDinh);
    }
}
