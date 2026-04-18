using Domain.Enums;

namespace Application.Areas;

public interface IAreaSessionState
{
    decimal HourlyPrice { get; }

    string CustomerName { get; set; }

    string PhoneNumber { get; set; }

    string? MemberId { get; set; }

    DateTime? CheckInDateTime { get; set; }

    int Capacity { get; set; }

    DateTime? StartTime { get; set; }

    bool IsSessionPaused { get; set; }

    DateTime? SessionPausedAt { get; set; }

    TimeSpan SessionPausedDuration { get; set; }

    decimal TotalAmount { get; set; }

    PlayAreaStatus Status { get; set; }
}
