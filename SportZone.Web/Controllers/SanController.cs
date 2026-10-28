using Microsoft.AspNetCore.Mvc;
using SportZone.Business;
using SportZone.Business.Services;
using SportZone.Web.Models;

namespace SportZone.Web.Controllers;

public class SanController : Controller
{
    private readonly DanhMucService _danhMuc;
    private readonly DatSanService _datSan;

    public SanController(DanhMucService danhMuc, DatSanService datSan)
    {
        _danhMuc = danhMuc;
        _datSan = datSan;
    }

    internal static Dictionary<int, decimal> GiaThapNhat(DanhMucService dm) =>
        dm.TatCaBangGia().GroupBy(g => g.MaLoai).ToDictionary(g => g.Key, g => g.Min(x => x.GiaMoiGio));

    // UC02 Xem danh sách sân
    public IActionResult Index(int? maLoai)
    {
        var vm = new DanhSachSanVM
        {
            MaLoai = maLoai,
            LoaiSans = _danhMuc.DanhSachLoaiSan(),
            Sans = _danhMuc.DanhSachSan(maLoai),
            GiaTu = GiaThapNhat(_danhMuc)
        };
        return View(vm);
    }

    // UC04 Xem chi tiết sân và lưới giờ trống trong ngày
    public IActionResult ChiTiet(int id, DateOnly? ngay)
    {
        var san = _danhMuc.LaySan(id);
        if (san == null) return NotFound();
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var n = ngay is { } x && x >= homNay ? x : homNay;
        var dons = _datSan.DonTrongNgay(n, id);
        var bayGio = DateTime.Now;
        var vm = new ChiTietSanVM
        {
            San = san,
            Ngay = n,
            CacO = QuyTac.CacBlockTrongNgay().Select(t => new ONhoGio
            {
                Gio = t,
                DaDat = dons.Any(d => t >= d.GioBatDau && t < d.GioKetThuc),
                DaQua = n.ToDateTime(t) <= bayGio
            }).ToList(),
            BangGia = _danhMuc.BangGiaCuaLoai(san.MaLoai),
            KhungGios = _danhMuc.DanhSachKhungGio()
        };
        return View(vm);
    }

    // UC03 Tìm sân trống
    public IActionResult TimSanTrong(int? maLoai, DateOnly? ngay, TimeOnly? gioBatDau, TimeOnly? gioKetThuc)
    {
        var vm = new TimSanTrongVM
        {
            MaLoai = maLoai,
            Ngay = ngay ?? DateOnly.FromDateTime(DateTime.Now),
            GioBatDau = gioBatDau ?? new TimeOnly(18, 0),
            GioKetThuc = gioKetThuc ?? new TimeOnly(19, 30),
            LoaiSans = _danhMuc.DanhSachLoaiSan()
        };
        if (ngay != null && gioBatDau != null && gioKetThuc != null)
        {
            vm.Loi = QuyTac.KiemTraKhungGio(vm.Ngay, vm.GioBatDau, vm.GioKetThuc, DateTime.Now);
            if (vm.Loi == null) vm.KetQua = _datSan.TimSanTrong(maLoai, vm.Ngay, vm.GioBatDau, vm.GioKetThuc);
        }
        return View(vm);
    }
}
