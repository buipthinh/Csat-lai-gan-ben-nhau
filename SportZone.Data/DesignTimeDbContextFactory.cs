using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SportZone.Data;

// Dùng cho lệnh "dotnet ef migrations add ..." (chỉ lúc thiết kế).
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SportZoneDbContext>
{
    public SportZoneDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SportZoneDbContext>()
            .UseSqlServer(DbConfig.ChuoiKetNoiMacDinh)
            .Options;
        return new SportZoneDbContext(options);
    }
}
