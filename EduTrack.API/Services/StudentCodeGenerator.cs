using EduTrack.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Services;

public static class StudentCodeGenerator
{
    public static async Task<string> NextAsync(EduTrackDbContext db, int enrollmentYear, CancellationToken ct = default)
    {
        var prefix = $"HS-{enrollmentYear}-";
        var existing = await db.HocSinhs.AsNoTracking()
            .Where(h => h.MaHS.StartsWith(prefix))
            .Select(h => h.MaHS)
            .ToListAsync(ct);

        var max = 0;
        foreach (var ma in existing)
        {
            var parts = ma.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && int.TryParse(parts[2], out var n)) max = Math.Max(max, n);
        }

        return $"{prefix}{(max + 1):D5}";
    }
}
