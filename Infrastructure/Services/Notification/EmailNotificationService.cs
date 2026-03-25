using Application.Services;

namespace Infrastructure.Services.Notification;

/// <summary>
/// Stub email notification service for future implementation.
/// Replace with actual SMTP/SendGrid integration when ready.
/// </summary>
public class EmailNotificationService : INotificationService
{
    public Task SendAsync(string title, string message, NotificationType type = NotificationType.Info)
    {
        // TODO: Implement email sending (SMTP, SendGrid, etc.)
        // Example: await _emailClient.SendAsync(recipient, title, message);
        return Task.CompletedTask;
    }
}
