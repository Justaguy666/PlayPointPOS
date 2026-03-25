using System;
using System.Threading.Tasks;
using Application.Services;

namespace Infrastructure.Services.Notification;

public class ToastNotificationService : INotificationService
{
    public event Action<string, string, NotificationType>? NotificationRequested;

    public Task SendAsync(string title, string message, NotificationType type = NotificationType.Info)
    {
        NotificationRequested?.Invoke(title, message, type);
        return Task.CompletedTask;
    }
}
