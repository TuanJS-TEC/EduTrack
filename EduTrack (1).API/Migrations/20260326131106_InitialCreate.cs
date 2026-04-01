using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiaoViens",
                columns: table => new
                {
                    MaGV = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LuongCoBan = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoViens", x => x.MaGV);
                });

            migrationBuilder.CreateTable(
                name: "LopHocs",
                columns: table => new
                {
                    MaLop = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenLop = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KhoiLop = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NamHoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaGVChuNhiem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocs", x => x.MaLop);
                    table.ForeignKey(
                        name: "FK_LopHocs_GiaoViens_MaGVChuNhiem",
                        column: x => x.MaGVChuNhiem,
                        principalTable: "GiaoViens",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MonHocs",
                columns: table => new
                {
                    MaMon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoTiet = table.Column<int>(type: "int", nullable: true),
                    HeSoThi = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    MaGV = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocs", x => x.MaMon);
                    table.ForeignKey(
                        name: "FK_MonHocs_GiaoViens_MaGV",
                        column: x => x.MaGV,
                        principalTable: "GiaoViens",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HocSinhs",
                columns: table => new
                {
                    MaHS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaLop = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email_PhuHuynh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SDT_PhuHuynh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocSinhs", x => x.MaHS);
                    table.ForeignKey(
                        name: "FK_HocSinhs_LopHocs_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopHocs",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichHocs",
                columns: table => new
                {
                    MaLich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaMon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaLop = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaGV = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Thu = table.Column<byte>(type: "tinyint", nullable: true),
                    TietBD = table.Column<int>(type: "int", nullable: true),
                    TietKT = table.Column<int>(type: "int", nullable: true),
                    Phong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHocs", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK_LichHocs_GiaoViens_MaGV",
                        column: x => x.MaGV,
                        principalTable: "GiaoViens",
                        principalColumn: "MaGV",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LichHocs_LopHocs_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopHocs",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichHocs_MonHocs_MaMon",
                        column: x => x.MaMon,
                        principalTable: "MonHocs",
                        principalColumn: "MaMon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiemSos",
                columns: table => new
                {
                    MaDiem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaMon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HocKy = table.Column<byte>(type: "tinyint", nullable: false),
                    DiemMieng = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    Diem15p = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    DiemGiuaKy = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    DiemCuoiKy = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    DiemTBMon = table.Column<decimal>(type: "decimal(4,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemSos", x => x.MaDiem);
                    table.ForeignKey(
                        name: "FK_DiemSos_HocSinhs_MaHS",
                        column: x => x.MaHS,
                        principalTable: "HocSinhs",
                        principalColumn: "MaHS",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiemSos_MonHocs_MaMon",
                        column: x => x.MaMon,
                        principalTable: "MonHocs",
                        principalColumn: "MaMon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HocPhis",
                columns: table => new
                {
                    MaHocPhi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HocKy = table.Column<byte>(type: "tinyint", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayDong = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocPhis", x => x.MaHocPhi);
                    table.ForeignKey(
                        name: "FK_HocPhis_HocSinhs_MaHS",
                        column: x => x.MaHS,
                        principalTable: "HocSinhs",
                        principalColumn: "MaHS",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    MaTB = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoaiTB = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaHS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.MaTB);
                    table.ForeignKey(
                        name: "FK_ThongBaos_HocSinhs_MaHS",
                        column: x => x.MaHS,
                        principalTable: "HocSinhs",
                        principalColumn: "MaHS",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiemSos_MaHS_MaMon_HocKy",
                table: "DiemSos",
                columns: new[] { "MaHS", "MaMon", "HocKy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemSos_MaMon",
                table: "DiemSos",
                column: "MaMon");

            migrationBuilder.CreateIndex(
                name: "IX_HocPhis_MaHS",
                table: "HocPhis",
                column: "MaHS");

            migrationBuilder.CreateIndex(
                name: "IX_HocSinhs_MaLop",
                table: "HocSinhs",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_MaGV",
                table: "LichHocs",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_MaLop",
                table: "LichHocs",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_MaMon",
                table: "LichHocs",
                column: "MaMon");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_MaGVChuNhiem",
                table: "LopHocs",
                column: "MaGVChuNhiem");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocs_MaGV",
                table: "MonHocs",
                column: "MaGV");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_MaHS",
                table: "ThongBaos",
                column: "MaHS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiemSos");

            migrationBuilder.DropTable(
                name: "HocPhis");

            migrationBuilder.DropTable(
                name: "LichHocs");

            migrationBuilder.DropTable(
                name: "ThongBaos");

            migrationBuilder.DropTable(
                name: "MonHocs");

            migrationBuilder.DropTable(
                name: "HocSinhs");

            migrationBuilder.DropTable(
                name: "LopHocs");

            migrationBuilder.DropTable(
                name: "GiaoViens");
        }
    }
}
