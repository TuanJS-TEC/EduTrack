using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class AddThongBaoReadStateAndFilterIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThongBaos_MaHS",
                table: "ThongBaos");

            migrationBuilder.AddColumn<bool>(
                name: "DaDoc",
                table: "ThongBaos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDoc",
                table: "ThongBaos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_MaHS_LoaiTB_DaDoc_NgayGui",
                table: "ThongBaos",
                columns: new[] { "MaHS", "LoaiTB", "DaDoc", "NgayGui" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThongBaos_MaHS_LoaiTB_DaDoc_NgayGui",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "DaDoc",
                table: "ThongBaos");

            migrationBuilder.DropColumn(
                name: "NgayDoc",
                table: "ThongBaos");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_MaHS",
                table: "ThongBaos",
                column: "MaHS");
        }
    }
}
