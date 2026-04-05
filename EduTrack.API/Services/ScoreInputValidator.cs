namespace EduTrack.API.Services;

public static class ScoreInputValidator
{
    public static string? Validate(decimal? value)
    {
        if (value is null) return null;
        if (value < 0m || value > 10m) return "Điểm phải trong khoảng 0–10.";
        return null;
    }

    public static string? ValidateMany(IEnumerable<decimal>? values)
    {
        if (values is null) return null;
        foreach (var v in values)
        {
            var e = Validate(v);
            if (e is not null) return e;
        }

        return null;
    }
}
