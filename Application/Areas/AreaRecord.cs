using Domain.Enums;

namespace Application.Areas;

public sealed record AreaRecord
{
    public string AreaName { get; init; } = string.Empty;

    public PlayAreaType PlayAreaType { get; init; } = PlayAreaType.Table;

    public PlayAreaStatus Status { get; init; } = PlayAreaStatus.Available;

    public int MaxCapacity { get; init; }

    public decimal HourlyPrice { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string? MemberId { get; init; }

    public DateTime? CheckInDateTime { get; init; }

    public int Capacity { get; init; }

    public DateTime? StartTime { get; init; }

    public bool IsSessionPaused { get; init; }

    public DateTime? SessionPausedAt { get; init; }

    public TimeSpan SessionPausedDuration { get; init; }

    public decimal TotalAmount { get; init; }
}
