using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportZone.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemTrangThaiKhachHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "KhachHang",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Hoạt động");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "KhachHang");
        }
    }
}
