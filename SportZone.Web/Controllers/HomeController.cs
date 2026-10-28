using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportZone.Business.Services;
using SportZone.Web.Models;

namespace SportZone.Web.Controllers;

public class HomeController : Controller
{
    private readonly DanhMucService _danhMuc;
    public HomeController(DanhMucService danhMuc) => _danhMuc = danhMuc;

    // UC01 Xem trang chủ
    public IActionResult Index()
    {
        var vm = new TrangChuVM
        {
            LoaiSans = _danhMuc.DanhSachLoaiSan(),
            Sans = _danhMuc.DanhSachSan(chiHoatDong: true),
            GiaTu = SanController.GiaThapNhat(_danhMuc)
        };
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
