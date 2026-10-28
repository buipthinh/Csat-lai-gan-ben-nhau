using Microsoft.EntityFrameworkCore;
using SportZone.Business.Services;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business;

// Tạo CSDL (nếu chưa có) và nạp dữ liệu mẫu cho lần chạy đầu tiên.
public static class DbSeeder
{
    public static void KhoiTao(SportZoneDbContext db, bool taoDonMau = true)
    {
        if (db.Database.IsSqlServer() && db.Database.GetMigrations().Any()) db.Database.Migrate();
        else db.Database.EnsureCreated();

        if (db.VaiTros.Any()) return;

        var qlRole = new VaiTro { TenVaiTro = TenVaiTro.QuanLy };
        var nvRole = new VaiTro { TenVaiTro = TenVaiTro.NhanVien };
        db.VaiTros.AddRange(qlRole, nvRole);

        var quanLy = new NhanVien { HoTen = "Nguyễn Văn An", TenDangNhap = "admin", MatKhauHash = TaiKhoanService.BamMatKhau("admin123"), VaiTro = qlRole };
        var nhanVien = new NhanVien { HoTen = "Trần Thị Bình", TenDangNhap = "nhanvien", MatKhauHash = TaiKhoanService.BamMatKhau("nv123456"), VaiTro = nvRole };
        db.NhanViens.AddRange(quanLy, nhanVien);

        // Khung giờ: thường 5–17h, vàng 17–21h, tối 21–23h (BR03)
        var kgSang = new KhungGio { TenKhungGio = "Giờ thường", GioBatDau = new(5, 0), GioKetThuc = new(17, 0) };
        var kgVang = new KhungGio { TenKhungGio = "Giờ vàng", GioBatDau = new(17, 0), GioKetThuc = new(21, 0) };
        var kgToi = new KhungGio { TenKhungGio = "Giờ tối", GioBatDau = new(21, 0), GioKetThuc = new(23, 0) };
        db.KhungGios.AddRange(kgSang, kgVang, kgToi);

        // Giá theo thứ tự: thường, vàng, cuối tuần thường, cuối tuần vàng
        var loais = new (string Ten, string MoTa, decimal[] Gia, string[] Sans)[]
        {
            ("Bóng đá 5 người", "Sân cỏ nhân tạo 20x40m", new decimal[] { 300000, 450000, 350000, 500000 }, new[] { "Sân 5A", "Sân 5B", "Sân 5C" }),
            ("Bóng đá 7 người", "Sân cỏ nhân tạo 40x60m", new decimal[] { 500000, 700000, 600000, 800000 }, new[] { "Sân 7A", "Sân 7B" }),
            ("Cầu lông", "Sân trong nhà, thảm PVC", new decimal[] { 80000, 120000, 100000, 140000 }, new[] { "Cầu lông 1", "Cầu lông 2", "Cầu lông 3", "Cầu lông 4" }),
            ("Pickleball", "Sân ngoài trời có mái che", new decimal[] { 120000, 180000, 150000, 200000 }, new[] { "Pickleball 1", "Pickleball 2" }),
            ("Tennis", "Sân cứng tiêu chuẩn", new decimal[] { 200000, 300000, 250000, 350000 }, new[] { "Tennis 1", "Tennis 2" }),
        };
        var sans = new List<San>();
        foreach (var l in loais)
        {
            var loai = new LoaiSan { TenLoai = l.Ten, MoTa = l.MoTa };
            db.LoaiSans.Add(loai);
            foreach (var cuoiTuan in new[] { false, true })
            {
                var g = cuoiTuan ? 2 : 0;
                db.BangGias.Add(new BangGia { LoaiSan = loai, KhungGio = kgSang, LaCuoiTuan = cuoiTuan, GiaMoiGio = l.Gia[g] });
                db.BangGias.Add(new BangGia { LoaiSan = loai, KhungGio = kgVang, LaCuoiTuan = cuoiTuan, GiaMoiGio = l.Gia[g + 1] });
                db.BangGias.Add(new BangGia { LoaiSan = loai, KhungGio = kgToi, LaCuoiTuan = cuoiTuan, GiaMoiGio = l.Gia[g] });
            }
            foreach (var ten in l.Sans)
                sans.Add(new San { TenSan = ten, LoaiSan = loai, MoTa = $"{l.Ten} - {l.MoTa}" });
        }
        sans.First(s => s.TenSan == "Tennis 2").TrangThai = TrangThaiSan.BaoTri;
        db.Sans.AddRange(sans);

        var dvs = new[]
        {
            new DichVu { TenDV = "Nước suối", DonVi = "chai", DonGia = 10000 },
            new DichVu { TenDV = "Revive", DonVi = "chai", DonGia = 15000 },
            new DichVu { TenDV = "Trà đá", DonVi = "ly", DonGia = 5000 },
            new DichVu { TenDV = "Thuê vợt cầu lông", DonVi = "cây", DonGia = 30000 },
            new DichVu { TenDV = "Thuê vợt pickleball", DonVi = "cây", DonGia = 40000 },
            new DichVu { TenDV = "Thuê áo bib", DonVi = "cái", DonGia = 5000 },
        };
        db.DichVus.AddRange(dvs);

        var khs = new[]
        {
            new KhachHang { HoTen = "Lê Minh Khoa", SDT = "0901234567", Email = "khoa@example.com", MatKhauHash = TaiKhoanService.BamMatKhau("123456") },
            new KhachHang { HoTen = "Phạm Thu Hà", SDT = "0912345678", Email = "ha@example.com", MatKhauHash = TaiKhoanService.BamMatKhau("123456") },
            new KhachHang { HoTen = "Võ Quốc Huy", SDT = "0923456789" },
            new KhachHang { HoTen = "Đặng Ngọc Lan", SDT = "0934567890" },
        };
        db.KhachHangs.AddRange(khs);
        db.SaveChanges();

        if (taoDonMau) TaoDonMau(db, sans.Where(s => s.TrangThai == TrangThaiSan.HoatDong).ToList(), khs, dvs, nhanVien);
    }

    // Đơn mẫu trong 30 ngày qua (đã hoàn thành, có hóa đơn) và vài ngày tới, để màn hình lịch và thống kê có số liệu.
    private static void TaoDonMau(SportZoneDbContext db, List<San> sans, KhachHang[] khs, DichVu[] dvs, NhanVien nv)
    {
        var rnd = new Random(42);
        var homNay = DateOnly.FromDateTime(DateTime.Now);
        var bayGio = DateTime.Now;
        for (var ngay = homNay.AddDays(-30); ngay <= homNay.AddDays(3); ngay = ngay.AddDays(1))
        {
            foreach (var san in sans)
            {
                var gio = new TimeOnly(6, 0);
                while (true)
                {
                    gio = gio.AddMinutes(30 * rnd.Next(1, 8));
                    var dai = rnd.Next(2, 5) * 30;
                    if (gio < new TimeOnly(6, 0) || gio.AddMinutes(dai) > QuyTac.GioDongCua || gio.AddMinutes(dai) < gio) break;
                    // Giờ vàng đông khách hơn
                    var xacSuat = gio.Hour is >= 17 and < 21 ? 0.75 : 0.3;
                    if (rnd.NextDouble() < xacSuat)
                    {
                        var kt = gio.AddMinutes(dai);
                        var gia = GiaService.TinhTien(db, san.MaSan, ngay, gio, kt).DuLieu!;
                        var online = rnd.NextDouble() < 0.6;
                        var don = new DatSan
                        {
                            MaKH = khs[rnd.Next(khs.Length)].MaKH, MaSan = san.MaSan, NgaySuDung = ngay,
                            GioBatDau = gio, GioKetThuc = kt, TienSan = gia.TienSan, TienCoc = online ? gia.TienCoc : 0,
                            NgayTao = ngay.ToDateTime(gio).AddDays(-1), MaNVXacNhan = nv.MaNV
                        };
                        var batDau = ngay.ToDateTime(gio);
                        var ketThuc = ngay.ToDateTime(kt);
                        if (ketThuc <= bayGio)
                        {
                            if (rnd.NextDouble() < 0.08) don.TrangThai = TrangThaiDon.DaHuy;
                            else
                            {
                                don.TrangThai = TrangThaiDon.HoanThanh;
                                decimal tienDv = 0;
                                if (rnd.NextDouble() < 0.6)
                                {
                                    var dv = dvs[rnd.Next(dvs.Length)];
                                    var sl = rnd.Next(1, 5);
                                    don.ChiTietDichVus.Add(new ChiTietDichVu { MaDV = dv.MaDV, SoLuong = sl, DonGia = dv.DonGia });
                                    tienDv = sl * dv.DonGia;
                                }
                                don.HoaDon = new HoaDon
                                {
                                    MaNV = nv.MaNV, NgayLap = ketThuc, TongTien = don.TienSan + tienDv,
                                    PhuongThucTT = rnd.NextDouble() < 0.5 ? PhuongThucThanhToan.TienMat : PhuongThucThanhToan.ChuyenKhoan
                                };
                            }
                        }
                        else if (batDau <= bayGio) don.TrangThai = TrangThaiDon.DangChoi;
                        else if (online && rnd.NextDouble() < 0.4) { don.TrangThai = TrangThaiDon.ChoXacNhan; don.MaNVXacNhan = null; }
                        else don.TrangThai = TrangThaiDon.DaXacNhan;
                        db.DatSans.Add(don);
                    }
                    gio = gio.AddMinutes(dai);
                }
            }
        }
        db.SaveChanges();
    }
}
