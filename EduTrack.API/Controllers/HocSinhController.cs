using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/hocsinh")]
[Authorize]
public sealed class HocSinhController(EduTrackDbContext db) : ControllerBase
{
    // Helper: tính DiemTB trung bình tất cả môn (trung bình các DiemTBMon có giá trị)
    private static decimal? TinhDiemTB(IEnumerable<DiemSo> diemSos)
    {
        var vals = diemSos
            .Where(d => d.DiemTBMon.HasValue)
            .Select(d => d.DiemTBMon!.Value)
            .ToList();
        if (!vals.Any()) return null;
        return Math.Round(vals.Average(), 1, MidpointRounding.AwayFromZero);
    }

    // Helper: xếp hạnh kiểm theo điểm TB
    private static string? XepHanhKiem(decimal? tb)
    {
        if (tb is null) return null;
        if (tb >= 8.0m) return "Tốt";
        if (tb >= 6.5m) return "Khá";
        if (tb >= 5.0m) return "Trung bình";
        return "Yếu";
    }

    // Map entity → DTO
    private static HocSinhResponse ToDto(HocSinh hs)
    {
        var diemTB = TinhDiemTB(hs.DiemSos);
        return new HocSinhResponse
        {
            MaHS           = hs.MaHS,
            HoTen          = hs.HoTen,
            NgaySinh       = hs.NgaySinh,
            DiaChi         = hs.DiaChi,
            MaLop          = hs.MaLop,
            Email_PhuHuynh = hs.Email_PhuHuynh,
            SDT_PhuHuynh   = hs.SDT_PhuHuynh,
            TrangThai      = hs.TrangThai,
            DiemTB         = diemTB,
            HanhKiem       = XepHanhKiem(diemTB),
        };
    }

    [HttpGet]
    public async Task<ActionResult<List<HocSinhResponse>>> GetAll([FromQuery] string? maLop)
    {
        var query = db.HocSinhs
            .AsNoTracking()
            .Include(h => h.DiemSos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(maLop))
            query = query.Where(x => x.MaLop == maLop);

        var list = await query.OrderBy(x => x.HoTen).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{maHS}")]
    public async Task<ActionResult<HocSinhResponse>> GetById([FromRoute] string maHS)
    {
        var hs = await db.HocSinhs
            .AsNoTracking()
            .Include(h => h.DiemSos)
            .FirstOrDefaultAsync(x => x.MaHS == maHS);
        return hs is null ? NotFound() : Ok(ToDto(hs));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Create([FromBody] HocSinh hs)
    {
        if (await db.HocSinhs.AnyAsync(x => x.MaHS == hs.MaHS))
            return Conflict(new { message = "MaHS đã tồn tại" });

        db.HocSinhs.Add(hs);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { maHS = hs.MaHS }, ToDto(hs));
    }

    [HttpPut("{maHS}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Update([FromRoute] string maHS, [FromBody] HocSinh input)
    {
        var hs = await db.HocSinhs.FirstOrDefaultAsync(x => x.MaHS == maHS);
        if (hs is null) return NotFound();

        hs.HoTen          = input.HoTen;
        hs.NgaySinh       = input.NgaySinh;
        hs.DiaChi         = input.DiaChi;
        hs.MaLop          = input.MaLop;
        hs.Email_PhuHuynh = input.Email_PhuHuynh;
        hs.SDT_PhuHuynh   = input.SDT_PhuHuynh;
        hs.TrangThai      = input.TrangThai ?? "Đang học";

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{maHS}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([FromRoute] string maHS)
    {
        var hs = await db.HocSinhs.FirstOrDefaultAsync(x => x.MaHS == maHS);
        if (hs is null) return NotFound();

        db.HocSinhs.Remove(hs);
        await db.SaveChangesAsync();
        return NoContent();
    }
}


