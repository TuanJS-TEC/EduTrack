using EduTrack.API.Models;

namespace EduTrack.API.Services;

public interface INotificationRealtimeService
{
    Task NotifyCreatedAsync(ThongBao item, CancellationToken ct = default);
    Task NotifyUpdatedAsync(ThongBao item, CancellationToken ct = default);
    Task NotifyReadAsync(ThongBao item, CancellationToken ct = default);
    Task NotifyDeletedAsync(int maTB, string? maHS, CancellationToken ct = default);
}
