using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportZone.Data.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DichVu",
                columns: table => new
                {
                    MaDV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDV = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DonVi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVu", x => x.MaDV);
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKH = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SDT = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatKhauHash = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.MaKH);
                });

            migrationBuilder.CreateTable(
                name: "KhungGio",
                columns: table => new
                {
                    MaKhungGio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhungGio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GioBatDau = table.Column<TimeOnly>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhungGio", x => x.MaKhungGio);
                    table.CheckConstraint("CK_KhungGio_Gio", "GioKetThuc > GioBatDau");
                });

            migrationBuilder.CreateTable(
                name: "LoaiSan",
                columns: table => new
                {
                    MaLoai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiSan", x => x.MaLoai);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "BangGia",
                columns: table => new
                {
                    MaLoai = table.Column<int>(type: "int", nullable: false),
                    MaKhungGio = table.Column<int>(type: "int", nullable: false),
                    LaCuoiTuan = table.Column<bool>(type: "bit", nullable: false),
                    GiaMoiGio = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangGia", x => new { x.MaLoai, x.MaKhungGio, x.LaCuoiTuan });
                    table.CheckConstraint("CK_BangGia_Gia", "GiaMoiGio >= 0");
                    table.ForeignKey(
                        name: "FK_BangGia_KhungGio_MaKhungGio",
                        column: x => x.MaKhungGio,
                        principalTable: "KhungGio",
                        principalColumn: "MaKhungGio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BangGia_LoaiSan_MaLoai",
                        column: x => x.MaLoai,
                        principalTable: "LoaiSan",
                        principalColumn: "MaLoai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "San",
                columns: table => new
                {
                    MaSan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenSan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaLoai = table.Column<int>(type: "int", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_San", x => x.MaSan);
                    table.ForeignKey(
                        name: "FK_San_LoaiSan_MaLoai",
                        column: x => x.MaLoai,
                        principalTable: "LoaiSan",
                        principalColumn: "MaLoai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauHash = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.MaNV);
                    table.ForeignKey(
                        name: "FK_NhanVien_VaiTro_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "VaiTro",
                        principalColumn: "MaVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichDinhKy",
                columns: table => new
                {
                    MaLich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKH = table.Column<int>(type: "int", nullable: false),
                    MaSan = table.Column<int>(type: "int", nullable: false),
                    ThuTrongTuan = table.Column<byte>(type: "tinyint", nullable: false),
                    GioBatDau = table.Column<TimeOnly>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    TuNgay = table.Column<DateOnly>(type: "date", nullable: false),
                    DenNgay = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichDinhKy", x => x.MaLich);
                    table.CheckConstraint("CK_LichDinhKy_Ngay", "DenNgay >= TuNgay");
                    table.CheckConstraint("CK_LichDinhKy_Thu", "ThuTrongTuan BETWEEN 1 AND 7");
                    table.ForeignKey(
                        name: "FK_LichDinhKy_KhachHang_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichDinhKy_San_MaSan",
                        column: x => x.MaSan,
                        principalTable: "San",
                        principalColumn: "MaSan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DatSan",
                columns: table => new
                {
                    MaDatSan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKH = table.Column<int>(type: "int", nullable: false),
                    MaSan = table.Column<int>(type: "int", nullable: false),
                    NgaySuDung = table.Column<DateOnly>(type: "date", nullable: false),
                    GioBatDau = table.Column<TimeOnly>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    TienSan = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TienCoc = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaNVXacNhan = table.Column<int>(type: "int", nullable: true),
                    MaLichDinhKy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatSan", x => x.MaDatSan);
                    table.CheckConstraint("CK_DatSan_Gio", "GioKetThuc > GioBatDau");
                    table.ForeignKey(
                        name: "FK_DatSan_KhachHang_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatSan_LichDinhKy_MaLichDinhKy",
                        column: x => x.MaLichDinhKy,
                        principalTable: "LichDinhKy",
                        principalColumn: "MaLich",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatSan_NhanVien_MaNVXacNhan",
                        column: x => x.MaNVXacNhan,
                        principalTable: "NhanVien",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DatSan_San_MaSan",
                        column: x => x.MaSan,
                        principalTable: "San",
                        principalColumn: "MaSan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDichVu",
                columns: table => new
                {
                    MaDatSan = table.Column<int>(type: "int", nullable: false),
                    MaDV = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDichVu", x => new { x.MaDatSan, x.MaDV });
                    table.CheckConstraint("CK_ChiTietDichVu_SoLuong", "SoLuong > 0");
                    table.ForeignKey(
                        name: "FK_ChiTietDichVu_DatSan_MaDatSan",
                        column: x => x.MaDatSan,
                        principalTable: "DatSan",
                        principalColumn: "MaDatSan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietDichVu_DichVu_MaDV",
                        column: x => x.MaDV,
                        principalTable: "DichVu",
                        principalColumn: "MaDV",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHD = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDatSan = table.Column<int>(type: "int", nullable: false),
                    MaNV = table.Column<int>(type: "int", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    PhuongThucTT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.MaHD);
                    table.ForeignKey(
                        name: "FK_HoaDon_DatSan_MaDatSan",
                        column: x => x.MaDatSan,
                        principalTable: "DatSan",
                        principalColumn: "MaDatSan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDon_NhanVien_MaNV",
                        column: x => x.MaNV,
                        principalTable: "NhanVien",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangGia_MaKhungGio",
                table: "BangGia",
                column: "MaKhungGio");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDichVu_MaDV",
                table: "ChiTietDichVu",
                column: "MaDV");

            migrationBuilder.CreateIndex(
                name: "IX_DatSan_MaKH",
                table: "DatSan",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_DatSan_MaLichDinhKy",
                table: "DatSan",
                column: "MaLichDinhKy");

            migrationBuilder.CreateIndex(
                name: "IX_DatSan_MaNVXacNhan",
                table: "DatSan",
                column: "MaNVXacNhan");

            migrationBuilder.CreateIndex(
                name: "IX_DatSan_MaSan_NgaySuDung_TrangThai",
                table: "DatSan",
                columns: new[] { "MaSan", "NgaySuDung", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaDatSan",
                table: "HoaDon",
                column: "MaDatSan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaNV",
                table: "HoaDon",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_SDT",
                table: "KhachHang",
                column: "SDT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichDinhKy_MaKH",
                table: "LichDinhKy",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_LichDinhKy_MaSan",
                table: "LichDinhKy",
                column: "MaSan");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaVaiTro",
                table: "NhanVien",
                column: "MaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_TenDangNhap",
                table: "NhanVien",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_San_MaLoai",
                table: "San",
                column: "MaLoai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangGia");

            migrationBuilder.DropTable(
                name: "ChiTietDichVu");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "KhungGio");

            migrationBuilder.DropTable(
                name: "DichVu");

            migrationBuilder.DropTable(
                name: "DatSan");

            migrationBuilder.DropTable(
                name: "LichDinhKy");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "San");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "LoaiSan");
        }
    }
}
