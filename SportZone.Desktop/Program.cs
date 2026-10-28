using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportZone.Business;
using SportZone.Business.Services;

namespace SportZone.Desktop;

internal static class Program
{
    public static IServiceProvider Dich { get; private set; } = null!;

    public static T Lay<T>() where T : notnull => Dich.GetRequiredService<T>();

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        Dich = new ServiceCollection().AddSportZone(config).BuildServiceProvider();

        try
        {
            Dich.KhoiTaoCsdl();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không kết nối được cơ sở dữ liệu.\n\n" + ex.Message,
                "SportZone", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var dangNhap = new FrmDangNhap();
        if (dangNhap.ShowDialog() != DialogResult.OK) return;
        Application.Run(new FrmMain());
    }
}
