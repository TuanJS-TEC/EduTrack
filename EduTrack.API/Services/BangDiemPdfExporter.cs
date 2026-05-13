using EduTrack.API.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduTrack.API.Services;

public static class BangDiemPdfExporter
{
    public static byte[] Build(
        IReadOnlyList<BangDiemItemResponse> rows,
        string maLop,
        string maMon,
        string tenMon,
        string namHoc,
        byte hocKy)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(8.5f));

                page.Header().Column(col =>
                {
                    col.Item().Text("Bảng điểm").FontSize(15).SemiBold();
                    col.Item().PaddingTop(3).Text($"{tenMon} ({maMon}) — Lớp {maLop} — Năm học {namHoc} — Học kỳ {hocKy}");
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(68);
                        c.RelativeColumn(2f);
                        c.RelativeColumn(1.4f);
                        c.RelativeColumn(1.4f);
                        c.ConstantColumn(28);
                        c.ConstantColumn(28);
                        c.ConstantColumn(32);
                        c.RelativeColumn(0.9f);
                        c.RelativeColumn(0.9f);
                    });

                    table.Header(h =>
                    {
                        static IContainer Hd(IContainer x) =>
                            x.Background(Colors.Grey.Lighten3).Padding(3).Border(0.5f).BorderColor(Colors.Grey.Lighten1);

                        h.Cell().Element(Hd).Text("Mã HS");
                        h.Cell().Element(Hd).Text("Họ tên");
                        h.Cell().Element(Hd).Text("Miệng");
                        h.Cell().Element(Hd).Text("15p");
                        h.Cell().Element(Hd).Text("GK");
                        h.Cell().Element(Hd).Text("CK");
                        h.Cell().Element(Hd).Text("TBM");
                        h.Cell().Element(Hd).Text("XL");
                        h.Cell().Element(Hd).Text("TT");
                    });

                    foreach (var row in rows)
                    {
                        static IContainer Bd(IContainer x) =>
                            x.Padding(3).Border(0.5f).BorderColor(Colors.Grey.Lighten2);

                        table.Cell().Element(Bd).Text(row.MaHS);
                        table.Cell().Element(Bd).Text(row.HoTen);
                        table.Cell().Element(Bd).Text(string.Join(", ", row.DiemMiengs));
                        table.Cell().Element(Bd).Text(string.Join(", ", row.Diem15ps));
                        table.Cell().Element(Bd).Text(row.DiemGiuaKy?.ToString("0.##") ?? "");
                        table.Cell().Element(Bd).Text(row.DiemCuoiKy?.ToString("0.##") ?? "");
                        table.Cell().Element(Bd).Text(row.DiemTBMon?.ToString("0.##") ?? "");
                        table.Cell().Element(Bd).Text(row.XepLoai ?? "");
                        table.Cell().Element(Bd).Text(row.TrangThaiNhapDiem);
                    }
                });
            });
        }).GeneratePdf();
    }
}
