using Microsoft.EntityFrameworkCore;
using SportZone.Data;
using SportZone.Data.Entities;

namespace SportZone.Business;

//Tạo CSDL (nếu chưa có), chạy migration còn thiếu và nạp danh mục giống Database/Database.sql
//Không nạp đơn đặt sân / hóa đơn: muốn có số liệu demo thì chạy Database.sql
//Nếu CSDL đã có dữ liệu (ví dụ đã chạy Database.sql) thì chỉ chạy migration, không nạp gì thêm
public static class DbSeeder {
    public static void KhoiTao(SportZoneDbContext db, bool taoDonMau = false) {
        if (db.Database.IsSqlServer() && db.Database.GetMigrations().Any()) db.Database.Migrate();
        else db.Database.EnsureCreated();

        if (db.VaiTros.Any()) return;

        //VaiTro: 2 dòng
        var qlRole = new VaiTro { TenVaiTro = TenVaiTro.QuanLy };
        var nvRole = new VaiTro { TenVaiTro = TenVaiTro.NhanVien };
        db.VaiTros.AddRange(qlRole, nvRole);

        //NhanVien: 2 dòng
        var quanLy = new NhanVien { HoTen = "Lê Thanh Huy", TenDangNhap = "admin", MatKhauHash = "$2a$11$aw27BwXA9iKbfOs6XPaP5erU0lpuJWZG24tk1eBpevmbNZNEGqf4y", VaiTro = qlRole };
        var nhanVien = new NhanVien { HoTen = "Bùi Phúc Thịnh", TenDangNhap = "nhanvien", MatKhauHash = "$2a$11$yKPWEIJHQHQ3gACVJhLhlesJNdIIVpvU95MA3NeJEtz/7YSs.25nC", VaiTro = nvRole };
        db.NhanViens.AddRange(quanLy, nhanVien);

        //KhachHang: 4 dòng
        var khs = new[] {
            new KhachHang { HoTen = "Bùi Quang Tín", SDT = "0901234567", Email = "tin@gmail.com", MatKhauHash = "$2a$11$DiSOGyf.DdJhNbEsz/bVMuYaZVrhMADyJPG/L86BbB6QFyRqUCvSe" },
            new KhachHang { HoTen = "Phạm Thu Hà", SDT = "0912345678", MatKhauHash = "$2a$11$iPpAvsoFGe1Zc/4g1OXwmus/uhejEvf9zEVjjANCuRxN.tyQVjTEm" },
            new KhachHang { HoTen = "Võ Quốc Huy", SDT = "0923456789" },
            new KhachHang { HoTen = "Đặng Ngọc Lan", SDT = "0934567890" },
        };
        db.KhachHangs.AddRange(khs);

        //KhungGio: thường 5–17h, vàng 17–21h, tối 21–23h (BR03)
        var kgThuong = new KhungGio { TenKhungGio = "Giờ thường", GioBatDau = new(5, 0), GioKetThuc = new(17, 0) };
        var kgVang = new KhungGio { TenKhungGio = "Giờ vàng", GioBatDau = new(17, 0), GioKetThuc = new(21, 0) };
        var kgToi = new KhungGio { TenKhungGio = "Giờ tối", GioBatDau = new(21, 0), GioKetThuc = new(23, 0) };
        db.KhungGios.AddRange(kgThuong, kgVang, kgToi);

        //LoaiSan: 5 dòng, San: 13 dòng, BangGia: 30 dòng
        //Giá theo thứ tự: thường, vàng, cuối tuần thường, cuối tuần vàng (giờ tối lấy giá giờ thường)
        var loais = new (string Ten, string MoTa, decimal[] Gia, string[] Sans)[] {
            ("Bóng đá 5 người", "Sân cỏ nhân tạo 20x40m", new decimal[] { 300000, 450000, 350000, 500000 }, new[] { "Sân 5A", "Sân 5B", "Sân 5C" }),
            ("Bóng đá 7 người", "Sân cỏ nhân tạo 40x60m", new decimal[] { 500000, 700000, 600000, 800000 }, new[] { "Sân 7A", "Sân 7B" }),
            ("Cầu lông", "Sân trong nhà, thảm PVC", new decimal[] { 80000, 120000, 100000, 140000 }, new[] { "Cầu lông 1", "Cầu lông 2", "Cầu lông 3", "Cầu lông 4" }),
            ("Pickleball", "Sân ngoài trời có mái che", new decimal[] { 120000, 180000, 150000, 200000 }, new[] { "Pickleball 1", "Pickleball 2" }),
            ("Tennis", "Sân cứng tiêu chuẩn", new decimal[] { 200000, 300000, 250000, 350000 }, new[] { "Tennis 1", "Tennis 2" }),
        };
        var sans = new List<San>();
        foreach (var l in loais) {
            var loai = new LoaiSan { TenLoai = l.Ten, MoTa = l.MoTa };
            db.LoaiSans.Add(loai);
            foreach (var cuoiTuan in new[] { false, true }) {
                var g = cuoiTuan ? 2 : 0;
                db.BangGias.Add(new BangGia { LoaiSan = loai, KhungGio = kgThuong, LaCuoiTuan = cuoiTuan, GiaMoiGio = l.Gia[g] });
                db.BangGias.Add(new BangGia { LoaiSan = loai, KhungGio = kgVang, LaCuoiTuan = cuoiTuan, GiaMoiGio = l.Gia[g + 1] });
                db.BangGias.Add(new BangGia { LoaiSan = loai, KhungGio = kgToi, LaCuoiTuan = cuoiTuan, GiaMoiGio = l.Gia[g] });
            }
            foreach (var ten in l.Sans)
                sans.Add(new San { TenSan = ten, LoaiSan = loai, MoTa = $"{l.Ten} - {l.MoTa}" });
        }
        sans.First(s => s.TenSan == "Tennis 2").TrangThai = TrangThaiSan.BaoTri;
        db.Sans.AddRange(sans);

        //DichVu: 6 dòng
        var dvs = new[] {
            new DichVu { TenDV = "Nước suối", DonVi = "chai", DonGia = 10000 },
            new DichVu { TenDV = "Revive", DonVi = "chai", DonGia = 15000 },
            new DichVu { TenDV = "Trà đá", DonVi = "ly", DonGia = 5000 },
            new DichVu { TenDV = "Thuê vợt cầu lông", DonVi = "cây", DonGia = 30000 },
            new DichVu { TenDV = "Thuê vợt pickleball", DonVi = "cây", DonGia = 40000 },
            new DichVu { TenDV = "Thuê áo bib", DonVi = "cái", DonGia = 5000 },
        };
        db.DichVus.AddRange(dvs);
        db.SaveChanges();
    }
}