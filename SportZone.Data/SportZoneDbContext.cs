using Microsoft.EntityFrameworkCore;
using SportZone.Data.Entities;

namespace SportZone.Data;

public class SportZoneDbContext : DbContext
{
    public SportZoneDbContext(DbContextOptions<SportZoneDbContext> options) : base(options) { }

    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<LoaiSan> LoaiSans => Set<LoaiSan>();
    public DbSet<San> Sans => Set<San>();
    public DbSet<KhungGio> KhungGios => Set<KhungGio>();
    public DbSet<BangGia> BangGias => Set<BangGia>();
    public DbSet<DatSan> DatSans => Set<DatSan>();
    public DbSet<LichDinhKy> LichDinhKys => Set<LichDinhKy>();
    public DbSet<DichVu> DichVus => Set<DichVu>();
    public DbSet<ChiTietDichVu> ChiTietDichVus => Set<ChiTietDichVu>();
    public DbSet<HoaDon> HoaDons => Set<HoaDon>();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<decimal>().HavePrecision(18, 0);
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<VaiTro>(e =>
        {
            e.ToTable("VaiTro");
            e.HasKey(x => x.MaVaiTro);
            e.Property(x => x.TenVaiTro).HasMaxLength(50).IsRequired();
        });

        mb.Entity<NhanVien>(e =>
        {
            e.ToTable("NhanVien");
            e.HasKey(x => x.MaNV);
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.TenDangNhap).IsUnique();
            e.Property(x => x.MatKhauHash).HasMaxLength(100).IsUnicode(false).IsRequired();
            e.Property(x => x.TrangThai).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.VaiTro).WithMany(v => v.NhanViens).HasForeignKey(x => x.MaVaiTro);
        });

        mb.Entity<KhachHang>(e =>
        {
            e.ToTable("KhachHang");
            e.HasKey(x => x.MaKH);
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.SDT).HasMaxLength(15).IsUnicode(false).IsRequired();
            e.HasIndex(x => x.SDT).IsUnique();
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.MatKhauHash).HasMaxLength(100).IsUnicode(false);
        });

        mb.Entity<LoaiSan>(e =>
        {
            e.ToTable("LoaiSan");
            e.HasKey(x => x.MaLoai);
            e.Property(x => x.TenLoai).HasMaxLength(50).IsRequired();
            e.Property(x => x.MoTa).HasMaxLength(500);
        });

        mb.Entity<San>(e =>
        {
            e.ToTable("San");
            e.HasKey(x => x.MaSan);
            e.Property(x => x.TenSan).HasMaxLength(50).IsRequired();
            e.Property(x => x.HinhAnh).HasMaxLength(255);
            e.Property(x => x.MoTa).HasMaxLength(500);
            e.Property(x => x.TrangThai).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.LoaiSan).WithMany(l => l.Sans).HasForeignKey(x => x.MaLoai);
        });

        mb.Entity<KhungGio>(e =>
        {
            e.ToTable("KhungGio", t => t.HasCheckConstraint("CK_KhungGio_Gio", "GioKetThuc > GioBatDau"));
            e.HasKey(x => x.MaKhungGio);
            e.Property(x => x.TenKhungGio).HasMaxLength(50).IsRequired();
        });

        mb.Entity<BangGia>(e =>
        {
            e.ToTable("BangGia", t => t.HasCheckConstraint("CK_BangGia_Gia", "GiaMoiGio >= 0"));
            e.HasKey(x => new { x.MaLoai, x.MaKhungGio, x.LaCuoiTuan });
            e.HasOne(x => x.LoaiSan).WithMany(l => l.BangGias).HasForeignKey(x => x.MaLoai);
            e.HasOne(x => x.KhungGio).WithMany().HasForeignKey(x => x.MaKhungGio);
        });

        mb.Entity<DatSan>(e =>
        {
            e.ToTable("DatSan", t => t.HasCheckConstraint("CK_DatSan_Gio", "GioKetThuc > GioBatDau"));
            e.HasKey(x => x.MaDatSan);
            e.Property(x => x.TrangThai).HasMaxLength(20).IsRequired();
            e.Ignore(x => x.ThoiDiemBatDau);
            e.Ignore(x => x.SoGio);
            // Phục vụ kiểm tra trùng lịch: cùng sân, cùng ngày, đơn chưa hủy
            e.HasIndex(x => new { x.MaSan, x.NgaySuDung, x.TrangThai });
            e.HasOne(x => x.KhachHang).WithMany(k => k.DatSans).HasForeignKey(x => x.MaKH);
            e.HasOne(x => x.San).WithMany().HasForeignKey(x => x.MaSan);
            e.HasOne(x => x.NhanVienXacNhan).WithMany().HasForeignKey(x => x.MaNVXacNhan);
            e.HasOne(x => x.LichDinhKy).WithMany().HasForeignKey(x => x.MaLichDinhKy);
        });

        mb.Entity<LichDinhKy>(e =>
        {
            e.ToTable("LichDinhKy", t =>
            {
                t.HasCheckConstraint("CK_LichDinhKy_Thu", "ThuTrongTuan BETWEEN 1 AND 7");
                t.HasCheckConstraint("CK_LichDinhKy_Ngay", "DenNgay >= TuNgay");
            });
            e.HasKey(x => x.MaLich);
            e.HasOne(x => x.KhachHang).WithMany().HasForeignKey(x => x.MaKH);
            e.HasOne(x => x.San).WithMany().HasForeignKey(x => x.MaSan);
        });

        mb.Entity<DichVu>(e =>
        {
            e.ToTable("DichVu");
            e.HasKey(x => x.MaDV);
            e.Property(x => x.TenDV).HasMaxLength(100).IsRequired();
            e.Property(x => x.DonVi).HasMaxLength(20).IsRequired();
        });

        mb.Entity<ChiTietDichVu>(e =>
        {
            e.ToTable("ChiTietDichVu", t => t.HasCheckConstraint("CK_ChiTietDichVu_SoLuong", "SoLuong > 0"));
            e.HasKey(x => new { x.MaDatSan, x.MaDV });
            e.HasOne(x => x.DatSan).WithMany(d => d.ChiTietDichVus).HasForeignKey(x => x.MaDatSan);
            e.HasOne(x => x.DichVu).WithMany().HasForeignKey(x => x.MaDV);
        });

        mb.Entity<HoaDon>(e =>
        {
            e.ToTable("HoaDon");
            e.HasKey(x => x.MaHD);
            e.HasIndex(x => x.MaDatSan).IsUnique();
            e.Property(x => x.PhuongThucTT).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.DatSan).WithOne(d => d.HoaDon).HasForeignKey<HoaDon>(x => x.MaDatSan);
            e.HasOne(x => x.NhanVien).WithMany().HasForeignKey(x => x.MaNV);
        });

        // Không xóa dây chuyền: muốn xóa sân, dịch vụ, khách đã có đơn thì phải xử lý đơn trước
        foreach (var fk in mb.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
