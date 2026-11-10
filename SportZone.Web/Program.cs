using Microsoft.AspNetCore.Authentication.Cookies;
using SportZone.Business;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSportZone(builder.Configuration);
// Tài khoản nhận tiền cọc, dùng để tạo mã VietQR trên trang chi tiết đơn
builder.Services.AddSingleton(builder.Configuration.GetSection("ChuyenKhoan").Get<TaiKhoanNhanCoc>() ?? new TaiKhoanNhanCoc());
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/TaiKhoan/DangNhap";
        o.AccessDeniedPath = "/TaiKhoan/DangNhap";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
    });

var app = builder.Build();

app.Services.KhoiTaoCsdl();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
