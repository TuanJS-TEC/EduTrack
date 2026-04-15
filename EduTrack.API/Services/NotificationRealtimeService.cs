using EduTrack.API.Hubs;
using EduTrack.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace EduTrack.API.Services;

public sealed class NotificationRealtimeService(IHubContext<NotificationHub> hub) : INotificationRealtimeService
{
    public Task NotifyCreatedAsync(ThongBao item, CancellationToken ct = default)
        => NotifyAsync("notification_created", item, ct);

    public Task NotifyUpdatedAsync(ThongBao item, CancellationToken ct = default)
        => NotifyAsync("notification_updated", item, ct);

    public Task NotifyReadAsync(ThongBao item, CancellationToken ct = default)
        => NotifyAsync("notification_read", item, ct);

    public Task NotifyDeletedAsync(int maTB, string? maHS, CancellationToken ct = default)
        => NotifyAsync("notification_deleted", new { MaTB = maTB, MaHS = maHS }, ct);

    private Task NotifyAsync(string eventName, object payload, CancellationToken ct)
    {
        if (payload is ThongBao item && !string.IsNullOrWhiteSpace(item.MaHS))
        {
            return hub.Clients.Group(NotificationHub.StudentGroup(item.MaHS))
                .SendCoreAsync(eventName, [payload], ct);
        }

        return hub.Clients.Group(NotificationHub.GlobalGroup)
            .SendCoreAsync(eventName, [payload], ct);
    }
}
