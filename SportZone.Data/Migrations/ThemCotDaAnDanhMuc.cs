﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportZone.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemCotDaAnDanhMuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaAn",
                table: "San",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaAn",
                table: "LoaiSan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaAn",
                table: "KhungGio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DaAn",
                table: "DichVu",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaAn",
                table: "San");

            migrationBuilder.DropColumn(
                name: "DaAn",
                table: "LoaiSan");

            migrationBuilder.DropColumn(
                name: "DaAn",
                table: "KhungGio");

            migrationBuilder.DropColumn(
                name: "DaAn",
                table: "DichVu");
        }
    }
}
