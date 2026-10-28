using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportZone.Business.Services;
using SportZone.Data.Entities;
using SportZone.Web.Infrastructure;
using SportZone.Web.Models;

namespace SportZone.Web.Controllers;

public class TaiKhoanController : Controller
{
    private readonly TaiKhoanService _taiKhoan;
    public TaiKhoanController(TaiKhoanService taiKhoan) => _taiKhoan = taiKhoan;

    private Task DangNhapCookie(KhachHang kh) =>
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, kh.MaKH.ToString()),
                new Claim(ClaimTypes.Name, kh.HoTen),
                new Claim(ClaimTypes.MobilePhone, kh.SDT)
            }, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true });

    // UC05 Đăng ký
    [HttpGet] public IActionResult DangKy() => View(new DangKyVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(DangKyVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var kq = _taiKhoan.DangKy(vm.HoTen, vm.SDT, vm.Email, vm.MatKhau);
        if (!kq.ThanhCong) { ModelState.AddModelError("", kq.ThongBao); return View(vm); }
        await DangNhapCookie(kq.DuLieu!);
        TempData["ThongBao"] = "Đăng ký thành công. Chào mừng bạn đến SportZone!";
        return RedirectToAction("Index", "Home");
    }

    // UC06 Đăng nhập
    [HttpGet] public IActionResult DangNhap(string? returnUrl) => View(new DangNhapVM { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var kq = _taiKhoan.DangNhapKhach(vm.TaiKhoan, vm.MatKhau);
        if (!kq.ThanhCong) { ModelState.AddModelError("", kq.ThongBao); return View(vm); }
        await DangNhapCookie(kq.DuLieu!);
        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)) return Redirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // UC07 Cập nhật thông tin cá nhân
    [Authorize, HttpGet]
    public IActionResult ThongTin()
    {
        var kh = _taiKhoan.LayKhach(User.MaKH()!.Value);
        if (kh == null) return RedirectToAction(nameof(DangNhap));
        return View(new ThongTinVM { SDT = kh.SDT, HoTen = kh.HoTen, Email = kh.Email });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ThongTin(ThongTinVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var kq = _taiKhoan.CapNhatThongTin(User.MaKH()!.Value, vm.HoTen, vm.Email);
        if (!kq.ThanhCong) { ModelState.AddModelError("", kq.ThongBao); return View(vm); }
        await DangNhapCookie(_taiKhoan.LayKhach(User.MaKH()!.Value)!); // cập nhật tên hiển thị
        TempData["ThongBao"] = kq.ThongBao;
        return RedirectToAction(nameof(ThongTin));
    }

    [Authorize, HttpGet] public IActionResult DoiMatKhau() => View(new DoiMatKhauVM());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public IActionResult DoiMatKhau(DoiMatKhauVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var kq = _taiKhoan.DoiMatKhauKhach(User.MaKH()!.Value, vm.MatKhauCu, vm.MatKhauMoi);
        if (!kq.ThanhCong) { ModelState.AddModelError("", kq.ThongBao); return View(vm); }
        TempData["ThongBao"] = kq.ThongBao;
        return RedirectToAction(nameof(ThongTin));
    }
}
