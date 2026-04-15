using System.Security.Claims;
using EduTrack.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.API.Hubs;

[Authorize]
public sealed class NotificationHub(EduTrackDbContext db) : Hub
{
    public const string HubPath = "/hubs/notifications";
    public const string GlobalGroup = "notifications:all";

    public static string StudentGroup(string maHS) => $"notifications:student:{maHS}";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            await base.OnConnectedAsync();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GlobalGroup);

        var studentCodes = await db.ParentStudentLinks
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.MaHS)
            .ToListAsync();

        foreach (var maHS in studentCodes)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, StudentGroup(maHS));
        }

        await base.OnConnectedAsync();
    }
}
