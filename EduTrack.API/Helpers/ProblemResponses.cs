using Microsoft.AspNetCore.Mvc;

namespace EduTrack.API.Helpers;

public static class ProblemResponses
{
    public static ObjectResult Of(int statusCode, string title, string? code = null)
    {
        var pd = new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };
        if (code is not null) pd.Extensions["code"] = code;
        return new ObjectResult(pd) { StatusCode = statusCode };
    }
}
