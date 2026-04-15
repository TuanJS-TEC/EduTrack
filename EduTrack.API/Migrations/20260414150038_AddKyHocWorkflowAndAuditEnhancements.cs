using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.API.Migrations
{
    /// <inheritdoc />
    public partial class AddKyHocWorkflowAndAuditEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "KyHocs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReopenedAtUtc",
                table: "KyHocs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "KyHocs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "KyHocs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Mo");

            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "AuditLogEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "AuditLogEntries",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViolationCode",
                table: "AuditLogEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KyHocWorkflowLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamHoc = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    HocKy = table.Column<byte>(type: "tinyint", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PerformedByUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BienBan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyHocWorkflowLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KyHocWorkflowLogs_KyHocs_NamHoc_HocKy",
                        columns: x => new { x.NamHoc, x.HocKy },
                        principalTable: "KyHocs",
                        principalColumns: new[] { "NamHoc", "HocKy" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KyHocWorkflowLogs_NamHoc_HocKy_AtUtc",
                table: "KyHocWorkflowLogs",
                columns: new[] { "NamHoc", "HocKy", "AtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KyHocWorkflowLogs");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "KyHocs");

            migrationBuilder.DropColumn(
                name: "ReopenedAtUtc",
                table: "KyHocs");

            migrationBuilder.DropColumn(
                name: "SubmittedAtUtc",
                table: "KyHocs");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "KyHocs");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "AuditLogEntries");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "AuditLogEntries");

            migrationBuilder.DropColumn(
                name: "ViolationCode",
                table: "AuditLogEntries");
        }
    }
}
