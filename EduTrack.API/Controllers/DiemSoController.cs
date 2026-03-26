using EduTrack.API.Data;
using EduTrack.API.DTOs;
using EduTrack.API.Models;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Controllers;

[ApiController]
[Route("api/diemso")]
[Authorize]
public sealed class DiemSoController(EduTrackDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DiemSo>>> GetAll([FromQuery] string? maHS, [FromQuery] string? maMon, [FromQuery] byte? hocKy)
    {
        var q = db.DiemSos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(maHS)) q = q.Where(x => x.MaHS == maHS);
        if (!string.IsNullOrWhiteSpace(maMon)) q = q.Where(x => x.MaMon == maMon);
        if (hocKy.HasValue) q = q.Where(x => x.HocKy == hocKy.Value);

        return Ok(await q.OrderBy(x => x.MaHS).ThenBy(x => x.MaMon).ToListAsync());
    }

    [HttpGet("{maDiem:int}")]
    public async Task<ActionResult<DiemSo>> GetById([FromRoute] int maDiem)
    {
        var item = await db.DiemSos.AsNoTracking().FirstOrDefaultAsync(x => x.MaDiem == maDiem);
        return item is null ? NotFound() : Ok(item);
    }

    // Upsert theo unique index (MaHS, MaMon, HocKy)
    [HttpPost("upsert")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<DiemSo>> Upsert([FromBody] DiemSoUpsertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.MaHS) || string.IsNullOrWhiteSpace(req.MaMon))
            return BadRequest(new { message = "MaHS/MaMon không hợp lệ" });

        var existsHS = await db.HocSinhs.AnyAsync(x => x.MaHS == req.MaHS);
        if (!existsHS) return BadRequest(new { message = "HocSinh không tồn tại" });

        var existsMon = await db.MonHocs.AnyAsync(x => x.MaMon == req.MaMon);
        if (!existsMon) return BadRequest(new { message = "MonHoc không tồn tại" });

        var item = await db.DiemSos.FirstOrDefaultAsync(x => x.MaHS == req.MaHS && x.MaMon == req.MaMon && x.HocKy == req.HocKy);
        if (item is null)
        {
            item = new DiemSo { MaHS = req.MaHS, MaMon = req.MaMon, HocKy = req.HocKy };
            db.DiemSos.Add(item);
        }

        item.DiemMieng = req.DiemMieng;
        item.Diem15p = req.Diem15p;
        item.DiemGiuaKy = req.DiemGiuaKy;
        item.DiemCuoiKy = req.DiemCuoiKy;
        item.DiemTBMon = GradeCalculator.CalcTbMon(req.DiemMieng, req.Diem15p, req.DiemGiuaKy, req.DiemCuoiKy);

        await db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{maDiem:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> Delete([FromRoute] int maDiem)
    {
        var item = await db.DiemSos.FirstOrDefaultAsync(x => x.MaDiem == maDiem);
        if (item is null) return NotFound();

        db.DiemSos.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Xuất bảng điểm (JSON) theo lớp/môn/học kỳ
    [HttpGet("bangdiem")]
    public async Task<ActionResult<List<BangDiemItemResponse>>> BangDiem([FromQuery] string maLop, [FromQuery] string maMon, [FromQuery] byte hocKy)
    {
        if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(maMon))
            return BadRequest(new { message = "Thiếu maLop/maMon" });

        var rows = await (
            from hs in db.HocSinhs.AsNoTracking()
            join mh in db.MonHocs.AsNoTracking() on maMon equals mh.MaMon
            where hs.MaLop == maLop
            join ds in db.DiemSos.AsNoTracking().Where(x => x.MaMon == maMon && x.HocKy == hocKy)
                on hs.MaHS equals ds.MaHS into g
            from ds in g.DefaultIfEmpty()
            select new BangDiemItemResponse
            {
                MaHS = hs.MaHS,
                HoTen = hs.HoTen,
                MaLop = hs.MaLop,
                MaMon = mh.MaMon,
                TenMon = mh.TenMon,
                HocKy = hocKy,
                DiemMieng = ds.DiemMieng,
                Diem15p = ds.Diem15p,
                DiemGiuaKy = ds.DiemGiuaKy,
                DiemCuoiKy = ds.DiemCuoiKy,
                DiemTBMon = ds.DiemTBMon,
                XepLoai = GradeCalculator.XepLoai(ds.DiemTBMon)
            }
        ).OrderBy(x => x.HoTen).ToListAsync();

        return Ok(rows);
    }
}

