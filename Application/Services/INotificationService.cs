namespace Application.Services;

public enum NotificationType
{
    Success,
    Info,
    Warning,
    Error,
}

public interface INotificationService
{
    Task SendAsync(string title, string message, NotificationType type = NotificationType.Info);
}
