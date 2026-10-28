using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportZone.Business.Services;
using SportZone.Data;

namespace SportZone.Business;

public static class DangKyDichVu
{
    // Dùng chung cho Web (Program.cs) và Desktop. Đọc "DatabaseProvider" và "ConnectionStrings:SportZone" từ appsettings.json.
    public static IServiceCollection AddSportZone(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["DatabaseProvider"];
        var chuoi = config.GetConnectionString("SportZone");
        services.AddDbContextFactory<SportZoneDbContext>(b => DbConfig.CauHinh(b, provider, chuoi));
        services.AddSingleton<GiaService>();
        services.AddSingleton<DatSanService>(sp => new DatSanService(sp.GetRequiredService<IDbContextFactory<SportZoneDbContext>>()));
        services.AddSingleton<ThanhToanService>();
        services.AddSingleton<TaiKhoanService>();
        services.AddSingleton<KhachHangService>();
        services.AddSingleton<DanhMucService>();
        services.AddSingleton<ThongKeService>();
        return services;
    }

    public static void KhoiTaoCsdl(this IServiceProvider sp)
    {
        using var db = sp.GetRequiredService<IDbContextFactory<SportZoneDbContext>>().CreateDbContext();
        DbSeeder.KhoiTao(db);
    }
}
