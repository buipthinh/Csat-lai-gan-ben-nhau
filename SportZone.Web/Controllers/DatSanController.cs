using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportZone.Business;
using SportZone.Business.Services;
using SportZone.Web.Infrastructure;
using SportZone.Web.Models;

namespace SportZone.Web.Controllers;

[Authorize]
public class DatSanController : Controller
{
    private readonly DatSanService _datSan;
    private readonly GiaService _gia;
    private readonly DanhMucService _danhMuc;

    public DatSanController(DatSanService datSan, GiaService gia, DanhMucService danhMuc)
    {
        _datSan = datSan;
        _gia = gia;
        _danhMuc = danhMuc;
    }

    // UC08 bước 1: xem lại giờ đã chọn và tiền (UC33) trước khi xác nhận
    [HttpGet]
    public IActionResult XacNhan(int maSan, DateOnly ngay, TimeOnly gioBatDau, TimeOnly gioKetThuc)
    {
        var san = _danhMuc.LaySan(maSan);
        if (san == null) return NotFound();
        var vm = new XacNhanDatVM { San = san, Ngay = ngay, GioBatDau = gioBatDau, GioKetThuc = gioKetThuc };
        vm.Loi = QuyTac.KiemTraKhungGio(ngay, gioBatDau, gioKetThuc, DateTime.Now);
        if (vm.Loi == null && _datSan.CoTrungLich(maSan, ngay, gioBatDau, gioKetThuc))
            vm.Loi = "Khung giờ này đã có người đặt. Vui lòng chọn giờ khác.";
        if (vm.Loi == null)
        {
            var bg = _gia.TinhTien(maSan, ngay, gioBatDau, gioKetThuc);
            if (bg.ThanhCong) vm.BaoGia = bg.DuLieu; else vm.Loi = bg.ThongBao;
        }
        return View(vm);
    }

    // UC08 bước 2: tạo đơn (kiểm tra trùng lần nữa trong transaction)
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Dat(int maSan, DateOnly ngay, TimeOnly gioBatDau, TimeOnly gioKetThuc)
    {
        var kq = _datSan.DatSanOnline(User.MaKH()!.Value, maSan, ngay, gioBatDau, gioKetThuc);
        if (!kq.ThanhCong)
        {
            TempData["Loi"] = kq.ThongBao;
            return RedirectToAction("ChiTiet", "San", new { id = maSan, ngay = ngay.ToString("yyyy-MM-dd") });
        }
        TempData["ThongBao"] = $"Đặt sân thành công! Đơn #DS{kq.DuLieu!.MaDatSan} đang chờ nhân viên xác nhận. Vui lòng chuyển cọc {kq.DuLieu.TienCoc.Vnd()}.";
        return RedirectToAction(nameof(ChiTiet), new { id = kq.DuLieu.MaDatSan });
    }

    // UC09 Xem lịch sử đặt sân
    public IActionResult LichSu() => View(_datSan.LichSuCuaKhach(User.MaKH()!.Value));

    // UC10 Xem chi tiết đơn
    public IActionResult ChiTiet(int id)
    {
        var don = _datSan.ChiTiet(id);
        if (don == null || don.MaKH != User.MaKH()) return NotFound();
        ViewBag.DuocHuy = _datSan.KhachDuocHuy(don);
        return View(don);
    }

    // UC11 Hủy đặt sân
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Huy(int id)
    {
        var kq = _datSan.HuyBoiKhach(id, User.MaKH()!.Value);
        TempData[kq.ThanhCong ? "ThongBao" : "Loi"] = kq.ThongBao;
        return RedirectToAction(nameof(ChiTiet), new { id });
    }
}
